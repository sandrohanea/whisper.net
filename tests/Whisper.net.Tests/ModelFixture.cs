// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Ggml;
using Xunit;

namespace Whisper.net.Tests;

public class TinyModelFixture() : ModelFixture(GgmlType.Tiny, QuantizationType.NoQuantization);

public class TinyQuantizedModelFixture() : ModelFixture(GgmlType.Tiny, QuantizationType.Q5_0);

public class SileroVadModelFixture() : VadModelFixture(SileroVadType.V6_2_0);

public abstract class ModelFixture(GgmlType type, QuantizationType quantizationType) : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        ModelFile = await DownloadModelAsync(type, quantizationType);
    }

    public Task DisposeAsync()
    {
        if (File.Exists(ModelFile))
        {
            File.Delete(ModelFile);
        }

        return Task.CompletedTask;
    }

    public string ModelFile { get; private set; } = string.Empty;

    private static async Task<string> DownloadModelAsync(GgmlType type, QuantizationType quantizationType = QuantizationType.NoQuantization)
    {
        var huggingFaceToken = Environment.GetEnvironmentVariable("HF_TOKEN");
        var predownloadedPath = Environment.GetEnvironmentVariable("WHISPER_TEST_MODEL_PATH");
        var modelFileName = $"ggml-{type.ToString().ToLowerInvariant()}-{quantizationType.ToString().ToLowerInvariant()}.bin";

        var ggmlModelPath = Path.Combine(Path.GetTempPath(), $"fișier-împânzit-utf8-{Guid.NewGuid()}.bin");

        // If a pre-downloaded model is specified, copy it into place and return immediately.
        if (!string.IsNullOrWhiteSpace(predownloadedPath))
        {
            var predownloadModelPath = Path.Combine(predownloadedPath, modelFileName);
            if (File.Exists(predownloadModelPath))
            {
                File.Copy(predownloadModelPath, ggmlModelPath, overwrite: true);
                Console.WriteLine($"Using pre-downloaded model from '{predownloadedPath}'.");
                return ggmlModelPath;
            }

            throw new Exception(
                $"Pre-downloaded model not found at '{predownloadModelPath}'. Either remove the env variable, or download the model manually and place it at '{predownloadModelPath}'."
            );
        }

        if (TestDataProvider.OpenModelFileStreamAsync is { } openModelFileStreamAsync)
        {
            using var packagedModel = await openModelFileStreamAsync(modelFileName);
            using var packagedModelWriter = File.Create(ggmlModelPath);
            await packagedModel.CopyToAsync(packagedModelWriter);
            Console.WriteLine($"Using packaged model '{modelFileName}'.");
            return ggmlModelPath;
        }

        // If you have a Hugging Face token, you can use it to download the model (to avoid rate limiting)
        // Otherwise, the default downloader will be used
        var downloader = string.IsNullOrEmpty(huggingFaceToken)
            ? WhisperGgmlDownloader.Default
            : new(
                new() { DefaultRequestHeaders = { { "Authorization", $"Bearer {huggingFaceToken}" } }, Timeout = TimeSpan.FromHours(1) });
        using var model = await downloader.GetGgmlModelAsync(type, quantizationType);
        using var fileWriter = File.OpenWrite(ggmlModelPath);
        await model.CopyToAsync(fileWriter);
        return ggmlModelPath;
    }
}

public abstract class VadModelFixture(SileroVadType type) : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        ModelFile = await DownloadModelAsync(type);
    }

    public Task DisposeAsync()
    {
        if (File.Exists(ModelFile))
        {
            File.Delete(ModelFile);
        }

        return Task.CompletedTask;
    }

    public string ModelFile { get; private set; } = string.Empty;

    private static async Task<string> DownloadModelAsync(SileroVadType type)
    {
        var huggingFaceToken = Environment.GetEnvironmentVariable("HF_TOKEN");
        var predownloadedPath = Environment.GetEnvironmentVariable("WHISPER_TEST_MODEL_PATH");

        var modelFileName = GetModelFileName(type);
        var vadModelPath = Path.Combine(Path.GetTempPath(), $"fișier-împânzit-utf8-{Guid.NewGuid()}.bin");

        if (!string.IsNullOrWhiteSpace(predownloadedPath))
        {
            var predownloadModelPath = Path.Combine(predownloadedPath, modelFileName);
            if (File.Exists(predownloadModelPath))
            {
                File.Copy(predownloadModelPath, vadModelPath, overwrite: true);
                Console.WriteLine($"Using pre-downloaded VAD model from '{predownloadedPath}'.");
                return vadModelPath;
            }

            throw new Exception(
                $"Pre-downloaded VAD model not found at '{predownloadModelPath}'. Either remove the env variable, or download the model manually and place it at '{predownloadModelPath}'."
            );
        }

        if (TestDataProvider.OpenModelFileStreamAsync is { } openModelFileStreamAsync)
        {
            using var packagedModel = await openModelFileStreamAsync(modelFileName);
            using var packagedModelWriter = File.Create(vadModelPath);
            await packagedModel.CopyToAsync(packagedModelWriter);
            Console.WriteLine($"Using packaged VAD model '{modelFileName}'.");
            return vadModelPath;
        }

        var downloader = string.IsNullOrEmpty(huggingFaceToken)
            ? WhisperGgmlDownloader.Default
            : new(
                new() { DefaultRequestHeaders = { { "Authorization", $"Bearer {huggingFaceToken}" } }, Timeout = TimeSpan.FromHours(1) });
        using var model = await downloader.GetGgmlSileroVadModelAsync(type);
        using var fileWriter = File.Create(vadModelPath);
        await model.CopyToAsync(fileWriter);
        return vadModelPath;
    }

    private static string GetModelFileName(SileroVadType type)
    {
        return type switch
        {
            SileroVadType.V5_1_2 => "ggml-silero-v5.1.2.bin",
            SileroVadType.V6_2_0 => "ggml-silero-v6.2.0.bin",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown Silero VAD model type.")
        };
    }
}
