// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Ggml;

public static class WhisperGgmlDownloader
{
    private static readonly Lazy<HttpClient> httpClient = new(() => new HttpClient(){Timeout = Timeout.InfiniteTimeSpan});

    public static async Task<Stream> GetGgmlModelAsync(GgmlType type, CancellationToken cancellationToken)
    {
        var url = type switch
        {
            GgmlType.Tiny => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny.bin",
            GgmlType.TinyEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-tiny.en.bin",
            GgmlType.Base => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base.bin",
            GgmlType.BaseEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-base.en.bin",
            GgmlType.Small => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small.bin",
            GgmlType.SmallEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-small.en.bin",
            GgmlType.Medium => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium.bin",
            GgmlType.MediumEn => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-medium.en.bin",
            GgmlType.LargeV1 => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large-v1.bin",
            GgmlType.Large => "https://huggingface.co/datasets/ggerganov/whisper.cpp/resolve/main/ggml-large.bin",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await httpClient.Value.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStreamAsync();
    }
}
