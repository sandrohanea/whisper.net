// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Wave;
using Xunit;

namespace Whisper.net.Tests;

public partial class ProcessAsyncFunctionalTests(TinyModelFixture model) : IClassFixture<TinyModelFixture>
{
    [Fact]
    public async Task TestHappyFlowAsync()
    {
        var segments = new List<SegmentData>();
        var segmentsEnumerated = new List<SegmentData>();
        var progress = new List<int>();

        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
            .WithLanguage("en")
            .WithEncoderBeginHandler((e) =>
            {
                encoderBegins.Add(e);
                return true;
            })
            .WithProgressHandler(progress.Add)
            .WithSegmentEventHandler(segments.Add)
            .Build();

        var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var data in processor.ProcessAsync(fileReader))
        {
            segmentsEnumerated.Add(data);
        }

        Assert.Equal(segments, segmentsEnumerated);
        Assert.True(segments.Count > 0);
        Assert.True(progress.SequenceEqual(progress.OrderBy(x => x)));
        Assert.True(progress.Count > 1);
        Assert.Single(encoderBegins);
        Assert.Contains(segments, segmentData => segmentData.Text.Contains("nation should commit"));
    }

    [Fact]
    public async Task ProcessAsync_Cancelled_WillCancelTheProcessing_AndDispose_WillWaitUntilFullyFinished()
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

        var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
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

        Assert.Empty(segmentsEnumerated);
        Assert.Single(segments);
        Assert.Single(encoderBegins);
        Assert.NotNull(taskCanceledException);
        Assert.Contains(segments, segmentData => segmentData.Text.Contains("nation should commit"));
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

        Assert.True(segments.Count >= 1);
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

        Assert.True(segments.Count >= 1);
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

        Assert.True(segments1.SequenceEqual(segments2, new SegmentDataComparer()));
        Assert.True(segments2.SequenceEqual(segments3, new SegmentDataComparer()));
    }

    [Fact]
    public async Task ProcessAsync_ParallelExecution_WillCompleteEverytime()
    {
        var segments1 = new List<SegmentData>();
        var segments2 = new List<SegmentData>();
        var segments3 = new List<SegmentData>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var waveParser = new WaveParser(fileReader);
        var samples = await waveParser.GetAvgSamplesAsync();

        var task1 = Task.Run(async () =>
        {
            await using var processor = factory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            await foreach (var segment in processor.ProcessAsync(samples))
            {
                Thread.Sleep(300);
                segments1.Add(segment);
            }
        });

        var task2 = Task.Run(async () =>
        {
            await using var processor = factory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            await foreach (var segment in processor.ProcessAsync(samples))
            {
                Thread.Sleep(300);
                segments2.Add(segment);
            }
        });


        var task3 = Task.Run(async () =>
        {
            await using var processor = factory.CreateBuilder()
                .WithLanguage("en")
                .Build();

            await foreach (var segment in processor.ProcessAsync(samples))
            {
                Thread.Sleep(300);
                segments3.Add(segment);
            }
        });

        await Task.WhenAll(task1, task2, task3);

        // Assert
        Assert.True(segments1.SequenceEqual(segments2, new SegmentDataComparer()));
        Assert.True(segments2.SequenceEqual(segments3, new SegmentDataComparer()));
        Assert.True(segments1.SequenceEqual(segments3, new SegmentDataComparer()));
    }
}
