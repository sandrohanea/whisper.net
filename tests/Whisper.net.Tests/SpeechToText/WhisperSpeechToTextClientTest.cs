// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.Extensions.AI;
using Xunit;
using static Whisper.net.Tests.ProcessAsyncFunctionalTests;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net.Tests;
public partial class WhisperSpeechToTextClientTest(TinyModelFixture model) : IClassFixture<TinyModelFixture>
{
    /*
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

        Assert.Equal(segments, segmentsEnumerated);
        Assert.True(segments.Count > 0);
        Assert.True(progress.SequenceEqual(progress.OrderBy(x => x)));
        Assert.True(progress.Count > 1);
        Assert.Single(encoderBegins);
        Assert.Contains(segments, segmentData => segmentData.Text.Contains("nation should commit"));
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

        Assert.Empty(segmentsEnumerated);
        Assert.Single( segments);
        Assert.Single( encoderBegins);
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
    }*/

    [Fact]
    public async Task GetStreamingTextAsync_CalledMultipleTimes_Serially_WillCompleteEverytime()
    {
        var updates1 = new List<SpeechToTextResponseUpdate>();
        var updates2 = new List<SpeechToTextResponseUpdate>();
        var updates3 = new List<SpeechToTextResponseUpdate>();

        var client = new WhisperSpeechToTextClient(model.ModelFile);
        var options = new SpeechToTextOptions().WithLanguage("en");

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var update in client.GetStreamingTextAsync(fileReader, options))
        {
            updates1.Add(update);
        }

        using var fileReader2 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var update in client.GetStreamingTextAsync(fileReader2, options))
        {
            updates2.Add(update);
        }

        using var fileReader3 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var update in client.GetStreamingTextAsync(fileReader3, options))
        {
            updates3.Add(update);
        }

        Assert.True(updates1.SequenceEqual(updates2, new UpdateDataComparer()));
        Assert.True(updates2.SequenceEqual(updates3, new UpdateDataComparer()));
    }

    [Fact]
    public async Task GetTextAsync_CalledMultipleTimes_Serially_WillCompleteEverytime()
    {
        var client = new WhisperSpeechToTextClient(model.ModelFile);
        var options = new SpeechToTextOptions().WithLanguage("en");

        using var fileReader1 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var result1 = await client.GetTextAsync(fileReader1, options);
        var segments1 = Assert.IsAssignableFrom<IEnumerable<SegmentData>>(result1.RawRepresentation);

        using var fileReader2 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var result2 = await client.GetTextAsync(fileReader2, options);
        var segments2 = Assert.IsAssignableFrom<IEnumerable<SegmentData>>(result2.RawRepresentation);

        using var fileReader3 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var result3 = await client.GetTextAsync(fileReader3, options);
        var segments3 = Assert.IsAssignableFrom<IEnumerable<SegmentData>>(result3.RawRepresentation);
        

        Assert.True(segments1.SequenceEqual(segments2, new SegmentDataComparer()));
        Assert.True(segments2.SequenceEqual(segments3, new SegmentDataComparer()));
    }

    private  class UpdateDataComparer : IEqualityComparer<SpeechToTextResponseUpdate>
    {
        public bool Equals(SpeechToTextResponseUpdate? xUpdate, SpeechToTextResponseUpdate? yUpdate)
        {
            if (xUpdate == null || yUpdate == null)
            {
                return false;
            }

            var x = (yUpdate.RawRepresentation as SegmentData)!;
            var y = (yUpdate.RawRepresentation as SegmentData)!;

            return x.Text == y.Text && x.MinProbability == y.MinProbability && x.Probability == y.Probability && x.Start == y.Start && x.End == y.End; // Compare by relevant properties
        }

        public int GetHashCode(SpeechToTextResponseUpdate obj)
        {
            return obj.Text.GetHashCode();
        }
    }
}
