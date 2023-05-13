// Licensed under the MIT license: https://opensource.org/licenses/MIT

using FluentAssertions;
using NUnit.Framework;
using Whisper.net.Ggml;

namespace Whisper.net.Tests;

public class ProcessorE2ETests
{
    private string ggmlModelPath = string.Empty;

    [OneTimeSetUp]
    public async Task SetupAsync()
    {
        ggmlModelPath = Path.GetTempFileName();
        var model = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Tiny);
        using var fileWriter = File.OpenWrite(ggmlModelPath);
        await model.CopyToAsync(fileWriter);
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        File.Delete(ggmlModelPath);
    }

    [Test]
    public void TestHappyFlow()
    {
        var segments = new List<SegmentData>();
        var progress = new List<int>();
        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(ggmlModelPath);
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

        using var fileReader = File.OpenRead("kennedy.wav");
        processor.Process(fileReader);

        segments.Should().HaveCountGreaterThan(0);
        encoderBegins.Should().HaveCount(1);
        progress.Should().BeInAscendingOrder().And.HaveCountGreaterThan(1);

        segments.Should().Contain(segmentData => segmentData.Text.Contains("nation should commit"));
    }

    [Test]
    public async Task TestHappyFlowAsync()
    {
        var segments = new List<SegmentData>();
        var segmentsEnumerated = new List<SegmentData>();
        var progress = new List<int>();

        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(ggmlModelPath);
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

        using var fileReader = File.OpenRead("kennedy.wav");
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

    [Test]
    public void TestCancelEncoder()
    {
        var segments = new List<SegmentData>();
        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(ggmlModelPath);
        using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .WithEncoderBeginHandler((e) =>
                        {
                            encoderBegins.Add(e);
                            return false;
                        })
                        .WithSegmentEventHandler(segments.Add)
                        .Build();

        using var fileReader = File.OpenRead("kennedy.wav");
        processor.Process(fileReader);

        segments.Should().HaveCount(0);
        encoderBegins.Should().HaveCount(1);
    }

    [Test]
    public async Task TestAutoDetectLanguageWithRomanian()
    {
        var segments = new List<SegmentData>();
        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(ggmlModelPath);
        using var processor = factory.CreateBuilder()
                        .WithLanguageDetection()
                        .WithEncoderBeginHandler((e) =>
                        {
                            encoderBegins.Add(e);
                            return true;
                        })
                        .Build();
        using var fileReader = File.OpenRead("romana.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader))
        {
            segments.Add(segment);
        }
        segments.Should().HaveCountGreaterThan(0);
        encoderBegins.Should().HaveCount(1);
        segments.Should().AllSatisfy(s => s.Language.Should().Be("ro"));
        segments.Should().Contain(segmentData => segmentData.Text.Contains("efectua"));
    }

    [Test]
    public async Task ProcessAsync_Cancelled_WillCancellTheProcessing_AndDispose_WillWaitUntilFullyFinished()
    {
        var segments = new List<SegmentData>();
        var segmentsEnumerated = new List<SegmentData>();
        var cts = new CancellationTokenSource();
        TaskCanceledException? taskCanceledException = null;

        var encoderBegins = new List<EncoderBeginData>();
        using var factory = WhisperFactory.FromPath(ggmlModelPath);
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

        using var fileReader = File.OpenRead("kennedy.wav");
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

    [Test]
    public async Task ProcessAsync_WhenJunkChunkExists_ProcessCorrectly()
    {
        var segments = new List<SegmentData>();

        using var factory = WhisperFactory.FromPath(ggmlModelPath);
        await using var processor = factory.CreateBuilder()
                        .WithLanguage("en")
                        .Build();

        using var fileReader = File.OpenRead("junkchunk16khz.wav");
        await foreach (var segment in processor.ProcessAsync(fileReader))
        {
            segments.Add(segment);
        }

        segments.Should().HaveCountGreaterThanOrEqualTo(1);
    }

}
