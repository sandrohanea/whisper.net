// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Wave;

public sealed class WaveParser
{
    private static readonly byte[] expectedSubFormatForPcm = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71 };
    private readonly Stream waveStream;
    private ushort channels;
    private uint sampleRate;
    private ushort bitsPerSample;
    private uint dataChunkSize;
    private long dataChunkPosition;
    private bool wasInitialized;

    public WaveParser(Stream waveStream)
    {
        this.waveStream = waveStream;
    }

    public int GetSamplesCount()
    {
        if (!wasInitialized)
        {
            Initialize();
        }

        return (int)(dataChunkSize / 2 / channels);
    }

    /// <summary>
    /// Returns the average samples from all channels.
    /// </summary>
    /// <returns></returns>
    public async Task<float[]> GetAvgSamplesAsync(CancellationToken cancellationToken)
    {
        if (!wasInitialized)
        {
            await InitializeAsync();
        }

        if (channels == 0)
        {
            throw new InvalidOperationException("Channel count is set to 0");
        }

        var samplesCount = GetSamplesCount();
        var samples = new float[samplesCount];

        var buffer = new byte[2048 * channels];

        var sampleIndex = 0;
        int bytesRead;

        do
        {
            bytesRead = await waveStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

            for (var i = 0; i < bytesRead;)
            {
                long sampleSum = 0;

                for (var currentChannel = 0; currentChannel < channels; currentChannel++)
                {
                    sampleSum += BitConverter.ToInt16(buffer, i);
                    i += 2;
                }

                samples[sampleIndex++] = sampleSum / (float)channels / 32768.0f;
            }
        } while (bytesRead > 0);

        return samples;
    }

    /// <summary>
    /// Returns the average samples from all channels.
    /// </summary>
    /// <returns></returns>
    public float[] GetAvgSamples()
    {
        if (!wasInitialized)
        {
            Initialize();
        }

        var reader = GetDataReader();
        var samplesCount = GetSamplesCount();
        var samples = new float[samplesCount];

        for (var i = 0; i < samplesCount; i++)
        {
            var sampleSum = 0L;

            for (var currentChannel = 0; currentChannel < channels; currentChannel++)
            {
                sampleSum += reader.ReadInt16();
            }

            samples[i] = (sampleSum / 4) / 32768.0f;
        }

        return samples;
    }

    public float[] GetChannelSamples(int channelIndex = 0)
    {
        if (!wasInitialized)
        {
            Initialize();
        }

        var reader = GetDataReader();
        var samplesCount = GetSamplesCount();
        var samples = new float[samplesCount];

        for (var i = 0; i < samplesCount; i++)
        {
            for (var currentChannel = 0; currentChannel < channels; currentChannel++)
            {
                if (channelIndex == currentChannel)
                {
                    samples[i] = reader.ReadInt16() / 32768.0f;
                }
                else
                {
                    _ = reader.ReadInt16();
                }
            }
        }
        return samples;
    }

    private void Initialize()
    {
        InitializeCore(useAsync: false).GetAwaiter().GetResult();
    }

    private Task InitializeAsync()
    {
        return InitializeCore(useAsync: true);
    }

    private async Task InitializeCore(bool useAsync)
    {
        if (wasInitialized)
        {
            return;
        }

        var buffer = new byte[12];
        var actualRead = useAsync
                ? await waveStream.ReadAsync(buffer, 0, 12)
                : waveStream.Read(buffer, 0, 12);

        if (actualRead != 12)
        {
            throw new CorruptedWaveException("Invalid wave file, the size is too small.");
        }

        //Read RIFF Header
        if (buffer[0] != 'R' || buffer[1] != 'I' || buffer[2] != 'F' || buffer[3] != 'F')
        {
            throw new CorruptedWaveException("Invalid wave file RIFF header.");
        }

        // Skip FileSize 4 => 8

        // Read Wave and Fmt tags
        if (buffer[8] != 'W' || buffer[9] != 'A' || buffer[10] != 'V' || buffer[11] != 'E')
        {
            throw new CorruptedWaveException("Invalid wave file header.");
        }

        // Search for format chunk
        int fmtChunkSize;
        while (true)
        {
            var nextChunkHeader = new byte[8];
            actualRead = useAsync
                            ? await waveStream.ReadAsync(nextChunkHeader, 0, 8)
                            : waveStream.Read(nextChunkHeader, 0, 8);

            if (actualRead != 8)
            {
                throw new CorruptedWaveException("Invalid wave file, cannot read next chunk.");
            }

            var chunkSize = BitConverter.ToInt32(nextChunkHeader, 4);
            if (chunkSize < 0)
            {
                throw new CorruptedWaveException("Invalid wave chunk size.");
            }

            if (nextChunkHeader[0] == 'f' && nextChunkHeader[1] == 'm' && nextChunkHeader[2] == 't' && nextChunkHeader[3] == ' ')
            {
                fmtChunkSize = chunkSize;
                break;
            }

            if (waveStream.CanSeek)
            {
                waveStream.Seek(chunkSize, SeekOrigin.Current);
            }
            else
            {
                var restOfChunk = new byte[chunkSize];
                _ = useAsync ? await waveStream.ReadAsync(restOfChunk, 0, chunkSize)
                            : waveStream.Read(restOfChunk, 0, chunkSize);
            }
        }

        if (fmtChunkSize < 16)
        {
            throw new CorruptedWaveException("Invalid wave format size.");
        }

        var fmtBuffer = new byte[fmtChunkSize];
        actualRead = useAsync ? await waveStream.ReadAsync(fmtBuffer, 0, fmtChunkSize)
                            : waveStream.Read(fmtBuffer, 0, fmtChunkSize);
        if (actualRead != fmtChunkSize)
        {
            throw new CorruptedWaveException("Invalid wave file, cannot read format chunk.");
        }

        // Read Format
        var format = BitConverter.ToUInt16(fmtBuffer, 0);
        if (format != 1 && format != 65534) // Allow both standard PCM and WAVE_FORMAT_EXTENSIBLE
        {
            throw new CorruptedWaveException("Unsupported wave file");
        }

        // If the file is in WAVE_FORMAT_EXTENSIBLE format, we'll need to read the SubFormat field
        if (format == 65534)
        {
            // Verify that fmtChunkSize is at least 40, which is required for WAVE_FORMAT_EXTENSIBLE
            if (fmtChunkSize < 40)
            {
                throw new CorruptedWaveException("Invalid wave format size.");
            }

            // The SubFormat field is a GUID, but for PCM data it will be {00000001-0000-0010-8000-00aa00389b71}
            // Check this manually, byte by byte
            for (var i = 0; i < 16; i++)
            {
                if (fmtBuffer[24 + i] != expectedSubFormatForPcm[i])
                {
                    throw new CorruptedWaveException("Unsupported wave file format. Only PCM is supported.");
                }
            }
        }

        channels = BitConverter.ToUInt16(fmtBuffer, 2);
        sampleRate = BitConverter.ToUInt32(fmtBuffer, 4);
        if (sampleRate != 16000)
        {
            throw new NotSupportedWaveException("Only 16KHz sample rate is supported.");
        }

        // Skip Average bytes rate 8 => 12

        // Skip Block Allign 12 => 14

        bitsPerSample = BitConverter.ToUInt16(fmtBuffer, 14);
        if (bitsPerSample != 16)
        {
            throw new NotSupportedWaveException("Only 16 bits per sample is supported.");
        }

        // Seek data chuunk
        // Read chunk name and size

        _ = useAsync ? await waveStream.ReadAsync(buffer, 0, 8)
                    : waveStream.Read(buffer, 0, 8);

        while (buffer[0] != 'd' || buffer[1] != 'a' || buffer[2] != 't' || buffer[3] != 'a')
        {
            var chunkSize = BitConverter.ToInt32(buffer, 4);
            if (chunkSize < 0)
            {
                throw new CorruptedWaveException("Invalid wave chunk size.");
            }
            if (waveStream.CanSeek)
            {
                waveStream.Seek(chunkSize, SeekOrigin.Current);
            }
            else
            {
                var restOfChunk = new byte[chunkSize];
                _ = useAsync ? await waveStream.ReadAsync(restOfChunk, 0, chunkSize)
                            : waveStream.Read(restOfChunk, 0, chunkSize);
            }

            actualRead = useAsync ? await waveStream.ReadAsync(buffer, 0, 8)
                        : waveStream.Read(buffer, 0, 8);

            if (actualRead != 8)
            {
                throw new CorruptedWaveException("Invalid wave chunk size.");
            }
        }

        dataChunkSize = BitConverter.ToUInt32(buffer, 4);
        dataChunkPosition = waveStream.Position;
        wasInitialized = true;
    }

    private BinaryReader GetDataReader()
    {
        var reader = new BinaryReader(waveStream);
        if (waveStream.Position != dataChunkPosition)
        {
            waveStream.Seek(dataChunkPosition, SeekOrigin.Begin);
        }
        return reader;
    }

}
