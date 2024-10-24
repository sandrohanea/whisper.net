// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Threading.Channels;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Wave;

var ggmlType = GgmlType.TinyEn;
var modelFileName = "ggml-tinyen.bin";
var wavFileName = "bush.wav";

var chunkDuration = TimeSpan.FromSeconds(5);

if (!File.Exists(modelFileName))
{
    await DownloadModel(modelFileName, ggmlType);
}

using var whisperFactory = WhisperFactory.FromPath(modelFileName);

var builder = whisperFactory.CreateBuilder().WithProbabilities().WithLanguage("en");

using var processor = builder.Build();

var channel = Channel.CreateUnbounded<float[]>(
    new UnboundedChannelOptions { SingleReader = true, SingleWriter = true, }
);

var thread = new Thread(() => PushToChannel(wavFileName, chunkDuration, channel))
{
    IsBackground = true,
};

thread.Start();

var reader = channel.Reader;

var sampleBuffer = (float[]?)null;
var bufferSize = 0;
var chunkCount = 0;

var start = DateTime.Now;

while (await reader.WaitToReadAsync())
{
    var samples = await reader.ReadAsync();

    sampleBuffer ??= new float[samples.Length * 2];

    WriteBuffer(sampleBuffer, samples, ref bufferSize);

    await foreach (var result in processor.ProcessAsync(sampleBuffer.AsMemory(0, bufferSize)))
    {
        Console.WriteLine(
            $"{chunkCount}: {result.Start}->{result.End}: {result.Text}  => with probability: {result.Probability}"
        );
    }

    chunkCount++;
}

static void WriteBuffer(float[] destination, in float[] source, ref int count)
{
    var floatSize = sizeof(float);

    if (count <= 0)
    {
        Buffer.BlockCopy(source, 0, destination, 0, source.Length * floatSize);
        count = source.Length;
        return;
    }

    Buffer.BlockCopy(
        destination,
        destination.Length / 2 * floatSize,
        destination,
        0,
        destination.Length / 2 * floatSize
    );

    Buffer.BlockCopy(
        source,
        0,
        destination,
        destination.Length / 2 * floatSize,
        source.Length * floatSize
    );

    count = destination.Length;
}

static void PushToChannel(string filename, TimeSpan chunkDuration, Channel<float[]> channel)
{
    var writer = channel.Writer;

    using var sourceStream = File.Open(filename, FileMode.Open);

    var waveParser = new WaveParser(sourceStream);
    waveParser.Initialize();

    var samplesSize = CalculateSamplesSize(waveParser, chunkDuration);
    var chunkSize = CalculateChunkSize(waveParser, chunkDuration);

    var samples = new float[samplesSize];
    var buffer = new byte[chunkSize];

    sourceStream.Seek(waveParser.DataChunkPosition, SeekOrigin.Begin);

    while (sourceStream.Position != sourceStream.Length)
    {
        var bytesRead = sourceStream.Read(buffer);

        ConvertToSamples(bytesRead, buffer, samples);

        writer.TryWrite(samples);

        if (bytesRead == chunkSize)
        {
            Thread.Sleep(chunkDuration);
        }
    }

    writer.Complete();
}

static int CalculateChunkSize(WaveParser waveParser, TimeSpan chunkDuration)
{
    if (!waveParser.IsInitialized)
    {
        throw new InvalidOperationException("WaveParser is not initialized");
    }

    var sizeOfOneSecondAudio = waveParser.SampleRate * (waveParser.BitsPerSample / 8);
    var totalAudioDataSize = sizeOfOneSecondAudio * chunkDuration.TotalSeconds;

    return Convert.ToInt32(totalAudioDataSize);
}

static int CalculateSamplesSize(WaveParser waveParser, TimeSpan chunkDuration)
{
    return Convert.ToInt32(waveParser.SampleRate * chunkDuration.TotalSeconds);
}

static void ConvertToSamples(int count, byte[] buffer, float[] samples)
{
    var sampleIndex = 0;

    for (var i = 0; i < count; i += 2)
    {
        samples[sampleIndex++] = BitConverter.ToInt16(buffer, i) / 32768.0f;
    }
}

static async Task DownloadModel(string fileName, GgmlType ggmlType)
{
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
}
