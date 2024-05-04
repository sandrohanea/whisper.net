// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;

namespace Whisper.net.Wave;

public sealed class WaveParser {
  private static readonly byte[] expectedSubFormatForPcm =
      new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00,
                   0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71 };
  private readonly Stream waveStream;
  private ushort channels;
  private uint sampleRate;
  private ushort bitsPerSample;
  private uint dataChunkSize;
  private long dataChunkPosition;
  private bool isInitialized;

  public WaveParser(Stream waveStream) { this.waveStream = waveStream; }

  /// <summary>
  /// Gets the number of channels in the current wave file.
  /// </summary>
  /// <remarks>
  /// It is populated only after the initialization.
  /// </remarks>
  public ushort Channels => channels;

  /// <summary>
  /// Gets the Sample Rate in the current wave file.
  /// </summary>
  /// <remarks>
  /// It is populated only after the initialization.
  /// </remarks>
  public uint SampleRate => sampleRate;

  /// <summary>
  /// Gets the Bits Per Sample in the current wave file.
  /// </summary>
  /// <remarks>
  /// It is populated only after the initialization.
  /// </remarks>
  public ushort BitsPerSample => bitsPerSample;

  /// <summary>
  /// Gets the size of the data chunk in the current wave file.
  /// </summary>
  /// <remarks>
  /// It is populated only after the initialization.
  /// </remarks>
  public uint DataChunkSize => dataChunkSize;

  /// <summary>
  /// Gets the position of the data chunk in the current wave file.
  /// </summary>
  /// <remarks>
  /// It is populated only after the initialization.
  /// </remarks>
  public long DataChunkPosition => dataChunkPosition;

  /// <summary>
  /// Gets a value indicating whether the wave parser is initialized.
  /// </summary>
  public bool IsInitialized => isInitialized;

  /// <summary>
  /// Gets the number of samples for each channel in the current wave file.
  /// </summary>
  /// <remarks>
  /// It is populated only after the initialization.
  /// </remarks>
  public long SamplesCount => dataChunkSize / (bitsPerSample / 8) / channels;

  /// <summary>
  /// Gets the size of a single frame in the current wave file.
  /// </summary>
  /// <remarks>
  /// It is populated only after the initialization and it is equal to <see
  /// cref="BitsPerSample"/> / 8 * <see cref="Channels"/>.
  /// </remarks>
  public int FrameSize => bitsPerSample / 8 * channels;

  /// <summary>
  /// Gets the value to divide the sample by to get the actual float value.
  /// </summary>
  public float ValueToDivide => bitsPerSample switch {
    8 => 128.0f,
    16 => 32768.0f,
    24 => 8388608.0f,
    _ => 2147483648.0f
  };

  /// <summary>
  /// Returns the average samples from all channels.
  /// </summary>
  public async Task<float[]>
  GetAvgSamplesAsync(CancellationToken cancellationToken = default) {
    await InitializeAsync(cancellationToken);

    var samples = new float[SamplesCount];

    var sampleIndex = 0;
    await foreach (var sampleFrame in InternalReadSamples(useAsync: true,
                                                          cancellationToken)) {
      var sampleSum = 0L;
      for (var i = 0; i < sampleFrame.Length; i++) {
        sampleSum += sampleFrame[i];
      }

      samples[sampleIndex++] = (sampleSum / ValueToDivide) / channels;
    }
    return samples;
  }

  /// <summary>
  /// Returns the average samples from all channels.
  /// </summary>
  /// <returns></returns>
  public float[] GetAvgSamples() {
    Initialize();

    var asyncEnumerator =
        InternalReadSamples(useAsync: false, CancellationToken.None)
            .GetAsyncEnumerator();
    var samples = new float[SamplesCount];
    var sampleIndex = 0;
    // Will disable CA2012 as I took care of the async enumerator to always run
    // synchronous if useAsync is false.
#pragma warning disable CA2012 // Use ValueTasks correctly
    while (asyncEnumerator.MoveNextAsync().GetAwaiter().GetResult()) {
      var sampleFrame = asyncEnumerator.Current;
      var sampleSum = 0L;
      for (var i = 0; i < sampleFrame.Length; i++) {
        sampleSum += sampleFrame[i];
      }

      samples[sampleIndex++] = (sampleSum / ValueToDivide) / channels;
    }
#pragma warning restore CA2012 // Use ValueTasks correctly
    return samples;
  }

  public float[] GetChannelSamples(int channelIndex = 0) {
    Initialize();
    if (channelIndex >= channels) {
      throw new ArgumentOutOfRangeException(nameof(channelIndex));
    }

    var samples = new float[SamplesCount];
    var sampleIndex = 0;

    var asyncEnumerator =
        InternalReadSamples(useAsync: false, CancellationToken.None)
            .GetAsyncEnumerator();
    // Will disable CA2012 as I took care of the async enumerator to always run
    // synchronous if useAsync is false.
#pragma warning disable CA2012 // Use ValueTasks correctly
    while (asyncEnumerator.MoveNextAsync().GetAwaiter().GetResult()) {
      var sampleFrame = asyncEnumerator.Current;
      samples[sampleIndex++] = sampleFrame[channelIndex] / ValueToDivide;
    }
#pragma warning restore CA2012 // Use ValueTasks correctly
    return samples;
  }

  public async Task<float[]>
  GetChannelSamplesAsync(int channelIndex = 0,
                         CancellationToken cancellationToken = default) {
    await InitializeAsync(cancellationToken);
    if (channelIndex >= channels) {
      throw new ArgumentOutOfRangeException(nameof(channelIndex));
    }

    var samples = new float[SamplesCount];
    var sampleIndex = 0;

    await foreach (var sampleFrame in InternalReadSamples(useAsync: true,
                                                          cancellationToken)) {
      samples[sampleIndex++] = sampleFrame[channelIndex] / ValueToDivide;
    }
    return samples;
  }

  /// <summary>
  /// Initializes the wave parser, by reading the header and the format chunk.
  /// </summary>
  public void Initialize() {
    InternalInitialize(useAsync: false, CancellationToken.None)
        .GetAwaiter()
        .GetResult();
  }

  /// <summary>
  /// Initializes the wave parser, by reading the header and the format chunk in
  /// an async manner.
  /// </summary>
  public Task InitializeAsync(CancellationToken cancellationToken = default) {
    return InternalInitialize(useAsync: true, cancellationToken);
  }

  private async IAsyncEnumerable<long[]> InternalReadSamples(bool useAsync, [
    EnumeratorCancellation
  ] CancellationToken cancellationToken) {
    var buffer = new byte[2048 * channels];
    var memoryBuffer = buffer.AsMemory();

    var sampleIndex = 0;
    var bytesRead = int.MaxValue;

    while (bytesRead > 0 && sampleIndex < SamplesCount) {
      // We need to ensure that we don't read from the stream, more data than
      // the data filled by samples count.
      var maxBytesToRead = (int)Math.Min(
          buffer.Length, (SamplesCount - sampleIndex) * FrameSize);
      if (useAsync) {
#if NET6_0_OR_GREATER
        var memoryToUse = maxBytesToRead == buffer.Length
                              ? memoryBuffer
                              : memoryBuffer[..maxBytesToRead];
        bytesRead = await waveStream.ReadAsync(memoryToUse, cancellationToken);
#else
        bytesRead = await waveStream.ReadAsync(buffer, 0, maxBytesToRead,
                                               cancellationToken);
#endif
      } else {
        bytesRead = waveStream.Read(buffer, 0, maxBytesToRead);
      }

      for (var i = 0; i < bytesRead;) {
        var currentSamples = new long[channels];

        for (var currentChannel = 0; currentChannel < channels;
             currentChannel++) {
          var (currentChannelValue, bytesConsumed) = bitsPerSample switch {
            8 => (buffer[i] - 128, 1),
            16 => (BitConverter.ToInt16(buffer, i), 2),
            24 => (BitConverter.ToInt32(buffer, i) >> 8, 3),
            _ => (BitConverter.ToInt32(buffer, i), 4),
          };
          currentSamples[currentChannel] = currentChannelValue;
          i += bytesConsumed;
        }
        yield return currentSamples;
        sampleIndex++;
      }
    };
    if (sampleIndex < SamplesCount) {
      throw new CorruptedWaveException(
          "Invalid wave file, the size is too small and couldn't read all " +
          "the samples.");
    }
  }

  private async Task InternalInitialize(bool useAsync,
                                        CancellationToken cancellationToken) {
    if (isInitialized) {
      return;
    }

    async Task<int> ReadBytesAsync(byte[] buffer, int offset, int count) {
      if (useAsync) {
#if NET6_0_OR_GREATER
        return await waveStream.ReadAsync(buffer.AsMemory(offset, count),
                                          cancellationToken);
#else
        return await waveStream.ReadAsync(buffer, offset, count,
                                          cancellationToken);
#endif
      }

      return waveStream.Read(buffer, offset, count);
    }

    var buffer = new byte[12];
    var actualRead = await ReadBytesAsync(buffer, 0, 12);

    if (actualRead != 12) {
      throw new CorruptedWaveException(
          "Invalid wave file, the size is too small.");
    }

    // Read RIFF Header
    if (buffer[0] != 'R' || buffer[1] != 'I' || buffer[2] != 'F' ||
        buffer[3] != 'F') {
      throw new CorruptedWaveException("Invalid wave file RIFF header.");
    }

    // Skip FileSize 4 => 8

    // Read Wave and Fmt tags
    if (buffer[8] != 'W' || buffer[9] != 'A' || buffer[10] != 'V' ||
        buffer[11] != 'E') {
      throw new CorruptedWaveException("Invalid wave file header.");
    }

    // Search for format chunk
    int fmtChunkSize;
    while (true) {
      var nextChunkHeader = new byte[8];

      actualRead = await ReadBytesAsync(nextChunkHeader, 0, 8);

      if (actualRead != 8) {
        throw new CorruptedWaveException(
            "Invalid wave file, cannot read next chunk.");
      }

      var chunkSize = BitConverter.ToInt32(nextChunkHeader, 4);
      if (chunkSize < 0) {
        throw new CorruptedWaveException("Invalid wave chunk size.");
      }

      if (nextChunkHeader[0] == 'f' && nextChunkHeader[1] == 'm' &&
          nextChunkHeader[2] == 't' && nextChunkHeader[3] == ' ') {
        fmtChunkSize = chunkSize;
        break;
      }

      if (waveStream.CanSeek) {
        waveStream.Seek(chunkSize, SeekOrigin.Current);
      } else {
        var restOfChunk = new byte[chunkSize];
        await ReadBytesAsync(restOfChunk, 0, chunkSize);
      }
    }

    if (fmtChunkSize < 16) {
      throw new CorruptedWaveException("Invalid wave format size.");
    }

    var fmtBuffer = new byte[fmtChunkSize];
    actualRead = await ReadBytesAsync(fmtBuffer, 0, fmtChunkSize);
    if (actualRead != fmtChunkSize) {
      throw new CorruptedWaveException(
          "Invalid wave file, cannot read format chunk.");
    }

    // Read Format
    var format = BitConverter.ToUInt16(fmtBuffer, 0);
    if (format != 1 &&
        format != 65534) // Allow both standard PCM and WAVE_FORMAT_EXTENSIBLE
    {
      throw new CorruptedWaveException("Unsupported wave file");
    }

    // If the file is in WAVE_FORMAT_EXTENSIBLE format, we'll need to read the
    // SubFormat field
    if (format == 65534) {
      // Verify that fmtChunkSize is at least 40, which is required for
      // WAVE_FORMAT_EXTENSIBLE
      if (fmtChunkSize < 40) {
        throw new CorruptedWaveException("Invalid wave format size.");
      }

      // The SubFormat field is a GUID, but for PCM data it will be
      // {00000001-0000-0010-8000-00aa00389b71} Check this manually, byte by
      // byte
      for (var i = 0; i < 16; i++) {
        if (fmtBuffer[24 + i] != expectedSubFormatForPcm[i]) {
          throw new CorruptedWaveException(
              "Unsupported wave file format. Only PCM is supported.");
        }
      }
    }

    channels = BitConverter.ToUInt16(fmtBuffer, 2);
    if (channels == 0) {
      throw new NotSupportedWaveException(
          "Cannot read wave file with 0 channels.");
    }

    sampleRate = BitConverter.ToUInt32(fmtBuffer, 4);
    if (sampleRate != 16000) {
      throw new NotSupportedWaveException(
          "Only 16KHz sample rate is supported.");
    }

    // Skip Average bytes rate 8 => 12

    // Skip Block Allign 12 => 14

    bitsPerSample = BitConverter.ToUInt16(fmtBuffer, 14);

    if (bitsPerSample != 8 && bitsPerSample != 16 && bitsPerSample != 24 &&
        bitsPerSample != 32) {
      throw new NotSupportedWaveException(
          $"Bits per sample {bitsPerSample} is not supported.");
    }

    // Seek data chuunk
    // Read chunk name and size

    await ReadBytesAsync(buffer, 0, 8);

    while (buffer[0] != 'd' || buffer[1] != 'a' || buffer[2] != 't' ||
           buffer[3] != 'a') {
      var chunkSize = BitConverter.ToInt32(buffer, 4);
      if (chunkSize < 0) {
        throw new CorruptedWaveException("Invalid wave chunk size.");
      }
      if (waveStream.CanSeek) {
        waveStream.Seek(chunkSize, SeekOrigin.Current);
      } else {
        var restOfChunk = new byte[chunkSize];
        await ReadBytesAsync(restOfChunk, 0, chunkSize);
      }

      actualRead = await ReadBytesAsync(buffer, 0, 8);

      if (actualRead != 8) {
        throw new CorruptedWaveException("Invalid wave chunk size.");
      }
    }

    dataChunkSize = BitConverter.ToUInt32(buffer, 4);
    // if the data chunk is not specified, it means the wave was constructed on
    // the fly and we need to read until the end of the stream
    if (dataChunkSize == uint.MaxValue) {
      dataChunkSize = (uint)(waveStream.Length - waveStream.Position);
    }

    dataChunkPosition = waveStream.Position;
    isInitialized = true;
  }
}
