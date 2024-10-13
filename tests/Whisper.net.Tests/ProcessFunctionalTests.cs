// Licensed under the MIT license: https://opensource.org/licenses/MIT

using FluentAssertions;
using Xunit;

namespace Whisper.net.Tests;

public class ProcessFunctionalTests(TinyModelFixture model) : IClassFixture<TinyModelFixture>
{
    [Fact]
    public async Task TestHappyFlow()
    {
        var segments = new List<SegmentData>();
        var progress = new List<int>();
        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(model.ModelFile);
        using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .WithEncoderBeginHandler((e) =>
                        {
                            encoderBegins.Add(e);
                            return true;
                        })
                        .WithPrompt("I am Kennedy")
                        .WithProgressHandler(progress.Add)
                        .WithSegmentEventHandler(segments.Add)
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        processor.Process(fileReader);

        segments.Should().HaveCountGreaterThan(0);
        encoderBegins.Should().HaveCount(1);
        progress.Should().BeInAscendingOrder().And.HaveCountGreaterThan(1);

        segments.Should().Contain(segmentData => segmentData.Text.Contains("nation should commit"));
    }

    [Fact]
    public async Task TestCancelEncoder()
    {
        var segments = new List<SegmentData>();
        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(model.ModelFile);
        using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .WithEncoderBeginHandler((e) =>
                        {
                            encoderBegins.Add(e);
                            return false;
                        })
                        .WithSegmentEventHandler(segments.Add)
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        processor.Process(fileReader);

        segments.Should().HaveCount(0);
        encoderBegins.Should().HaveCount(1);
    }

    [Fact]
    public async Task TestAutoDetectLanguageWithRomanian()
    {
        var segments = new List<SegmentData>();
        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(model.ModelFile);
        using var processor = factory.CreateBuilder()
                        .WithLanguageDetection()
                        .WithEncoderBeginHandler((e) =>
                        {
                            encoderBegins.Add(e);
                            return true;
                        })
                        .Build();
        using var fileReader = await TestDataProvider.OpenFileStreamAsync("romana.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader))
        {
            segments.Add(segment);
        }
        segments.Should().HaveCountGreaterThan(0);
        encoderBegins.Should().HaveCountGreaterThanOrEqualTo(1);
        segments.Should().AllSatisfy(s => s.Language.Should().Be("ro"));
        segments.Should().Contain(segmentData => segmentData.Text.Contains("efectua"));
    }

    [Fact]
    public async Task Process_WhenMultichannel_ProcessCorrectly()
    {
        var segments = new List<SegmentData>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .WithSegmentEventHandler(segments.Add)
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("multichannel.wav");
        processor.Process(fileReader);

        segments.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task Process_CalledMultipleTimes_Serially_WillCompleteEverytime()
    {

        var segments1 = new List<SegmentData>();
        var segments2 = new List<SegmentData>();
        var segments3 = new List<SegmentData>();

        OnSegmentEventHandler onNewSegment = segments1.Add;

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .WithSegmentEventHandler((s) => onNewSegment(s))
                        .Build();

        using var fileReader1 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        processor.Process(fileReader1);

        onNewSegment = segments2.Add;

        using var fileReader2 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        processor.Process(fileReader2);

        onNewSegment = segments3.Add;

        using var fileReader3 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        processor.Process(fileReader3);

        segments1.Should().BeEquivalentTo(segments2);
        segments2.Should().BeEquivalentTo(segments3);
    }
}
