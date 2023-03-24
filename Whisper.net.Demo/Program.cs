// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Wave;

await Parser.Default.ParseArguments<Options>(args)
    .WithParsedAsync(Demo);

async Task Demo(Options opt)

{
    if (!File.Exists(opt.ModelName))
    {
        Console.WriteLine($"Downloading Model {opt.ModelName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(opt.ModelType);
        using var fileWriter = File.OpenWrite(opt.ModelName);
        await modelStream.CopyToAsync(fileWriter);
    }

    switch (opt.Command)
    {
        case "lang-detect":
            LanguageIdentification(opt);
            break;
        case "transcribe":
        case "translate":
            await FullDetection(opt);
            break;
        default:
            Console.WriteLine("Unknown command");
            break;
    }
}

void LanguageIdentification(Options opt)
{
    var bufferedModel = File.ReadAllBytes(opt.ModelName);

    // Same factory can be used by multiple task to create processors.
    using var factory = WhisperFactory.FromBuffer(bufferedModel);

    var builder = factory.CreateBuilder()
       .WithLanguage(opt.Language);

    using var processor = builder.Build();

    using var fileStream = File.OpenRead(opt.FileName);

    var wave = new WaveParser(fileStream);

    var samples = wave.GetAvgSamples();

    var language = processor.DetectLanguage(samples, speedUp: true);
    Console.WriteLine("Language is " + language);
}

async Task FullDetection(Options opt)
{
    // Same factory can be used by multiple task to create processors.
    using var factory = WhisperFactory.FromPath(opt.ModelName);

    var builder = factory.CreateBuilder()
        .WithLanguage(opt.Language);

    if (opt.Command == "translate")
    {
        builder.WithTranslate();
    }

    using var processor = builder.Build();

    using var fileStream = File.OpenRead(opt.FileName);

    await foreach (var segment in processor.ProcessAsync(fileStream, CancellationToken.None))
    {
        Console.WriteLine($"New Segment: {segment.Start} ==> {segment.End} : {segment.Text}");
    }
}

public class Options
{
    [Option('t', "command", Required = false, HelpText = "Command to run (lang-detect, transcribe or translate)", Default = "transcribe")]
    public string Command { get; set; }

    [Option('f', "file", Required = false, HelpText = "File to process", Default = "kennedy.wav")]
    public string FileName { get; set; }

    [Option('l', "lang", Required = false, HelpText = "Language", Default = "auto")]
    public string Language { get; set; }

    [Option('m', "modelFile", Required = false, HelpText = "Model to use (filename", Default = "ggml-base.bin")]
    public string ModelName { get; set; }

    [Option('g', "ggml", Required = false, HelpText = "Ggml Model type to download (if not exists)", Default = GgmlType.Base)]
    public GgmlType ModelType { get; set; }
}
