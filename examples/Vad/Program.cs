// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

public class Program
{
    public static async Task Main()
    {
        var modelFileName = Path.Combine(AppContext.BaseDirectory, "ggml-silero-v6.2.0.bin");
        var wavFileName = Path.Combine(AppContext.BaseDirectory, "kennedy.wav");

        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName);
        }

        using var vadFactory = WhisperVadFactory.FromPath(modelFileName);
        using var vadProcessor = vadFactory.CreateBuilder()
            .WithThreshold(0.5f)
            .Build();

        using var fileStream = File.OpenRead(wavFileName);
        var segments = await vadProcessor.DetectSpeechAsync(fileStream);

        if (segments.Count == 0)
        {
            throw new InvalidOperationException("No speech segments were detected.");
        }

        foreach (var segment in segments)
        {
            Console.WriteLine($"{segment.Start}->{segment.End}");
        }
    }

    private static async Task DownloadModel(string fileName)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlSileroVadModelAsync();
        using var fileWriter = File.Create(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}
