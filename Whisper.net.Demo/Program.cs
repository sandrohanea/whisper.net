
using System;
using System.IO;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Wave;

var modelName = "ggml-base.bin";
if (!File.Exists(modelName))
{
    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
    using var fileWriter = File.OpenWrite(modelName);
    await modelStream.CopyToAsync(fileWriter);
}

var bufferedModel = File.ReadAllBytes(modelName);

using var processor = WhisperProcessorBuilder.Create()
    .WithSegmentEventHandler(OnNewSegment)
    .WithBeamSearchSamplingStrategy()
    .ParentBuilder
    .WithBufferedModel(bufferedModel)
    .WithTranslate()
    .WithLanguage("auto")
    .Build();

static void OnNewSegment(object sender, OnSegmentEventArgs e)
{
    Console.WriteLine($"CSSS {e.Start} ==> {e.End} : {e.Segment}");
}

using var fileStream = File.OpenRead("rom.wav");

var wave = new WaveParser(fileStream);

var samples = wave.GetAvgSamples();

var language = processor.DetectLanguage(samples);
processor.ChangeLanguage(language);

processor.Process(samples);
