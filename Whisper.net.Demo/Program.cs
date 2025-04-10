// Licensed under the MIT license: https://opensource.org/licenses/MIT

using CommandLine;
using Microsoft.Extensions.AI;
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
        using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(opt.ModelType);
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
            await FullDetectionSpeechToText(opt);
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

    var language = processor.DetectLanguage(samples);
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
    Console.WriteLine($"Using {nameof(WhisperProcessor)}:\n");
    await foreach (var segment in processor.ProcessAsync(fileStream, CancellationToken.None))
    {
        Console.WriteLine($"New Segment: {segment.Start} ==> {segment.End} : {segment.Text}");
    }
}

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
async Task FullDetectionSpeechToText(Options opt)
{
    // Same factory can be used by multiple task to create processors.
    using var speechToTextClient = new WhisperSpeechToTextClient(opt.ModelName);

    var options = new SpeechToTextOptions().WithLanguage(opt.Language);

    if (opt.Command == "translate")
    {
        options.WithTranslate();
    }

    using var fileStream = File.OpenRead(opt.FileName);

    Console.WriteLine($"\nUsing {nameof(ISpeechToTextClient)}:\n");
    await foreach (var segment in speechToTextClient.GetStreamingTextAsync(fileStream, options, CancellationToken.None))
    {
        Console.WriteLine($"New Segment: {segment.StartTime} ==> {segment.EndTime} : {segment.Text}");
    }
}
#pragma warning restore MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

/// <summary>
/// The options for this Demo
/// </summary>
public class Options
{
    /// <summary>
    /// The command to run (lang-detect, transcribe or transalte)
    /// </summary>
    [Option('t', "command", Required = false, HelpText = "Command to run (lang-detect, transcribe or translate)", Default = "transcribe")]
    public string Command { get; set; } = "transcribe";

    /// <summary>
    /// The fileName to process
    /// </summary>
    [Option('f', "file", Required = false, HelpText = "File to process", Default = "kennedy.wav")]
    public string FileName { get; set; } = "kennedy.wav";

    /// <summary>
    /// The language to be used, or `auto` if auto-detection should be tried.
    /// </summary>
    [Option('l', "lang", Required = false, HelpText = "Language", Default = "auto")]
    public string Language { get; set; } = "auto";

    /// <summary>
    /// The ggml model file to be used
    /// </summary>
    [Option('m', "modelFile", Required = false, HelpText = "Model to use (filename", Default = "ggml-base.bin")]
    public string ModelName { get; set; } = "ggml-base.bin";

    /// <summary>
    /// The model type, to be downloaded if model file was not found.
    /// </summary>

    [Option('g', "ggml", Required = false, HelpText = "Ggml Model type to download (if not exists)", Default = GgmlType.Base)]
    public GgmlType ModelType { get; set; } = GgmlType.Base;
}
