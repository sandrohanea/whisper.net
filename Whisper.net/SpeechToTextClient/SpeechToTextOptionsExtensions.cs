// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.Extensions.AI;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net;

public static class SpeechToTextOptionsExtensions
{
    private const string BeamSearchSamplingStrategyKey = "BeamSearchSamplingStrategy";
    private const string AudioContextSizeKey = "AudioContextSize";
    private const string DurationKey = "Duration";
    private const string EncoderBeginHandlerKey = "EncoderBeginHandler";
    private const string EntropyThresholdKey = "EntropyThreshold";
    private const string GreedySamplingStrategyKey = "GreedySamplingStrategy";
    private const string LanguageKey = "Language";
    private const string LanguageDetectionKey = "LanguageDetection";
    private const string LengthPenaltyKey = "LengthPenalty";
    private const string LogProbThresholdKey = "LogProbThreshold";
    private const string MaxInitialTsKey = "MaxInitialTs";
    private const string MaxSegmentLengthKey = "MaxSegmentLength";
    private const string MaxLastTextTokensKey = "MaxLastTextTokens";
    private const string MaxTokensPerSegmentKey = "MaxTokensPerSegment";
    private const string NoContextKey = "NoContext";
    private const string NoSpeechThresholdKey = "NoSpeechThreshold";
    private const string OffsetKey = "Offset";
    private const string OpenVinoEncoderPathKey = "OpenVinoEncoderPath";
    private const string OpenVinoDeviceKey = "OpenVinoDevice";
    private const string OpenVinoCachePathKey = "OpenVinoCachePath";
    private const string SuppressBlankKey = "SuppressBlank";
    private const string StringPoolKey = "StringPool";
    private const string PrintProgressKey = "PrintProgress";
    private const string PrintTimestampsKey = "PrintTimestamps";
    private const string PrintSpecialTokensKey = "PrintSpecialTokens";
    private const string PrintResultsKey = "PrintResults";
    private const string ProbabilitiesKey = "Probabilities";
    private const string ProgressHandlerKey = "ProgressHandler";
    private const string SegmentEventHandlerKey = "SegmentEventHandler";
    private const string TemperatureKey = "Temperature";
    private const string TemperatureIncKey = "TemperatureInc";
    private const string ThreadsKey = "Threads";
    private const string TokenTimestampsKey = "TokenTimestamps";
    private const string TokenTimestampsSumThresholdKey = "TokenTimestampsSumThreshold";
    private const string TokenTimestampsThresholdKey = "TokenTimestampsThreshold";

    /// <summary>
    /// Configures the processor with the language to be used for detection.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="language">The language (2 letters) to be used.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Default value is "en".
    /// Example: "en", "ro"
    /// </remarks>
    public static SpeechToTextOptions WithLanguage(this SpeechToTextOptions options, string language)
    {
        options.SpeechLanguage = language;
        return options;
    }

    /// <summary>
    /// Configures the processor to translate the text to English.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processor will just transcribe it.
    /// </remarks>
    public static SpeechToTextOptions WithTranslate(this SpeechToTextOptions options)
    {
        options.TextLanguage = "English";
        return options;
    }

    /// <summary>
    /// Configures the processor to use the Beam Search Sampling Strategy.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithBeamSearchSamplingStrategy(this SpeechToTextOptions options)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[BeamSearchSamplingStrategyKey] = true;

