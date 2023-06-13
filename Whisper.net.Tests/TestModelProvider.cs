// Licensed under the MIT license: https://opensource.org/licenses/MIT

using NUnit.Framework;
using Whisper.net.Ggml;

namespace Whisper.net.Tests;

[SetUpFixture]
internal class TestModelProvider
{
    private static string ggmlModelTiny = string.Empty;
    private static string ggmlModelTinyQ5 = string.Empty;

    [OneTimeSetUp]
    public static async Task SetupAsync()
    {
        ggmlModelTiny = await DownloadModelAsync(GgmlType.Tiny);
        ggmlModelTinyQ5 = await DownloadModelAsync(GgmlType.Tiny, QuantizationType.Q5_0);
    }

    private static async Task<string> DownloadModelAsync(GgmlType type, QuantizationType quantizationType = QuantizationType.NoQuantization)
    {
        var ggmlModelPath = Path.GetTempFileName();
        var model = await WhisperGgmlDownloader.GetGgmlModelAsync(type, quantizationType);
        using var fileWriter = File.OpenWrite(ggmlModelPath);
        await model.CopyToAsync(fileWriter);
        return ggmlModelPath;
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        File.Delete(ggmlModelTiny);
        File.Delete(ggmlModelTinyQ5);
    }

    public static string GgmlModelTiny => ggmlModelTiny;
    public static string GgmlModelTinyQ5 => ggmlModelTinyQ5;
}
