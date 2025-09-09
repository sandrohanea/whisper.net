// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Wave;
using Xunit;

namespace Whisper.net.Tests;

public class WaveParserTests
{
    [Fact]
    public async Task Initialize_WithTruncatedStream_ShouldThrow()
    {
        using var stream = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");

        var truncatedBytes = new byte[stream.Length - 1000];
#if NETFRAMEWORK
        var read = stream.Read(truncatedBytes, 0, truncatedBytes.Length);
#else
        var read = await stream.ReadAsync(truncatedBytes);
#endif
        Assert.Equal(truncatedBytes.Length, read);

        using var truncated = new MemoryStream(truncatedBytes);

        var parser = new WaveParser(truncated);

        await Assert.ThrowsAsync<CorruptedWaveException>(() => parser.GetAvgSamplesAsync());
    }

    [Fact]
    public async Task Initialize_WithTruncatedStreamAndPermissiveFlag_ShouldNotThrow()
    {
        using var stream = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");

        var truncatedBytes = new byte[stream.Length - 1000];
#if NETFRAMEWORK
        var read = stream.Read(truncatedBytes, 0, truncatedBytes.Length);
#else
        var read = await stream.ReadAsync(truncatedBytes);
#endif
        Assert.Equal(truncatedBytes.Length, read);

        using var truncated = new MemoryStream(truncatedBytes);

        var parser = new WaveParser(truncated, new WaveParserOptions { AllowLessSamples = true });

        var samples = await parser.GetAvgSamplesAsync();

        Assert.NotNull(samples);
    }
}

