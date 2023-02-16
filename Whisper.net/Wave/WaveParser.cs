// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Wave;

public sealed class WaveParser
{
    private readonly Stream waveStream;
    private readonly ushort channels;
    private readonly uint sampleRate;
    private readonly ushort bitsPerSample;
    private readonly uint dataChunkSize;
    private readonly long dataChunkPosition;

    public WaveParser(Stream waveStream)
    {
        this.waveStream = waveStream;
        //Read RIFF Header
        var reader = new BinaryReader(waveStream);
        var riff = reader.ReadChars(4);
        if (riff[0] != 'R' || riff[1] != 'I' || riff[2] != 'F' || riff[3] != 'F')
        {
            throw new CorruptedWaveException("Invalid wave file RIFF header.");
        }

        // Read FileSize
        _ = reader.ReadInt32();

        // Read Wave and Fmt tags
        var wave = reader.ReadChars(8);
        if (wave[0] != 'W' || wave[1] != 'A' || wave[2] != 'V' || wave[3] != 'E' || wave[4] != 'f' || wave[5] != 'm' || wave[6] != 't' || wave[7] != ' ')
        {
            throw new CorruptedWaveException("Invalid wave file header.");
        }

        // Read Format Chunk Size
        var formatChunkSize = reader.ReadInt32();
        if (formatChunkSize < 0)
        {
            throw new CorruptedWaveException("Invalid wave format size.");
        }
        // Read Format
        var format = reader.ReadUInt16();
        if (format != 1)
        {
            throw new CorruptedWaveException("Unsupported wave file");
        }

        channels = reader.ReadUInt16();
        sampleRate = reader.ReadUInt32();
        if (sampleRate != 16000)
        {
            throw new NotSupportedWaveException("Only 16KHz sample rate is supported.");
        }

        //Average bytes rate
        _ = reader.ReadUInt32();

        // Block allign
        _ = reader.ReadUInt16();

        bitsPerSample = reader.ReadUInt16();
        if (bitsPerSample != 16)
        {
            throw new NotSupportedWaveException("Only 16 bits per sample is supported.");
        }
        // Until now we have read 16 bytes in format, the rest is cbSize, averageBytesRate, and is ignored for now.
        if (formatChunkSize > 16)
        {
            if (waveStream.CanSeek)
            {
                waveStream.Seek(formatChunkSize - 16, SeekOrigin.Current);
            }
            else
            {
                _ = reader.ReadBytes((int)(formatChunkSize - 16));
            }
        }

        // Seek Data Chunk
        var nextChunk = reader.ReadChars(4);
        while (nextChunk[0] != 'd' || nextChunk[1] != 'a' || nextChunk[2] != 't' || nextChunk[3] != 'a')
        {
            var chunkSize = reader.ReadInt32();
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
                _ = reader.ReadBytes(chunkSize);
            }
            nextChunk = reader.ReadChars(4);
        }

        dataChunkSize = reader.ReadUInt32();
        dataChunkPosition = waveStream.Position;
    }

    public int GetSamplesCount()
    {
        return (int)(dataChunkSize / 2 / channels);
    }

    /// <summary>
    /// Returns the average samples from all channels.
    /// </summary>
    /// <returns></returns>
    public async Task<float[]> GetAvgSamplesAsync(CancellationToken cancellationToken)
    {
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
