// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Net;
using System.Net.Http;
using Whisper.net.Ggml;
using Xunit;

namespace Whisper.net.Tests;

public class GgmlDownloaderTests
{
    [Fact]
    public async Task GetGgmlModelAsync_ShouldDownloadFromCurrentModelVersion()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetGgmlModelAsync(GgmlType.Base);

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v4/classic/ggml-base.bin"), handler.RequestUri);
    }

    [Fact]
    public async Task GetEncoderOpenVinoModelAsync_ShouldDownloadFromCurrentModelVersion()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetEncoderOpenVinoModelAsync(GgmlType.Base);

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v4/openvino/ggml-base-encoder.zip"), handler.RequestUri);
    }

    [Fact]
    public async Task GetEncoderCoreMLModelAsync_ShouldDownloadFromCurrentModelVersion()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetEncoderCoreMLModelAsync(GgmlType.Base);

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v4/coreml/ggml-base-encoder.zip"), handler.RequestUri);
    }

    [Fact]
    public async Task GetGgmlSileroVadModelAsync_ShouldDownloadDefaultSileroVadModel()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetGgmlSileroVadModelAsync();

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v4/vad/ggml-silero-v6.2.0.bin"), handler.RequestUri);
    }

    [Fact]
    public async Task GetGgmlSileroVadModelAsync_ShouldDownloadRequestedSileroVadModel()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetGgmlSileroVadModelAsync(SileroVadType.V5_1_2);

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v4/vad/ggml-silero-v5.1.2.bin"), handler.RequestUri);
    }

    [Fact]
    public async Task GetGgmlSileroVadModelAsync_WithInvalidType_ShouldThrow()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => downloader.GetGgmlSileroVadModelAsync((SileroVadType)999));
        Assert.Null(handler.RequestUri);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(Array.Empty<byte>())
            };

            return Task.FromResult(response);
        }
    }
}
