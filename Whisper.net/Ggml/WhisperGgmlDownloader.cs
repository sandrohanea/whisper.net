// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.IO.Compression;

namespace Whisper.net.Ggml;

public static class WhisperGgmlDownloader
{
    private static readonly Lazy<HttpClient> httpClient = new(() => new HttpClient() { Timeout = Timeout.InfiniteTimeSpan });

    /// <summary>
    /// Gets the download stream for the model
    /// </summary>
    /// <param name="type">The type of the model which needs to be downloaded.</param>
    /// <param name="quantization">The quantization of the model.</param>
    /// <param name="cancellationToken">A cancellation token used to cancell the request to huggingface.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<Stream> GetGgmlModelAsync(GgmlType type, QuantizationType quantization = QuantizationType.NoQuantization, CancellationToken cancellationToken = default)
    {
        var subdirectory = GetQuantizationSubdirectory(quantization);
        var modelName = GetModelName(type);

        var url = $"https://huggingface.co/sandrohanea/whisper.net/resolve/v3/{subdirectory}/{modelName}.bin";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await httpClient.Value.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

#if NETSTANDARD
        return await response.Content.ReadAsStreamAsync();
#else
        return await response.Content.ReadAsStreamAsync(cancellationToken);
#endif
    }

    /// <summary>
    /// Gets the download stream for the CoreML model, which is a zip file.
    /// </summary>
    /// <param name="type">The type of the model which needs to be downloaded.</param>
    /// <param name="cancellationToken">A cancellation token used to stop the request to huggingface.</param>
    /// <remarks>
    /// Needs to be extracted on in the same directory as the ggml model, also ggml model needs to be loaded using file path, not stream.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<Stream> GetEncoderCoreMLModelAsync(GgmlType type, CancellationToken cancellationToken = default)
    {
        var modelName = GetModelName(type);
        var url = $"https://huggingface.co/sandrohanea/whisper.net/resolve/v3/coreml/{modelName}-encoder.zip";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await httpClient.Value.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

#if NETSTANDARD
        return await response.Content.ReadAsStreamAsync();
#else
        return await response.Content.ReadAsStreamAsync(cancellationToken);
#endif
    }

    /// <summary>
    /// Extracts the given zip stream to the given path.
    /// </summary>
    /// <param name="zipStream">The zip stream to be extracted.</param>
    /// <param name="path">The path.</param>
    /// <remarks>
    /// In order to work, you'll need to provide the same path as the ggml model.
    /// </remarks>
    /// <returns></returns>
    public static async Task ExtractToPath(this Task<Stream> zipStream, string path)
    {
        using var zipArchive = new ZipArchive(await zipStream, ZipArchiveMode.Read);
        zipArchive.ExtractToDirectory(path);
    }

    private static string GetModelName(GgmlType type)
    {
        return type switch
        {
            GgmlType.Tiny => "ggml-tiny",
            GgmlType.TinyEn => "ggml-tiny.en",
            GgmlType.Base => "ggml-base",
            GgmlType.BaseEn => "ggml-base.en",
            GgmlType.Small => "ggml-small",
            GgmlType.SmallEn => "ggml-small.en",
            GgmlType.Medium => "ggml-medium",
            GgmlType.MediumEn => "ggml-medium.en",
            GgmlType.LargeV1 => "ggml-large-v1",
            GgmlType.LargeV2 => "ggml-large-v2",
            GgmlType.LargeV3 => "ggml-large-v3",
            GgmlType.LargeV3Turbo => "ggml-large-v3-turbo",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private static string GetQuantizationSubdirectory(QuantizationType quantization)
    {
        return quantization switch
        {
            QuantizationType.NoQuantization => "classic",
            QuantizationType.Q4_0 => "q4_0",
            QuantizationType.Q4_1 => "q4_1",
            QuantizationType.Q5_0 => "q5_0",
            QuantizationType.Q5_1 => "q5_1",
            QuantizationType.Q8_0 => "q8_0",
            _ => throw new ArgumentOutOfRangeException(nameof(quantization), quantization, null)
        };
    }
}
