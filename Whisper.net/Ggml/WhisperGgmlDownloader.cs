// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Ggml;

public class WhisperGgmlDownloader(HttpClient httpClient)
{
    private const string HuggingFaceRepository = "https://huggingface.co/sandrohanea/whisper.net/resolve";
    private const string ModelVersion = "v4";

    private static readonly Lazy<WhisperGgmlDownloader> defaultInstance = new
        (
            () => new WhisperGgmlDownloader(new() { Timeout = TimeSpan.FromHours(1) })
        );

    /// <summary>
    /// The default instance of the downloader, which uses an unauthenticated client with a 1 hour timeout.
    /// </summary>
    /// <remarks>
    /// If running in an environment where the default timeout is not sufficient or
    /// multiple requests are being made from the same IP address (e.g. Github Actions with public runners),
    /// consider creating a new instance of the downloader with a custom <see cref="HttpClient"/> instance.
    /// The HttpClient should have a longer timeout and, if necessary, an authorization header with a Hugging Face token.
    /// </remarks>
    public static WhisperGgmlDownloader Default { get; } = defaultInstance.Value;

    /// <summary>
    /// Gets the download stream for the model
    /// </summary>
    /// <param name="type">The type of the model which needs to be downloaded.</param>
    /// <param name="quantization">The quantization of the model.</param>
    /// <param name="cancellationToken">A cancellation token used to cancell the request to huggingface.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<Stream> GetGgmlModelAsync(GgmlType type, QuantizationType quantization = QuantizationType.NoQuantization, CancellationToken cancellationToken = default)
    {
        var subdirectory = GetQuantizationSubdirectory(quantization);
        var modelName = GetModelName(type);

        var url = $"{HuggingFaceRepository}/{ModelVersion}/{subdirectory}/{modelName}.bin";
#if NETSTANDARD
        return await httpClient.GetStreamAsync(url);
#else
        return await httpClient.GetStreamAsync(url, cancellationToken);
#endif
    }

    /// <summary>
    /// Gets the download stream for the OpenVino model, which is a zip file.
    /// </summary>
    /// <param name="type">The type of the model which needs to be downloaded.</param>
    /// <param name="cancellationToken">A cancellation token used to stop the request to huggingface.</param>
    /// <returns></returns>
    public async Task<Stream> GetEncoderOpenVinoModelAsync(GgmlType type, CancellationToken cancellationToken = default)
    {
        var modelName = GetModelName(type);
        var url = $"{HuggingFaceRepository}/{ModelVersion}/openvino/{modelName}-encoder.zip";
#if NETSTANDARD
        return await httpClient.GetStreamAsync(url);
#else
        return await httpClient.GetStreamAsync(url, cancellationToken);
#endif
    }

    /// <summary>
    /// Gets the manifest file for the OpenVino model.
    /// </summary>
    /// <param name="type"> The type of the model which needs to be loaded</param>
    /// <returns></returns>
    public string GetOpenVinoManifestFileName(GgmlType type)
    {
        var modelName = GetModelName(type);
        return $"{modelName}-encoder.xml";
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
    public async Task<Stream> GetEncoderCoreMLModelAsync(GgmlType type, CancellationToken cancellationToken = default)
    {
        var modelName = GetModelName(type);
        var url = $"{HuggingFaceRepository}/{ModelVersion}/coreml/{modelName}-encoder.zip";

#if NETSTANDARD
        return await httpClient.GetStreamAsync(url);
#else
        return await httpClient.GetStreamAsync(url, cancellationToken);
#endif
    }

    /// <summary>
    /// Gets the download stream for the ggml Silero VAD model.
    /// </summary>
    /// <param name="type">The type of the Silero VAD model which needs to be downloaded.</param>
    /// <param name="cancellationToken">A cancellation token used to stop the request to huggingface.</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<Stream> GetGgmlSileroVadModelAsync(SileroVadType type = SileroVadType.V6_2_0, CancellationToken cancellationToken = default)
    {
        var modelName = GetSileroVadModelName(type);
        var url = $"{HuggingFaceRepository}/{ModelVersion}/vad/{modelName}.bin";

#if NETSTANDARD
        return await httpClient.GetStreamAsync(url);
#else
        return await httpClient.GetStreamAsync(url, cancellationToken);
#endif
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

    private static string GetSileroVadModelName(SileroVadType type)
    {
        return type switch
        {
            SileroVadType.V5_1_2 => "ggml-silero-v5.1.2",
            SileroVadType.V6_2_0 => "ggml-silero-v6.2.0",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
