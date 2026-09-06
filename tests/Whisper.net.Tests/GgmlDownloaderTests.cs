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

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v5/classic/ggml-base.bin"), handler.RequestUri);
    }

    [Fact]
    public async Task GetEncoderOpenVinoModelAsync_ShouldDownloadFromCurrentModelVersion()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetEncoderOpenVinoModelAsync(GgmlType.Base);

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v5/openvino/ggml-base-encoder.zip"), handler.RequestUri);
    }

    [Fact]
    public async Task GetEncoderCoreMLModelAsync_ShouldDownloadFromCurrentModelVersion()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetEncoderCoreMLModelAsync(GgmlType.Base);

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v5/coreml/ggml-base-encoder.zip"), handler.RequestUri);
    }

    [Fact]
    public async Task GetGgmlSileroVadModelAsync_ShouldDownloadDefaultSileroVadModel()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetGgmlSileroVadModelAsync();

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v5/vad/ggml-silero-v6.2.0.bin"), handler.RequestUri);
    }

    [Fact]
    public async Task GetGgmlSileroVadModelAsync_ShouldDownloadRequestedSileroVadModel()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetGgmlSileroVadModelAsync(SileroVadType.V5_1_2);

        Assert.Equal(new Uri("https://huggingface.co/sandrohanea/whisper.net/resolve/v5/vad/ggml-silero-v5.1.2.bin"), handler.RequestUri);
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

    [Theory]
    [InlineData(ParakeetQuantizationType.F32, "f32")]
    [InlineData(ParakeetQuantizationType.F16, "f16")]
    [InlineData(ParakeetQuantizationType.Q8_0, "q8_0")]
    [InlineData(ParakeetQuantizationType.Q4_0, "q4_0")]
    [InlineData(ParakeetQuantizationType.Q4_K, "q4_k")]
    public async Task GetGgmlParakeetModelAsync_ShouldDownloadVersionedMirrorVariant(
        ParakeetQuantizationType quantization,
        string fileSuffix)
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        using var _ = await downloader.GetGgmlParakeetModelAsync(ParakeetModelType.Tdt0_6B_V3, quantization);

        Assert.Equal(
            new Uri($"https://huggingface.co/sandrohanea/whisper.net/resolve/v5/parakeet/ggml-parakeet-tdt-0.6b-v3-{fileSuffix}.bin"),
            handler.RequestUri);
    }

    [Fact]
    public async Task GetGgmlParakeetModelAsync_WithInvalidModelType_ShouldThrow()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => downloader.GetGgmlParakeetModelAsync((ParakeetModelType)999, ParakeetQuantizationType.F16));

        Assert.Equal("type", exception.ParamName);
        Assert.Null(handler.RequestUri);
    }

    [Fact]
    public async Task GetGgmlParakeetModelAsync_WithInvalidQuantizationType_ShouldThrow()
    {
        using var handler = new CapturingHandler();
        using var httpClient = new HttpClient(handler);
        var downloader = new WhisperGgmlDownloader(httpClient);

        var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => downloader.GetGgmlParakeetModelAsync(ParakeetModelType.Tdt0_6B_V3, (ParakeetQuantizationType)999));

        Assert.Equal("quantization", exception.ParamName);
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
