// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Wave;

var ggmlType = GgmlType.TinyEn;
var modelFileName = "ggml-tinyen.bin";
var wavFileName = "bush.wav";

var maxProcessingTimeMs = 10000;
var minProcessingTimeMs = 1500;
var advancingProcessingTimeMs = 500;

if (!File.Exists(modelFileName))
{
    await DownloadModel(modelFileName, ggmlType);
}

using var whisperFactory = WhisperFactory.FromPath(modelFileName);

var builder = whisperFactory.CreateBuilder()
    .WithProbabilities()
    .WithLanguage("en");

using var fileStream = File.OpenRead(wavFileName);
var waveParser = new WaveParser(fileStream);
await waveParser.InitializeAsync();

var samples = new float[waveParser.SampleRate / 1000 * maxProcessingTimeMs];

// Process first the minimum processing time of the audio file 

// Read first min processing time into samples
var dataPosition = waveParser.DataChunkPosition;

fileStream.Seek(dataPosition, SeekOrigin.Begin);

var partialResults = new List<(List<SegmentData> segments, TimeSpan startTime, TimeSpan endTime)>();
var buffer = new byte[waveParser.SampleRate / 1000 * maxProcessingTimeMs * 2 * waveParser.Channels];

var bufferSize = waveParser.SampleRate / 1000 * minProcessingTimeMs * 2 * waveParser.Channels;

var bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, (int)bufferSize));

var currentSampleIndex = 0;

for (var i = 0; i < bytesRead;)
{
    long sampleSum = 0;

    for (var currentChannel = 0; currentChannel < waveParser.Channels; currentChannel++)
    {
        sampleSum += BitConverter.ToInt16(buffer, i);
        i += 2;
    }

    samples[currentSampleIndex++] = sampleSum / (float)waveParser.Channels / 32768.0f;
}

var currentProcessedStartTime = TimeSpan.Zero;
var currentProcessedEndTime = TimeSpan.FromMilliseconds(minProcessingTimeMs);

await using (var processor = builder.Build())
{
    var segments = new List<SegmentData>();
    await foreach (var data in processor.ProcessAsync(samples.AsMemory(0, currentSampleIndex)))
    {
        segments.Add(data);

    }
    partialResults.Add((segments, currentProcessedStartTime, currentProcessedEndTime));
}

var fullText = string.Empty;

while (currentSampleIndex < waveParser.SamplesCount)
{
    bufferSize = waveParser.SampleRate / 1000 * advancingProcessingTimeMs * 2 * waveParser.Channels;

    bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, (int)bufferSize));
    for (var i = 0; i < bytesRead;)
    {
        long sampleSum = 0;

        for (var currentChannel = 0; currentChannel < waveParser.Channels; currentChannel++)
        {
            sampleSum += BitConverter.ToInt16(buffer, i);
            i += 2;
        }

        samples[currentSampleIndex++] = sampleSum / (float)waveParser.Channels / 32768.0f;
    }

    currentProcessedEndTime = currentProcessedEndTime.Add(TimeSpan.FromMilliseconds(advancingProcessingTimeMs));

    await using (var processor = builder.Build())
    {
        var segments = new List<SegmentData>();
        await foreach (var data in processor.ProcessAsync(samples.AsMemory(0, currentSampleIndex)))
        {
            segments.Add(data);
        }
        partialResults.Add((segments, currentProcessedStartTime, currentProcessedEndTime));

        var indexSegment = 0;
        foreach (var segment in segments)
        {
            Console.WriteLine($"{indexSegment}: {segment.Start}->{segment.End}: {segment.Text}  => with probability: {segment.Probability}");
            indexSegment++;
        }
    }

    var indexPartial = 0;
    //TODO: Check if partials concluded to one finished segment and return it.
    foreach (var partial in partialResults)
    {
      //  Console.WriteLine(indexPartial + ":" + partial.startTime + " - " + partial.endTime + " " + partial.segments.Count + " segments\n-----------");
        indexPartial++;
        // If one segment is identified. E.g. "My fellow Americans" from second 0 to second 3 => we remove that part from the samples, adding the text to the prompt and continue processing the rest of the samples.
    }

    // If the total current processing time is reaching max processing time => we remove half of the samples and continue processing the rest of the samples.
    if (currentProcessedEndTime.TotalMilliseconds - currentProcessedStartTime.TotalMilliseconds >= maxProcessingTimeMs)
    {
        // First, we copy the last part of the samples to the beginning of the array
        var samplesToCopy = currentSampleIndex - maxProcessingTimeMs / 2;
        for (var i = 0; i < samplesToCopy; i++)
        {
            samples[i] = samples[i + maxProcessingTimeMs / 2];
        }
        currentProcessedStartTime = currentProcessedStartTime.Add(TimeSpan.FromMilliseconds(maxProcessingTimeMs / 2));
        currentSampleIndex = samplesToCopy;
    }
}

static async Task DownloadModel(string fileName, GgmlType ggmlType)
{
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
}
