// See https://aka.ms/new-console-template for more information

using Whisper.net.Ggml;

var predownloadedPath = Environment.GetEnvironmentVariable("WHISPER_TEST_MODEL_PATH");

if (predownloadedPath is null)
{
    Console.WriteLine("Please set the WHISPER_TEST_MODEL_PATH environment variable to a path where the model will be downloaded.");
    return -1;
}

if (!Directory.Exists(predownloadedPath))
{
    Directory.CreateDirectory(predownloadedPath);
}

IEnumerable<GgmlType> types = [GgmlType.Tiny];
IEnumerable<QuantizationType> quantizations = [QuantizationType.NoQuantization, QuantizationType.Q5_0];
foreach (var type in types)
{
    foreach (var quantizationType in quantizations)
    {
        await using var model = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(type, quantizationType);

        var predownloadModelPath = Path.Combine(predownloadedPath, $"ggml-{type.ToString().ToLowerInvariant()}-{quantizationType.ToString().ToLowerInvariant()}.bin");
        await using var fileStream = File.OpenWrite(predownloadModelPath);
        await model.CopyToAsync(fileStream);
        Console.WriteLine($"Written {predownloadModelPath} with {fileStream} bytes");
    }
}
return 0;
