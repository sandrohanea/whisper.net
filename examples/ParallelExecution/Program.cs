// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

public class Program
{
    // This examples shows how to use Whisper.net to create a transcription from an audio file with 16Khz sample rate with 2 parallel executions.
    public static async Task Main(string[] args)
    {
        var ggmlType = GgmlType.Base;
        var modelFileName = "ggml-base.bin";

        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        // Optional logging from the native library
        LogProvider.OnLog += (level, message) =>
        {
            Console.Write($"{level}: {message}");
        };

        // This section creates the whisperFactory object which is used to create the processor object.
        using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

        var task1 = Task.Run(() => RunInParallel("Task1", "kennedy.wav", whisperFactory));
        var task2 = Task.Run(() => RunInParallel("Task2", "kennedy.wav", whisperFactory));

        // We wait both tasks and we'll see that the results are interleaved
        await Task.WhenAll(task1, task2);
    }

    public static async Task RunInParallel(string name, string wavFileName, WhisperFactory whisperFactory)
    {

        // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
        using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        using var fileStream = File.OpenRead(wavFileName);

        // This section processes the audio file and prints the results (start time, end time and text) to the console.
        await foreach (var result in processor.ProcessAsync(fileStream))
        {
            Console.WriteLine($"{name} =====> {result.Start}->{result.End}: {result.Text}");

            // Add some delay, otherwise we might get the results too fast
            await Task.Delay(1000);
        }
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}
