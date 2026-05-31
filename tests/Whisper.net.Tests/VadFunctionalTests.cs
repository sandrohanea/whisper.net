// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Xunit;
using Xunit.Extensions.AssemblyFixture;
using Whisper.net.Wave;

namespace Whisper.net.Tests;

public class VadFunctionalTests(SileroVadModelFixture model) : IAssemblyFixture<SileroVadModelFixture>
{
    private static readonly TimeSpan TestAudioDuration = TimeSpan.FromSeconds(3);

    [Fact]
    public async Task DetectSpeechAsync_WhenUsingSileroVadModel_DetectsExpectedSegments()
    {
        using var factory = WhisperVadFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
            .WithThreshold(0.5f)
            .Build();

        var samples = await ReadTestSamplesAsync();
        var segments = await processor.DetectSpeechAsync(samples);

        AssertSegmentsDetected(segments);
    }

    [Fact]
    public async Task DetectSpeech_WhenUsingSileroVadModel_DetectsExpectedSegments()
    {
        using var factory = WhisperVadFactory.FromPath(model.ModelFile);
        using var processor = factory.CreateBuilder()
            .WithThreshold(0.5f)
            .Build();

        var samples = await ReadTestSamplesAsync();
        var segments = processor.DetectSpeech(samples);

        AssertSegmentsDetected(segments);
    }

    private static async Task<float[]> ReadTestSamplesAsync()
    {
        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var parser = new WaveParser(fileReader);
        var samples = await parser.GetAvgSamplesAsync();
        var testSampleCount = Math.Min(samples.Length, (int)(parser.SampleRate * TestAudioDuration.TotalSeconds));
        var testSamples = new float[testSampleCount];
        Array.Copy(samples, testSamples, testSampleCount);
        return testSamples;
    }

    private static void AssertSegmentsDetected(IReadOnlyList<VadSegmentData> segments)
    {
        Assert.Single(segments);

        Assert.InRange(segments[0].Start, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300));
        Assert.InRange(segments[0].End, TimeSpan.FromMilliseconds(2700), TimeSpan.FromMilliseconds(2800));
    }
}
