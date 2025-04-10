// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.Extensions.AI;
using Xunit;
using static Whisper.net.Tests.ProcessAsyncFunctionalTests;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net.Tests.SpeechToText;
public partial class WhisperSpeechToTextClientTest(TinyModelFixture model) : IClassFixture<TinyModelFixture>
{
    [Fact]
    public async Task TestHappyFlowAsync()
    {
        var segments = new List<SegmentData>();
        var updatesEnumerated = new List<SpeechToTextResponseUpdate>();
        var progress = new List<int>();

        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(model.ModelFile);

        var options = new SpeechToTextOptions()
                        .WithLanguage("en")
                        .WithEncoderBeginHandler((e) =>
                        {
                            encoderBegins.Add(e);
                            return true;
                        })
                        .WithProgressHandler(progress.Add)
                        .WithSegmentEventHandler(segments.Add);

        using var client = new WhisperSpeechToTextClient(() => factory);

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await foreach (var data in client.GetStreamingTextAsync(fileReader, options))
        {
            updatesEnumerated.Add(data);
        }

        Assert.Equal(segments, updatesEnumerated.Select(u => u.RawRepresentation));
        Assert.True(segments.Count > 0);
        Assert.True(progress.SequenceEqual(progress.OrderBy(x => x)));
        Assert.True(progress.Count > 1);
        Assert.Single(encoderBegins);
        Assert.Contains(segments, segmentData => segmentData.Text.Contains("nation should commit"));
        Assert.Contains(updatesEnumerated, update => update.Text.Contains("nation should commit"));
    }

    [Fact]
    public async Task WithSegmentEventHandler_Cancelled_WillCancellTheProcessing_AndDispose()
    {
        var segments = new List<SegmentData>();
        var segmentsEnumerated = new List<SpeechToTextResponseUpdate>();
        var cts = new CancellationTokenSource();
        TaskCanceledException? taskCanceledException = null;

        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(model.ModelFile);

        var options = new SpeechToTextOptions()
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
            });

        var client = new WhisperSpeechToTextClient(() => factory);

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        try
        {
            await foreach (var update in client.GetStreamingTextAsync(fileReader, options, cts.Token))
            {
                segmentsEnumerated.Add(update);
            }
        }
        catch (TaskCanceledException ex)
        {
            taskCanceledException = ex;
        }

        client.Dispose();

        Assert.Empty(segmentsEnumerated);
        Assert.Single(segments);
        Assert.Single(encoderBegins);
        Assert.NotNull(taskCanceledException);
        Assert.Contains(segments, segmentData => segmentData.Text.Contains("nation should commit"));
    }

    [Fact]
    public async Task ProcessAsync_WhenJunkChunkExists_ProcessCorrectly()
    {
        var segments = new List<SpeechToTextResponseUpdate>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        var options = new SpeechToTextOptions()
                        .WithLanguage("en");

        using var client = new WhisperSpeechToTextClient(() => factory);

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("junkchunk16khz.wav");
        await foreach (var update in client.GetStreamingTextAsync(fileReader, options))
        {
            segments.Add(update);
        }

        Assert.True(segments.Count >= 1);
    }

    [Fact]
    public async Task ProcessAsync_WhenMultichannel_ProcessCorrectly()
    {
        var segments = new List<SpeechToTextResponseUpdate>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        var options = new SpeechToTextOptions()
                        .WithLanguage("en");

        using var client = new WhisperSpeechToTextClient(() => factory);

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("multichannel.wav");
        await foreach (var update in client.GetStreamingTextAsync(fileReader, options))
        {
            segments.Add(update);
        }

        Assert.True(segments.Count >= 1);
    }

    [Fact]
    public async Task GetStreamingTextAsync_CalledMultipleTimes_Serially_WillCompleteEverytime()
    {
        var updates1 = new List<SpeechToTextResponseUpdate>();
        var updates2 = new List<SpeechToTextResponseUpdate>();
        var updates3 = new List<SpeechToTextResponseUpdate>();

        using var client = new WhisperSpeechToTextClient(model.ModelFile);
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
        using var client = new WhisperSpeechToTextClient(model.ModelFile);
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

    private class UpdateDataComparer : IEqualityComparer<SpeechToTextResponseUpdate>
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
