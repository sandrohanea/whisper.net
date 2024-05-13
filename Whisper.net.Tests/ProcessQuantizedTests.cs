// Licensed under the MIT license: https://opensource.org/licenses/MIT

using FluentAssertions;
using NUnit.Framework;

namespace Whisper.net.Tests;

public class ProcessQuantizedTests {
  [Test]
  public void TestHappyFlowQuantized() {
    var segments = new List<SegmentData>();
    var progress = new List<int>();
    var encoderBegins = new List<EncoderBeginData>();
    using var factory =
        WhisperFactory.FromPath(TestModelProvider.GgmlModelTinyQ5);
    using var processor = factory.CreateBuilder()
                              .WithLanguage("en")
                              .WithEncoderBeginHandler((e) => {
                                encoderBegins.Add(e);
                                return true;
                              })
                              .WithProgressHandler(progress.Add)
                              .WithSegmentEventHandler(segments.Add)
                              .Build();

    using var fileReader = File.OpenRead("bush.wav");
    processor.Process(fileReader);

    segments.Should().HaveCountGreaterThan(0);
    encoderBegins.Should().HaveCountGreaterThanOrEqualTo(1);
    progress.Should().BeInAscendingOrder().And.HaveCountGreaterThan(1);

    segments.Should().Contain(
        segmentData => segmentData.Text.Contains("My fellow Americans"));
  }
}
