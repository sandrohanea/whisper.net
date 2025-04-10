// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.Extensions.AI;
using Xunit;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net.Tests.SpeechToText;

public class SpeechToTextOptionsExtensionsTests
{
    [Fact]
    public void WithLanguage_SetsLanguageProperty()
    {
        // Arrange

        var options = new SpeechToTextOptions();

        var language = "en";

        // Act
        var result = options.WithLanguage(language);

        // Assert
        Assert.Equal(language, options.SpeechLanguage);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithTranslate_SetsTextLanguageToEnglish()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithTranslate();

        // Assert
        Assert.Equal("English", options.TextLanguage);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithBeamSearchSamplingStrategy_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithBeamSearchSamplingStrategy();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("BeamSearchSamplingStrategy", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithAudioContextSize_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var audioContextSize = 42;

        // Act
        var result = options.WithAudioContextSize(audioContextSize);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("AudioContextSize", out var value));
        Assert.Equal(audioContextSize, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithDuration_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var duration = TimeSpan.FromSeconds(10);

        // Act
        var result = options.WithDuration(duration);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("Duration", out var value));
        Assert.Equal(duration, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithEncoderBeginHandler_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        OnEncoderBeginEventHandler handler = (e) => true;

        // Act
        var result = options.WithEncoderBeginHandler(handler);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("EncoderBeginHandler", out var value));
        Assert.Same(handler, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithEntropyThreshold_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var threshold = 2.4f;

        // Act
        var result = options.WithEntropyThreshold(threshold);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("EntropyThreshold", out var value));
        Assert.Equal(threshold, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithGreedySamplingStrategy_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithGreedySamplingStrategy();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("GreedySamplingStrategy", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithWhisperLanguage_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var language = "en";

        // Act
        var result = options.WithWhisperLanguage(language);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("Language", out var value));
        Assert.Equal(language, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithLanguageDetection_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithLanguageDetection();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("LanguageDetection", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithLanguageDetection_WithFalseParameter_SetsAdditionalPropertyToFalse()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithLanguageDetection(false);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("LanguageDetection", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithLengthPenalty_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var penalty = 0.5f;

        // Act
        var result = options.WithLengthPenalty(penalty);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("LengthPenalty", out var value));
        Assert.Equal(penalty, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithLogProbThreshold_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var threshold = -1.0f;

        // Act
        var result = options.WithLogProbThreshold(threshold);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("LogProbThreshold", out var value));
        Assert.Equal(threshold, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithMaxInitialTs_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var maxInitialTs = 1.0f;

        // Act
        var result = options.WithMaxInitialTs(maxInitialTs);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("MaxInitialTs", out var value));
        Assert.Equal(maxInitialTs, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithMaxSegmentLength_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var maxLength = 100;

        // Act
        var result = options.WithMaxSegmentLength(maxLength);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("MaxSegmentLength", out var value));
        Assert.Equal(maxLength, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithMaxLastTextTokens_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var maxTokens = 16384;

        // Act
        var result = options.WithMaxLastTextTokens(maxTokens);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("MaxLastTextTokens", out var value));
        Assert.Equal(maxTokens, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithMaxTokensPerSegment_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var maxTokens = 50;

        // Act
        var result = options.WithMaxTokensPerSegment(maxTokens);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("MaxTokensPerSegment", out var value));
        Assert.Equal(maxTokens, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithNoContext_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithNoContext();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("NoContext", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithNoContext_WithFalseParameter_SetsAdditionalPropertyToFalse()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithNoContext(false);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("NoContext", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithNoSpeechThreshold_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var threshold = 0.6f;

        // Act
        var result = options.WithNoSpeechThreshold(threshold);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("NoSpeechThreshold", out var value));
        Assert.Equal(threshold, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithOffset_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var offset = TimeSpan.FromSeconds(5);

        // Act
        var result = options.WithOffset(offset);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("Offset", out var value));
        Assert.Equal(offset, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithOpenVinoEncoder_SetsAdditionalProperties()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var encoderPath = "path/to/encoder";
        var device = "CPU";
        var cachePath = "path/to/cache";

        // Act
        var result = options.WithOpenVinoEncoder(encoderPath, device, cachePath);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("OpenVinoEncoderPath", out var pathValue));
        Assert.Equal(encoderPath, pathValue);
        Assert.True(options.AdditionalProperties.TryGetValue("OpenVinoDevice", out var deviceValue));
        Assert.Equal(device, deviceValue);
        Assert.True(options.AdditionalProperties.TryGetValue("OpenVinoCachePath", out var cacheValue));
        Assert.Equal(cachePath, cacheValue);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithOpenVinoEncoder_WithNullDeviceAndCache_SetsOnlyEncoderPath()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var encoderPath = "path/to/encoder";

        // Act
        var result = options.WithOpenVinoEncoder(encoderPath);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("OpenVinoEncoderPath", out var pathValue));
        Assert.Equal(encoderPath, pathValue);
        Assert.False(options.AdditionalProperties.ContainsKey("OpenVinoDevice"));
        Assert.False(options.AdditionalProperties.ContainsKey("OpenVinoCachePath"));
        Assert.Same(options, result);
    }

    [Fact]
    public void WithoutSuppressBlank_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithoutSuppressBlank();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("SuppressBlank", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithoutStringPool_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithoutStringPool();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("StringPool", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintProgress_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintProgress();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintProgress", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintProgress_WithFalseParameter_SetsAdditionalPropertyToFalse()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintProgress(false);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintProgress", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintTimestamps_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintTimestamps();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintTimestamps", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintTimestamps_WithFalseParameter_SetsAdditionalPropertyToFalse()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintTimestamps(false);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintTimestamps", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintSpecialTokens_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintSpecialTokens();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintSpecialTokens", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintSpecialTokens_WithFalseParameter_SetsAdditionalPropertyToFalse()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintSpecialTokens(false);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintSpecialTokens", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintResults_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintResults();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintResults", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithPrintResults_WithFalseParameter_SetsAdditionalPropertyToFalse()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithPrintResults(false);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("PrintResults", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithProbabilities_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithProbabilities();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("Probabilities", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithProbabilities_WithFalseParameter_SetsAdditionalPropertyToFalse()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithProbabilities(false);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("Probabilities", out var value));
        Assert.False((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithProgressHandler_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        OnProgressHandler handler = (progress) => { };

        // Act
        var result = options.WithProgressHandler(handler);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("ProgressHandler", out var value));
        Assert.Same(handler, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithSegmentEventHandler_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        OnSegmentEventHandler handler = (e) => { };

        // Act
        var result = options.WithSegmentEventHandler(handler);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("SegmentEventHandler", out var value));
        Assert.Same(handler, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithStringPool_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var stringPool = new TestStringPool();

        // Act
        var result = options.WithStringPool(stringPool);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("StringPool", out var value));
        Assert.Same(stringPool, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithTemperature_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var temperature = 0.8f;

        // Act
        var result = options.WithTemperature(temperature);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("Temperature", out var value));
        Assert.Equal(temperature, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithTemperatureInc_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var temperatureInc = 0.2f;

        // Act
        var result = options.WithTemperatureInc(temperatureInc);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("TemperatureInc", out var value));
        Assert.Equal(temperatureInc, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithThreads_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var threads = 4;

        // Act
        var result = options.WithThreads(threads);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("Threads", out var value));
        Assert.Equal(threads, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithTokenTimestamps_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();

        // Act
        var result = options.WithTokenTimestamps();

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("TokenTimestamps", out var value));
        Assert.True((bool)value!);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithTokenTimestampsSumThreshold_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var threshold = 0.01f;

        // Act
        var result = options.WithTokenTimestampsSumThreshold(threshold);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("TokenTimestampsSumThreshold", out var value));
        Assert.Equal(threshold, value);
        Assert.Same(options, result);
    }

    [Fact]
    public void WithTokenTimestampsThreshold_SetsAdditionalProperty()
    {
        // Arrange
        var options = new SpeechToTextOptions();
        var threshold = 0.01f;

        // Act
        var result = options.WithTokenTimestampsThreshold(threshold);

        // Assert
        Assert.NotNull(options.AdditionalProperties);
        Assert.True(options.AdditionalProperties.TryGetValue("TokenTimestampsThreshold", out var value));
        Assert.Equal(threshold, value);
        Assert.Same(options, result);
    }

    // Helper class for testing
    private class TestStringPool : IStringPool
    {
        public string? GetStringUtf8(IntPtr nativeUtf8)
        {
            return null;
        }

        public void ReturnString(string? returnedString)
        {
            throw new NotImplementedException();
        }
    }
}
