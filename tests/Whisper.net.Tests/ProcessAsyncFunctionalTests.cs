// Licensed under the MIT license: https://opensource.org/licenses/MIT

using FluentAssertions;
using Xunit;

namespace Whisper.net.Tests;
public class ProcessAsyncFunctionalTests(TinyModelFixture model) : IClassFixture<TinyModelFixture>
{
    [Fact]
    public async Task TestHappyFlowAsync()
    {
        var segments = new List<SegmentData>();
        var segmentsEnumerated = new List<SegmentData>();
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
                        .WithProgressHandler(progress.Add)
                        .WithSegmentEventHandler(segments.Add)
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var data in processor.ProcessAsync(fileReader))
        {
            segmentsEnumerated.Add(data);
        }

        segmentsEnumerated.Should().BeEquivalentTo(segments);

        segments.Should().HaveCountGreaterThan(0);
        progress.Should().BeInAscendingOrder().And.HaveCountGreaterThan(1);
        encoderBegins.Should().HaveCount(1);

        segments.Should().Contain(segmentData => segmentData.Text.Contains("nation should commit"));
    }

    [Fact]
    public async Task ProcessAsync_Cancelled_WillCancellTheProcessing_AndDispose_WillWaitUntilFullyFinished()
    {
        var segments = new List<SegmentData>();
        var segmentsEnumerated = new List<SegmentData>();
        var cts = new CancellationTokenSource();
        TaskCanceledException? taskCanceledException = null;

        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(model.ModelFile);
        var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .WithEncoderBeginHandler((e) =>
                        {
                            encoderBegins.Add(e);
                            return true;
                        })
                        .WithSegmentEventHandler(s =>
                        {
                            segments.Add(s);
                            cts.Cancel();
                        })
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        try
        {
            await foreach (var data in processor.ProcessAsync(fileReader, cts.Token))
            {
                segmentsEnumerated.Add(data);
            }
        }
        catch (TaskCanceledException ex)
        {
            taskCanceledException = ex;
        }

        await processor.DisposeAsync();

        segmentsEnumerated.Should().BeEmpty();

        segments.Should().HaveCount(1);
        encoderBegins.Should().HaveCount(1);
        taskCanceledException.Should().NotBeNull();

        segments.Should().Contain(segmentData => segmentData.Text.Contains("nation should commit"));
    }

    [Fact]
    public async Task ProcessAsync_WhenJunkChunkExists_ProcessCorrectly()
    {
        var segments = new List<SegmentData>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("junkchunk16khz.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader))
        {
            segments.Add(segment);
        }

        segments.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ProcessAsync_WhenMultichannel_ProcessCorrectly()
    {
        var segments = new List<SegmentData>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("multichannel.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader))
        {
            segments.Add(segment);
        }

        segments.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task ProcessAsync_CalledMultipleTimes_Serially_WillCompleteEverytime()
    {

        var segments1 = new List<SegmentData>();
        var segments2 = new List<SegmentData>();
        var segments3 = new List<SegmentData>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader))
        {
            segments1.Add(segment);
        }

        using var fileReader2 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader2))
        {
            segments2.Add(segment);
        }

        using var fileReader3 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader3))
        {
            segments3.Add(segment);
        }

        segments1.Should().BeEquivalentTo(segments2);
        segments2.Should().BeEquivalentTo(segments3);
    }
}
