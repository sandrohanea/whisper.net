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
        var ggmlModelPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bin");
        var model = await WhisperGgmlDownloader.GetGgmlModelAsync(type, quantizationType);
        using var fileWriter = File.OpenWrite(ggmlModelPath);
        await model.CopyToAsync(fileWriter);
        return ggmlModelPath;
    }
}
