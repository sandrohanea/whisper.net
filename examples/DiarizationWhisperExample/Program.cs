// Licensed under the MIT license: https://opensource.org/licenses/MIT

// Example is still in progress (Multichannel wav file with 3 channels is not supported yet by Whisper.net 1.4.2)

using Whisper.net.Ggml;
using Whisper.net;

var ggmlType = GgmlType.Base;
var modelFileName = "ggml-base.bin";
var wavFileName = "multichannel.wav";

if (!File.Exists(modelFileName))
{
    await DownloadModel(modelFileName, ggmlType);
}

using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

using var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")
    .Build();

using var fileStream = File.OpenRead(wavFileName);

await foreach (var result in processor.ProcessAsync(fileStream))
{
    // TODO: here, check the wave stream to see in which channel the diff is the highest for the specified time interval
    // 1. Get the wave position for the specified time interval
    // 2. Get the wave data for the specified time interval
    // 3. Iterate in the wave data to find the channel with the highest diff
    Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
}

static async Task DownloadModel(string fileName, GgmlType ggmlType)
{
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
}
