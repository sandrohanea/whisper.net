// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Ggml;
using Xunit;

namespace Whisper.net.Tests;

public class TinyModelFixture() : ModelFixture(GgmlType.Tiny, QuantizationType.NoQuantization);
public class TinyQuantizedModelFixture() : ModelFixture(GgmlType.Tiny, QuantizationType.Q5_0);

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
        var ggmlModelPath = Path.Combine(Path.GetTempPath(), $"fișier-împânzit-utf8-{Guid.NewGuid()}.bin");

        // If you have a Hugging Face token, you can use it to download the model (to avoid rate limiting)
        // Otherwise, the default downloader will be used
        var downloader = string.IsNullOrEmpty(huggingFaceToken)
            ? WhisperGgmlDownloader.Default
            : new(
                new()
                {
                    DefaultRequestHeaders =
                    {
                        { "Authorization", $"Bearer {huggingFaceToken}" }
                    },
                    Timeout = TimeSpan.FromHours(1)
                });
        using var model = await downloader.GetGgmlModelAsync(type, quantizationType);
        using var fileWriter = File.OpenWrite(ggmlModelPath);
        await model.CopyToAsync(fileWriter);
        return ggmlModelPath;
    }
}
