// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;

public class Program
{
    public static async Task Main(string[] args)
    {
        var ggmlType = GgmlType.Base;
        var modelFileName = "ggml-base.bin";
        var wavFileName = "kennedy.wav";

        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .WithSegmentEventHandler((segment) =>
            {
                // Do whetever you want with your segment here.
                Console.WriteLine($"{segment.Start}->{segment.End}: {segment.Text}");
            })
            .Build();

        using var fileStream = File.OpenRead(wavFileName);
        processor.Process(fileStream);
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}