        return options;
    }

    /// <summary>
    ///  [EXPERIMENTAL] Configures the processor to override the audio context size.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="audioContextSize">Audio context size to be overridden</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Quality might be degraded while performance might be improved.
    /// </remarks>
    public static SpeechToTextOptions WithAudioContextSize(this SpeechToTextOptions options, int audioContextSize)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[AudioContextSizeKey] = audioContextSize;
        return options;
    }

    /// <summary>
    /// Configures the processor with the duration in the audio to which it processes.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="duration">Duration in the audio.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processing is happening until the end.
    /// </remarks>
    public static SpeechToTextOptions WithDuration(this SpeechToTextOptions options, TimeSpan duration)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[DurationKey] = duration;
        return options;
    }

    /// <summary>
    /// Adds a <see cref="OnEncoderBeginEventHandler"/> which will be called when the encoder begins processing.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="handler">The event handler to be added.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithEncoderBeginHandler(this SpeechToTextOptions options, OnEncoderBeginEventHandler handler)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[EncoderBeginHandlerKey] = handler;
        return options;
    }

    /// <summary>
    /// Configures the processor with an entropy threshold for decoder fallback.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="threshold">The entropy threshold.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Default value is 2.4f.
    /// </remarks>
    public static SpeechToTextOptions WithEntropyThreshold(this SpeechToTextOptions options, float threshold)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[EntropyThresholdKey] = threshold;
        return options;
    }

    /// <summary>
    /// Configures the processor to use the Greedy Sampling strategy.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithGreedySamplingStrategy(this SpeechToTextOptions options)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[GreedySamplingStrategyKey] = true;
        return options;
    }

    /// <summary>
    /// Configures the processor with the language to be used for detection.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="language">The language (2 letters) to be used.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Default value is "en".
    /// Example: "en", "ro"
    /// </remarks>
    public static SpeechToTextOptions WithWhisperLanguage(this SpeechToTextOptions options, string language)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[LanguageKey] = language;
        return options;
    }

    /// <summary>
    /// Configures the processor to auto-detect the language based on initial samples.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="enableDetection">Whether to enable language detection.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Note: Processing time will slightly increase.
    /// </remarks>
    public static SpeechToTextOptions WithLanguageDetection(this SpeechToTextOptions options, bool enableDetection = true)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[LanguageDetectionKey] = enableDetection;
        return options;
    }

    /// <summary>
    /// Configures the processor with a value indicating the length penalty (alpha).
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="penalty">The length penalty value.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processor will use simple length normalization by default.
    /// More information about the length penalty can be found here: https://arxiv.org/abs/1609.08144.
    /// </remarks>
    public static SpeechToTextOptions WithLengthPenalty(this SpeechToTextOptions options, float penalty)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[LengthPenaltyKey] = penalty;
        return options;
    }

    /// <summary>
    /// Configures the processor with a average log probability threshold over sampled tokens.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="threshold">The average log probability threshold.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Default value is -1.0f.
    /// </remarks>
    public static SpeechToTextOptions WithLogProbThreshold(this SpeechToTextOptions options, float threshold)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[LogProbThresholdKey] = threshold;
        return options;
    }

    /// <summary>
    /// Configures the processor with a value indicating that the initial timestamp cannot be later than this.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="maxInitialTs">The initial max timestamp.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, default value is: 1f.
    /// </remarks>
    public static SpeechToTextOptions WithMaxInitialTs(this SpeechToTextOptions options, float maxInitialTs)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[MaxInitialTsKey] = maxInitialTs;
        return options;
    }

    /// <summary>
    /// Configures the processor with the maximum segment length in characters.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="maxLength">The maximum segment length in characters.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithMaxSegmentLength(this SpeechToTextOptions options, int maxLength)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[MaxSegmentLengthKey] = maxLength;
        return options;
    }

    /// <summary>
    /// Configures the processor with the max number of tokens to be used from the previous text as prompt for the decoder.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="maxTokens">The max number of tokens to be used.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, a number of 16384 tokens is used.
    /// </remarks>
    public static SpeechToTextOptions WithMaxLastTextTokens(this SpeechToTextOptions options, int maxTokens)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[MaxLastTextTokensKey] = maxTokens;
        return options;
    }

    /// <summary>
    /// Configures the processor with the maximum number of tokens per segment.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="maxTokens">The maximum number of tokens per segment.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithMaxTokensPerSegment(this SpeechToTextOptions options, int maxTokens)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[MaxTokensPerSegmentKey] = maxTokens;
        return options;
    }

    /// <summary>
    /// Configures the processor to not use past transformation (if any) as the initial prompt for a newer processing.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="noContext">Whether to disable context.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processor use part transformations as initial prompt for newer processing.
    /// </remarks>
    public static SpeechToTextOptions WithNoContext(this SpeechToTextOptions options, bool noContext = true)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[NoContextKey] = noContext;
        return options;
    }

    /// <summary>
    /// [EXPERIMENTAL] Configures the processor with a no_speech probability.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="threshold">The no_speech probability</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Default value is 0.6f.
    /// </remarks>
    public static SpeechToTextOptions WithNoSpeechThreshold(this SpeechToTextOptions options, float threshold)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[NoSpeechThresholdKey] = threshold;
        return options;
    }

    /// <summary>
    /// Configures the processor with the start time in the audio from which it starts the processing.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="offset">Offset in the audio.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processing is happening from the beginning.
    /// </remarks>
    public static SpeechToTextOptions WithOffset(this SpeechToTextOptions options, TimeSpan offset)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[OffsetKey] = offset;
        return options;
    }

    /// <summary>
    /// Configures the processor to use OpenVINO for the encoder part.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="encoderPath">Path to the OpenVINO encoder model.</param>
    /// <param name="device">The device to use for OpenVINO.</param>
    /// <param name="cachePath">Path to the OpenVINO cache directory.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithOpenVinoEncoder(this SpeechToTextOptions options, string? encoderPath, string? device = null, string? cachePath = null)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[OpenVinoEncoderPathKey] = encoderPath;
        if (device != null)
        {
            options.AdditionalProperties[OpenVinoDeviceKey] = device;
        }
        if (cachePath != null)
        {
            options.AdditionalProperties[OpenVinoCachePathKey] = cachePath;
        }
        return options;
    }

    /// <summary>
    /// Configures the processor to NOT suppress blank outputs.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, blanks are automatically suppressed.
    /// </remarks>
    public static SpeechToTextOptions WithoutSuppressBlank(this SpeechToTextOptions options)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[SuppressBlankKey] = false;
        return options;
    }

    /// <summary>
    /// Disables the string pooling.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// This will disable the pooling of strings that are generated (have effect only if <see cref="WithStringPool"/> was called).
    /// </remarks>
    public static SpeechToTextOptions WithoutStringPool(this SpeechToTextOptions options)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[StringPoolKey] = false;
        return options;
    }

    /// <summary>
    /// Configures the processor to print progress information.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="printProgress">Whether to print progress information.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processor will not print progress information.
    /// </remarks>
    public static SpeechToTextOptions WithPrintProgress(this SpeechToTextOptions options, bool printProgress = true)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[PrintProgressKey] = printProgress;
        return options;
    }

    /// <summary>
    /// Configures the processor to print timestamps for each segment to stdout.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="printTimestamps">Whether to print timestamps.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// This option is available only if <see cref="WithPrintResults"/> is configured.
    /// If not specified, the processor will print also timestamps.
    /// </remarks>
    public static SpeechToTextOptions WithPrintTimestamps(this SpeechToTextOptions options, bool printTimestamps = true)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[PrintTimestampsKey] = printTimestamps;
        return options;
    }

    /// <summary>
    /// Configures the processor to print special tokens to stdout.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="printSpecialTokens">Whether to print special tokens.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// This option is available only if <see cref="WithPrintResults"/> is configured.
    /// </remarks>
    public static SpeechToTextOptions WithPrintSpecialTokens(this SpeechToTextOptions options, bool printSpecialTokens = true)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[PrintSpecialTokensKey] = printSpecialTokens;
        return options;
    }

    /// <summary>
    /// Configures the processor to print results to stdout.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="printResults">Whether to print results.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processor will not print results to stdout.
    /// </remarks>
    public static SpeechToTextOptions WithPrintResults(this SpeechToTextOptions options, bool printResults = true)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[PrintResultsKey] = printResults;
        return options;
    }

    /// <summary>
    /// Configures the processor to return probabilities during segment decoding <see cref="SegmentData.MaxProbability"/>, <see cref="SegmentData.MinProbability"/> and <see cref="SegmentData.Probability"/>.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="enableProbabilities">Whether to enable probabilities.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithProbabilities(this SpeechToTextOptions options, bool enableProbabilities = true)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[ProbabilitiesKey] = enableProbabilities;
        return options;
    }

    /// <summary>
    /// Adds a <see cref="OnProgressHandler"/> which will be called to report progress.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="handler">The event handler to be added.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithProgressHandler(this SpeechToTextOptions options, OnProgressHandler handler)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[ProgressHandlerKey] = handler;
        return options;
    }

    /// <summary>
    /// Adds a <see cref="OnSegmentEventHandler"/> which will be called every time a new segment is detected.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="handler">The event handler to be added.</param>
    /// <returns>The same options instance for chaining.</returns>
    public static SpeechToTextOptions WithSegmentEventHandler(this SpeechToTextOptions options, OnSegmentEventHandler handler)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[SegmentEventHandlerKey] = handler;
        return options;
    }

    /// <summary>
    /// Adds the functionality of pooling the strings that are generated reducing the number of allocations.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="stringPool">The string pool to use.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// When using this option designed for high-performance use-cases,
    /// ensure that you're returning the <see cref="SegmentData"/> object back to the <see cref="WhisperProcessor"/>
    /// using the method <see cref="WhisperProcessor.Return(SegmentData)"/>.
    /// </remarks>
    public static SpeechToTextOptions WithStringPool(this SpeechToTextOptions options, IStringPool stringPool)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[StringPoolKey] = stringPool;
        return options;
    }

    /// <summary>
    /// Configures the processor with a temperature for sampling.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="temperature">The temperature value.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Higher values like 0.8 will make the output more random, while lower values like 0.2 will make it more focused and deterministic.
    /// Default value is 0.0f.
    /// </remarks>
    public static SpeechToTextOptions WithTemperature(this SpeechToTextOptions options, float temperature)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[TemperatureKey] = temperature;
        return options;
    }

    /// <summary>
    /// Configures the processor with a temperature to increase when falling back.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="temperatureInc">The temperature to increase when falling back.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Falling back can happen when the decoding fails to meet either of the thresholds in: <see cref="WithEntropyThreshold"/>, <see cref="WithLogProbThreshold"/> or <see cref="WithNoSpeechThreshold"/>.
    /// Default value is 0.2f.
    /// </remarks>
    public static SpeechToTextOptions WithTemperatureInc(this SpeechToTextOptions options, float temperatureInc)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[TemperatureIncKey] = temperatureInc;
        return options;
    }

    /// <summary>
    /// Configures the processor to use the specified number of threads.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="threads">The number of threads to be used during encoding and decoding.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the same number as the hardware threads that the underlying hardware can support concurrently is used.
    /// </remarks>
    public static SpeechToTextOptions WithThreads(this SpeechToTextOptions options, int threads)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[ThreadsKey] = threads;
        return options;
    }

    /// <summary>
    /// [EXPERIMENTAL] Configures the processor to use token-level timestamps.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// If not specified, the processor will not use token timestamps.
    /// </remarks>
    public static SpeechToTextOptions WithTokenTimestamps(this SpeechToTextOptions options)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[TokenTimestampsKey] = true;
        return options;
    }

    /// <summary>
    /// Configures the processor to use the specified SUM probability threshold for token timestamps.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="threshold">Probability SUM threshold to be used for token-level timestamps.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Default value is 0.01f.
    /// This option have effect only together with <see cref="WithTokenTimestamps"/>
    /// </remarks>
    public static SpeechToTextOptions WithTokenTimestampsSumThreshold(this SpeechToTextOptions options, float threshold)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[TokenTimestampsSumThresholdKey] = threshold;
        return options;
    }

    /// <summary>
    /// Configures the processor to use the specified probability threshold for token timestamps.
    /// </summary>
    /// <param name="options">The options to configure.</param>
    /// <param name="threshold">Probability threshold to be used for token-level timestamps.</param>
    /// <returns>The same options instance for chaining.</returns>
    /// <remarks>
    /// Default value is 0.01f.
    /// This option have effect only together with <see cref="WithTokenTimestamps"/>
    /// </remarks>
    public static SpeechToTextOptions WithTokenTimestampsThreshold(this SpeechToTextOptions options, float threshold)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[TokenTimestampsThresholdKey] = threshold;
        return options;
    }

    internal static WhisperProcessor BuildWhisperProcessor(this SpeechToTextOptions? options, WhisperFactory factory)
    {
        var processorBuilder = factory.CreateBuilder();

        if (options is null)
        {
            return processorBuilder.Build();
        }

        if (!string.IsNullOrWhiteSpace(options?.SpeechLanguage))
        {
            processorBuilder.WithLanguage(options!.SpeechLanguage!);
        }

        if (GetAdditionalProperty<int>(AudioContextSizeKey, options!, out var audioContextSize))
        {
            processorBuilder.WithAudioContextSize(audioContextSize);
        }

        if (GetAdditionalProperty<bool>(BeamSearchSamplingStrategyKey, options!, out var beamSearchSamplingStrategy) && beamSearchSamplingStrategy)
        {
            processorBuilder.WithBeamSearchSamplingStrategy();
        }

        if (GetAdditionalProperty<TimeSpan>(DurationKey, options!, out var duration))
        {
            processorBuilder.WithDuration(duration);
        }

        if (GetAdditionalProperty<OnEncoderBeginEventHandler>(EncoderBeginHandlerKey, options!, out var encoderBeginHandler) && encoderBeginHandler != null)
        {
            processorBuilder.WithEncoderBeginHandler(encoderBeginHandler);
        }

        if (GetAdditionalProperty<float>(EntropyThresholdKey, options!, out var entropyThreshold))
        {
            processorBuilder.WithEntropyThreshold(entropyThreshold);
        }

        if (GetAdditionalProperty<bool>(GreedySamplingStrategyKey, options!, out var greedySamplingStrategy) && greedySamplingStrategy)
        {
            processorBuilder.WithGreedySamplingStrategy();
        }

        if (GetAdditionalProperty<string>(LanguageKey, options!, out var language) && !string.IsNullOrEmpty(language))
        {
            processorBuilder.WithLanguage(language!);
        }

        if (GetAdditionalProperty<bool>(LanguageDetectionKey, options!, out var languageDetection) && languageDetection)
        {
            processorBuilder.WithLanguageDetection();
        }

        if (GetAdditionalProperty<float>(LengthPenaltyKey, options!, out var lengthPenalty))
        {
            processorBuilder.WithLengthPenalty(lengthPenalty);
        }

        if (GetAdditionalProperty<float>(LogProbThresholdKey, options!, out var logProbThreshold))
        {
            processorBuilder.WithLogProbThreshold(logProbThreshold);
        }

        if (GetAdditionalProperty<float>(MaxInitialTsKey, options!, out var maxInitialTs))
        {
            processorBuilder.WithMaxInitialTs(maxInitialTs);
        }

        if (GetAdditionalProperty<int>(MaxSegmentLengthKey, options!, out var maxSegmentLength))
        {
            processorBuilder.WithMaxSegmentLength(maxSegmentLength);
        }

        if (GetAdditionalProperty<int>(MaxLastTextTokensKey, options!, out var maxLastTextTokens))
        {
            processorBuilder.WithMaxLastTextTokens(maxLastTextTokens);
        }

        if (GetAdditionalProperty<int>(MaxTokensPerSegmentKey, options!, out var maxTokensPerSegment))
        {
            processorBuilder.WithMaxTokensPerSegment(maxTokensPerSegment);
        }

        if (GetAdditionalProperty<bool>(NoContextKey, options!, out var noContext) && noContext)
        {
            processorBuilder.WithNoContext();
        }

        if (GetAdditionalProperty<float>(NoSpeechThresholdKey, options!, out var noSpeechThreshold))
        {
            processorBuilder.WithNoSpeechThreshold(noSpeechThreshold);
        }

        if (GetAdditionalProperty<TimeSpan>(OffsetKey, options!, out var offset))
        {
            processorBuilder.WithOffset(offset);
        }

        if (GetAdditionalProperty<string>(OpenVinoEncoderPathKey, options!, out var openVinoEncoderPath))
        {
            GetAdditionalProperty<string>(OpenVinoDeviceKey, options!, out var openVinoDevice);
            GetAdditionalProperty<string>(OpenVinoCachePathKey, options!, out var openVinoCachePath);
            processorBuilder.WithOpenVinoEncoder(openVinoEncoderPath, openVinoDevice, openVinoCachePath);
        }

        if (GetAdditionalProperty<bool>(SuppressBlankKey, options!, out var suppressBlank) && !suppressBlank)
        {
            processorBuilder.WithoutSuppressBlank();
        }

        if (GetAdditionalProperty<bool>(StringPoolKey, options!, out var useStringPool) && !useStringPool)
        {
            processorBuilder.WithoutStringPool();
        }

        if (GetAdditionalProperty<bool>(PrintProgressKey, options!, out var printProgress) && printProgress)
        {
            processorBuilder.WithPrintProgress();
        }

        if (GetAdditionalProperty<bool>(PrintTimestampsKey, options!, out var printTimestamps) && printTimestamps)
        {
            processorBuilder.WithPrintTimestamps();
        }

        if (GetAdditionalProperty<bool>(PrintSpecialTokensKey, options!, out var printSpecialTokens) && printSpecialTokens)
        {
            processorBuilder.WithPrintSpecialTokens();
        }

        if (GetAdditionalProperty<bool>(PrintResultsKey, options!, out var printResults) && printResults)
        {
            processorBuilder.WithPrintResults();
        }

        if (GetAdditionalProperty<bool>(ProbabilitiesKey, options!, out var probabilities) && probabilities)
        {
            processorBuilder.WithProbabilities();
        }

        if (GetAdditionalProperty<OnProgressHandler>(ProgressHandlerKey, options!, out var progressHandler) && progressHandler != null)
        {
            processorBuilder.WithProgressHandler(progressHandler);
        }

        if (GetAdditionalProperty<OnSegmentEventHandler>(SegmentEventHandlerKey, options!, out var segmentEventHandler) && segmentEventHandler != null)
        {
            processorBuilder.WithSegmentEventHandler(segmentEventHandler);
        }

        if (GetAdditionalProperty<IStringPool>(StringPoolKey, options!, out var stringPool) && stringPool != null)
        {
            processorBuilder.WithStringPool(stringPool);
        }

        if (GetAdditionalProperty<float>(TemperatureKey, options!, out var temperature))
        {
            processorBuilder.WithTemperature(temperature);
        }

        if (GetAdditionalProperty<float>(TemperatureIncKey, options!, out var temperatureInc))
        {
            processorBuilder.WithTemperatureInc(temperatureInc);
        }

        if (GetAdditionalProperty<int>(ThreadsKey, options!, out var threads))
        {
            processorBuilder.WithThreads(threads);
        }

        if (GetAdditionalProperty<bool>(TokenTimestampsKey, options!, out var tokenTimestamps) && tokenTimestamps)
        {
            processorBuilder.WithTokenTimestamps();
        }

        if (GetAdditionalProperty<float>(TokenTimestampsSumThresholdKey, options!, out var tokenTimestampsSumThreshold))
        {
            processorBuilder.WithTokenTimestampsSumThreshold(tokenTimestampsSumThreshold);
        }

        if (GetAdditionalProperty<float>(TokenTimestampsThresholdKey, options!, out var tokenTimestampsThreshold))
        {
            processorBuilder.WithTokenTimestampsThreshold(tokenTimestampsThreshold);
        }

        if (!string.IsNullOrWhiteSpace(options?.TextLanguage))
        {
            processorBuilder.WithTranslate();
        }

        return processorBuilder.Build();
    }

    private static bool GetAdditionalProperty<T>(string propertyName, SpeechToTextOptions options, out T? value)
    {
        if (options.AdditionalProperties?.TryGetValue(propertyName, out value) ?? false)
        {
            return true;
        }

        value = default;
        return false;
    }
}
