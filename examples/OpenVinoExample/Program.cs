// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

public class Program
{
    // This examples shows how to use Whisper.net to create a transcription from an audio file with 16Khz sample rate.
    // It uses both Cuda (NVidia GPU) or CPU, and loads the first one that is available.
    public static async Task Main(string[] args)
    {
        // We declare three variables which we will use later, ggmlType, modelFileName and wavFileName
        var ggmlType = GgmlType.Small;
        var modelFileName = "ggml-small.bin";
        var wavFileName = "kennedy.wav";
        var encoderDirectoryName = "ggml-small-encoder";

        using var whisperLogger = LogProvider.AddConsoleLogging(WhisperLogLevel.Debug);

        // This section detects whether the "ggml-small" file exists in our project disk. If it doesn't, it downloads it from the internet
        if (!File.Exists(modelFileName))
        {
            await DownloadModel(modelFileName, ggmlType);
        }

        // This sections detects whether the modelc directory (used by CoreML) is in out project disk. If it doesn't, it downloads it and extract it to the current folder.
        if (!Directory.Exists(encoderDirectoryName))
        {
            // Note: The encoder directory needs to be extracted
            await WhisperGgmlDownloader.GetEncoderOpenVinoModelAsync(ggmlType)
                                       .ExtractToPath(encoderDirectoryName);
        }

        // This section creates the whisperFactory object which is used to create the processor object.
        using var whisperFactory = WhisperFactory.FromPath(modelFileName);

        // We need to get the path to the xml encoder manifest file
        var xmlEncoderManifest = Path.Combine(encoderDirectoryName, WhisperGgmlDownloader.GetOpenVinoManifestFileName(ggmlType));

        // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
        using var processor = whisperFactory.CreateBuilder()
            .WithOpenVinoEncoder(xmlEncoderManifest, "GPU", null)
            .WithLanguage("auto")
            .Build();

        using var fileStream = File.OpenRead(wavFileName);

        // This section processes the audio file and prints the results (start time, end time and text) to the console.
        await foreach (var result in processor.ProcessAsync(fileStream))
        {
            Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
        }
    }

    private static async Task DownloadModel(string fileName, GgmlType ggmlType)
    {
        Console.WriteLine($"Downloading Model {fileName}");
        using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(ggmlType);
        using var fileWriter = File.OpenWrite(fileName);
        await modelStream.CopyToAsync(fileWriter);
    }
}
