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
        await using var fileStream = File.Create(predownloadModelPath);
        await model.CopyToAsync(fileStream);
        Console.WriteLine($"Written {predownloadModelPath} with {fileStream.Length} bytes");
    }
}

IEnumerable<SileroVadType> vadTypes = [SileroVadType.V5_1_2, SileroVadType.V6_2_0];
foreach (var vadType in vadTypes)
{
    await using var model = await WhisperGgmlDownloader.Default.GetGgmlSileroVadModelAsync(vadType);

    var predownloadModelPath = Path.Combine(predownloadedPath, GetSileroVadFileName(vadType));
    await using var fileStream = File.Create(predownloadModelPath);
    await model.CopyToAsync(fileStream);
    Console.WriteLine($"Written {predownloadModelPath} with {fileStream.Length} bytes");
}

return 0;

static string GetSileroVadFileName(SileroVadType type)
{
    return type switch
    {
        SileroVadType.V5_1_2 => "ggml-silero-v5.1.2.bin",
        SileroVadType.V6_2_0 => "ggml-silero-v6.2.0.bin",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown Silero VAD model type.")
    };
}
