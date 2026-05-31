// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Whisper.net.Tests;

public class VadFunctionalTests(SileroVadModelFixture model) : IAssemblyFixture<SileroVadModelFixture>
{
    [Fact]
    public async Task DetectSpeechAsync_WhenUsingSileroVadModel_DetectsExpectedSegments()
    {
        using var factory = WhisperVadFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
            .WithThreshold(0.5f)
            .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var segments = await processor.DetectSpeechAsync(fileReader);

        AssertSegmentsDetected(segments);
    }

    [Fact]
    public async Task DetectSpeech_WhenUsingSileroVadModel_DetectsExpectedSegments()
    {
        using var factory = WhisperVadFactory.FromPath(model.ModelFile);
        using var processor = factory.CreateBuilder()
            .WithThreshold(0.5f)
            .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var segments = processor.DetectSpeech(fileReader);

        AssertSegmentsDetected(segments);
    }

    private static void AssertSegmentsDetected(IReadOnlyList<VadSegmentData> segments)
    {
        Assert.Equal(7, segments.Count);

        Assert.InRange(segments[0].Start, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300));
        Assert.InRange(segments[0].End, TimeSpan.FromMilliseconds(2700), TimeSpan.FromMilliseconds(2800));

        Assert.InRange(segments[6].Start, TimeSpan.FromMilliseconds(17500), TimeSpan.FromMilliseconds(17600));
        Assert.InRange(segments[6].End, TimeSpan.FromMilliseconds(20900), TimeSpan.FromMilliseconds(21100));
    }
}
