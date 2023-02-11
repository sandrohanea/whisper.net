
using System;
using System.IO;
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
            FullDetection(opt);
            break;
        default:
            Console.WriteLine("Unknown command");
            break;
    }
}

void LanguageIdentification(Options opt)
{
    var bufferedModel = File.ReadAllBytes(opt.ModelName);

    var builder = WhisperProcessorBuilder.Create()
       .WithBufferedModel(bufferedModel)
       .WithLanguage(opt.Language);

    if (opt.Command == "translate")
    {
        builder.WithTranslate();
    }

    using var processor = builder.Build();

    using var fileStream = File.OpenRead(opt.FileName);

    var wave = new WaveParser(fileStream);

    var samples = wave.GetAvgSamples();

    var language = processor.DetectLanguage(samples, speedUp: true);
    Console.WriteLine("Language is" + language);
}

void FullDetection(Options opt)
{

    var builder = WhisperProcessorBuilder.Create()
       .WithFileModel(opt.ModelName)
       .WithSegmentEventHandler(OnNewSegment)
       .WithLanguage(opt.Language);

    if (opt.Command == "translate")
    {
        builder.WithTranslate();
    }

    using var processor = builder.Build();

    static void OnNewSegment(object sender, OnSegmentEventArgs e)
    {
        Console.WriteLine($"New Segment: {e.Start} ==> {e.End} : {e.Segment}");
    }

    using var fileStream = File.OpenRead(opt.FileName);
    processor.Process(fileStream);
    var language = processor.GetAutodetectedLanguage();
    Console.WriteLine("Language was " + language);
}

public class Options
{
    [Option('t', "command", Required = false, HelpText = "Command to run (lang-detect, transcribe or translate)", Default = "transcribe")]
    public string Command { get; set; }

    [Option('f', "file", Required = false, HelpText = "File to process", Default = "1min.wav")]
    public string FileName { get; set; }

    [Option('l', "lang", Required = false, HelpText = "Language", Default = "auto")]
    public string Language { get; set; }

    [Option('m', "modelFile", Required = false, HelpText = "Model to use (filename", Default = "ggml-base.bin")]
    public string ModelName { get; set; }

    [Option('g', "ggml", Required = false, HelpText = "Ggml Model type to download (if not exists)", Default = GgmlType.Base)]
    public GgmlType ModelType { get; set; }
}
