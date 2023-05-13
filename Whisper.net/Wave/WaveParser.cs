// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Wave;

public sealed class WaveParser
{
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

        var buffer = new byte[4096];

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
        if (wasInitialized)
        {
            return;
        }

        var buffer = new byte[12];
        var actualRead = waveStream.Read(buffer, 0, 12);
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
            actualRead = waveStream.Read(nextChunkHeader, 0, 8);
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
                waveStream.Read(restOfChunk, 0, chunkSize);
            }
        }

        if (fmtChunkSize < 16)
        {
            throw new CorruptedWaveException("Invalid wave format size.");
        }

        var fmtBuffer = new byte[fmtChunkSize];
        actualRead = waveStream.Read(fmtBuffer, 0, fmtChunkSize);
        if (actualRead != fmtChunkSize)
        {
            throw new CorruptedWaveException("Invalid wave file, cannot read format chunk.");
        }

        // Read Format
        var format = BitConverter.ToUInt16(fmtBuffer, 0);
        if (format != 1)
        {
            throw new CorruptedWaveException("Unsupported wave file");
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
        // Until now we have read 18 bytes in format, the rest is cbSize, averageBytesRate, and is ignored for now.
        if (fmtChunkSize > 18)
        {
            if (waveStream.CanSeek)
            {
                waveStream.Seek(fmtChunkSize - 18, SeekOrigin.Current);
            }
            else
            {
                var restOfBuffer = new byte[fmtChunkSize - 18];
                waveStream.Read(restOfBuffer, 0, fmtChunkSize - 18);
            }
        }

        // Seek data chuunk
        // Read chunk name and size
        waveStream.Read(buffer, 0, 8);
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
                waveStream.Read(restOfChunk, 0, chunkSize);
            }

            actualRead = waveStream.Read(buffer, 0, 8);
            if (actualRead != 8)
            {
                throw new CorruptedWaveException("Invalid wave chunk size.");
            }
        }

        dataChunkSize = BitConverter.ToUInt32(buffer, 4);
        dataChunkPosition = waveStream.Position;
        wasInitialized = true;
    }

    private async Task InitializeAsync()
    {
        if (wasInitialized)
        {
            return;
        }

        var buffer = new byte[12];
        var actualRead = await waveStream.ReadAsync(buffer, 0, 12);
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
            actualRead = await waveStream.ReadAsync(nextChunkHeader, 0, 8);
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
                await waveStream.ReadAsync(restOfChunk, 0, chunkSize);
            }
        }

        if (fmtChunkSize < 16)
        {
            throw new CorruptedWaveException("Invalid wave format size.");
        }

        var fmtBuffer = new byte[fmtChunkSize];
        actualRead = await waveStream.ReadAsync(fmtBuffer, 0, fmtChunkSize);
        if (actualRead != fmtChunkSize)
        {
            throw new CorruptedWaveException("Invalid wave file, cannot read format chunk.");
        }

        // Read Format
        var format = BitConverter.ToUInt16(fmtBuffer, 0);
        if (format != 1)
        {
            throw new CorruptedWaveException("Unsupported wave file");
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
        // Until now we have read 18 bytes in format, the rest is cbSize, averageBytesRate, and is ignored for now.
        if (fmtChunkSize > 18)
        {
            if (waveStream.CanSeek)
            {
                waveStream.Seek(fmtChunkSize - 18, SeekOrigin.Current);
            }
            else
            {
                var restOfBuffer = new byte[fmtChunkSize - 18];
                await waveStream.ReadAsync(restOfBuffer, 0, fmtChunkSize - 18);
            }
        }

        // Seek data chuunk
        // Read chunk name and size
        await waveStream.ReadAsync(buffer, 0, 8);
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
                await waveStream.ReadAsync(restOfChunk, 0, chunkSize);
            }

            actualRead = await waveStream.ReadAsync(buffer, 0, 8);
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
