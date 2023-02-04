
using System;
using System.IO;
using Whisper.net;
using Whisper.net.Ggml;

var modelName = "ggml-base.bin";
if (!File.Exists(modelName))
{
    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
    using var fileWriter = File.OpenWrite(modelName);
    await modelStream.CopyToAsync(fileWriter);
}

using var processor = WhisperProcessorBuilder.Create()
    .WithSegmentEventHandler(OnNewSegment)
    .WithFileModel("ggml-base.bin")
    .WithTranslate()
    .WithLanguage("auto")
    .Build();

void OnNewSegment(object sender, OnSegmentEventArgs e)
{
    Console.WriteLine($"CSSS {e.Start} ==> {e.End} : {e.Segment}");
}

using var fileStream = File.OpenRead("rom.wav");
processor.Process(fileStream);
