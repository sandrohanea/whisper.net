// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Whisper.net.Tests;

public class ProcessQuantizedTests(TinyQuantizedModelFixture model) : IAssemblyFixture<TinyQuantizedModelFixture>
{
    [Fact]
    public async Task TestHappyFlowQuantized()
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
                        .WithProgressHandler(progress.Add)
                        .WithSegmentEventHandler(segments.Add)
                        .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("bush.wav");
        processor.Process(fileReader);

        Assert.True(segments.Count > 0);
        Assert.True(encoderBegins.Count >= 1);
        Assert.True(progress.Count >= 1);
        Assert.Equal(progress, progress.OrderBy(s => s));
        Assert.Contains(segments, segmentData => segmentData.Text.Contains("My fellow Americans"));
    }
}
