// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;
using Whisper.net.SamplingStrategy;

namespace Whisper.net;

/// <summary>
/// Builder for <see cref="WhisperProcessor"/>.
/// </summary>
public class WhisperProcessorBuilder
{
    private readonly WhisperProcessorOptions whisperProcessorOptions;
    private readonly INativeWhisper nativeWhisper;
    private readonly IStringPool stringPool;

    internal WhisperProcessorBuilder(IntPtr context, INativeWhisper nativeWhisper, IStringPool stringPool)
    {
        whisperProcessorOptions = new WhisperProcessorOptions() { ContextHandle = context };
        this.nativeWhisper = nativeWhisper;
        this.stringPool = stringPool;
    }

    /// <summary>
    /// Configures the processor to use the specified number of threads.
    /// </summary>
    /// <param name="threads">The number of threads to be used during encoding and decoding.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the same number as the hardware threads that the underlying hardware can support concurrently is used.
    /// </remarks>
    public WhisperProcessorBuilder WithThreads(int threads)
    {
        whisperProcessorOptions.Threads = threads;
        return this;
    }

    /// <summary>
    /// Configures the processor with the max number of tokens to be used from the previous text as prompt for the decoder.
    /// </summary>
    /// <param name="maxLastTextTokens">The max number of tokens to be used.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, a number of 16384 tokens is used.
    /// </remarks>
    public WhisperProcessorBuilder WithMaxLastTextTokens(int maxLastTextTokens)
    {
        whisperProcessorOptions.MaxLastTextTokens = maxLastTextTokens;
        return this;
    }

    /// <summary>
    /// Configures the processor with the start time in the audio from which it starts the processing.
    /// </summary>
    /// <param name="offset">Offset in the audio.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processing is happening from the beginning.
    /// </remarks>
    public WhisperProcessorBuilder WithOffset(TimeSpan offset)
    {
        whisperProcessorOptions.Offset = offset;
        return this;
    }

    /// <summary>
    /// Configures the processor with the duration of the audio to be processed.
    /// </summary>
    /// <param name="duration">Duration to be processed</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processing is happening for the entire input.
    /// </remarks>
    public WhisperProcessorBuilder WithDuration(TimeSpan duration)
    {
        whisperProcessorOptions.Duration = duration;
        return this;
    }

    /// <summary>
    /// Configures the processor to translate the text to English.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor will just transcribe it.
    /// </remarks>
    public WhisperProcessorBuilder WithTranslate()
    {
        whisperProcessorOptions.Translate = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to not use past transformation (if any) as the initial prompt for a newer processing.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor use part transformations as initial prompt for newer processing.
    /// </remarks>
    public WhisperProcessorBuilder WithNoContext()
    {
        whisperProcessorOptions.NoContext = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to force a single segment as output instead of multiple.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor will return multiple segments (if they are recognized).
    /// </remarks>
    public WhisperProcessorBuilder WithSingleSegment()
    {
        whisperProcessorOptions.SingleSegment = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to print special tokens (e.g. &lt;SOT&gt;, &lt;EOT&gt;, &lt;BEG&gt;, etc.)
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor will not print special tokens.
    /// </remarks>
    public WhisperProcessorBuilder WithPrintSpecialTokens()
    {
        whisperProcessorOptions.PrintSpecialTokens = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to print progress information.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor will not print progress information.
    /// </remarks>
    public WhisperProcessorBuilder WithPrintProgress()
    {
        whisperProcessorOptions.PrintProgress = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to print results to stdout.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor will not print results to stdout.
    /// </remarks>
    public WhisperProcessorBuilder WithPrintResults()
    {
        whisperProcessorOptions.PrintResults = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to print timestamps for each segment to stdout.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// This option is available only if <seealso cref="WithPrintResults"/> is configured.
    /// If not specified, the processor will print also timestamps.
    /// </remarks>
    public WhisperProcessorBuilder WithPrintTimestamps(bool printTimestamps = true)
    {
        whisperProcessorOptions.PrintTimestamps = printTimestamps;
        return this;
    }

    /// <summary>
    /// [EXPERIMENTAL] Configures the processor to use token-level timestamps.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor will not use token timestamps.
    /// </remarks>
    public WhisperProcessorBuilder WithTokenTimestamps()
    {
        whisperProcessorOptions.UseTokenTimestamps = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to use the specified probability threshold for token timestamps.
    /// </summary>
    /// <param name="tokenTimestampsThreshold">Probability threshold to be used for token-level timestamps.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Default value is 0.01f.
    /// This option have effect only together with <seealso cref="WithTokenTimestamps"/>
    /// </remarks>
    public WhisperProcessorBuilder WithTokenTimestampsThreshold(float tokenTimestampsThreshold)
    {
        whisperProcessorOptions.TokenTimestampsThreshold = tokenTimestampsThreshold;
        return this;
    }

    /// <summary>
    /// Configures the processor to use the specified SUM probability threshold for token timestamps.
    /// </summary>
    /// <param name="tokenTimestampsSumThreshold">Probability SUM threshold to be used for token-level timestamps.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Default value is 0.01f.
    /// This option have effect only together with <seealso cref="WithTokenTimestamps"/>
    /// </remarks>
    public WhisperProcessorBuilder WithTokenTimestampsSumThreshold(float tokenTimestampsSumThreshold)
    {
        whisperProcessorOptions.TokenTimestampsSumThreshold = tokenTimestampsSumThreshold;
        return this;
    }

    /// <summary>
    /// Configures the processor to use a maximum segment length.
    /// </summary>
    /// <param name="maxSegmentLength">The maximum segment length to be used</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified no max segment length will be applied.
    /// </remarks>
    public WhisperProcessorBuilder WithMaxSegmentLength(int maxSegmentLength)
    {
        whisperProcessorOptions.MaxSegmentLength = maxSegmentLength;
        return this;
    }

    /// <summary>
    /// Configures the processor to split on each word.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified split will be done based on the model configuration.
    /// </remarks>
    public WhisperProcessorBuilder SplitOnWord()
    {
        whisperProcessorOptions.SplitOnWord = true;
        return this;
    }

    /// <summary>
    /// Configures the processor to use a maximum tokens per segment.
    /// </summary>
    /// <param name="maxTokensPerSegment">The maximum number of tokens to be used</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified no max number of tokens will be applied.
    /// </remarks>
    public WhisperProcessorBuilder WithMaxTokensPerSegment(int maxTokensPerSegment)
    {
        whisperProcessorOptions.MaxTokensPerSegment = maxTokensPerSegment;
        return this;
    }

    /// <summary>
    ///  [EXPERIMENTAL] Configures the processor to override the audio context size.
    /// </summary>
    /// <param name="audioContextSize">Audio context size to be overridden</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Quality might be degraded while performance might be improved.
    /// </remarks>
    public WhisperProcessorBuilder WithAudioContextSize(int audioContextSize)
    {
        whisperProcessorOptions.AudioContextSize = audioContextSize;
        return this;
    }

    /// <summary>
    /// [EXPERIMENTAL] Configures the processor to suppress specific tokens that are matched by the regex.
    /// </summary>
    /// <param name="regex">The regex that should be used for filtering.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// See https://github.com/openai/whisper/discussions/1041 for more details.
    /// </remarks>
    public WhisperProcessorBuilder WithSuppressRegex(string regex)
    {
        whisperProcessorOptions.SuppressRegex = regex;
        return this;
    }

    /// <summary>
    /// [EXPERIMENTAL] Configures the processor to use an initial prompt, which will be prepended to any existing text context.
    /// </summary>
    /// <param name="prompt">The prompt to be used.</param>
    /// <returns>An instance to the same builder.</returns>
    public WhisperProcessorBuilder WithPrompt(string prompt)
    {
        whisperProcessorOptions.Prompt = prompt;
        return this;
    }

    /// <summary>
    /// [EXPERIMENTAL] Configures the processor to always prepend InitialPrompt to every decode window (may reduce conditioning on previous text)
    /// </summary>
    /// <remarks>
    /// By default, it is false.
    /// </remarks>
    /// <param name="carryInitialPrompt">A value indicating how to configure this value.</param>
    public WhisperProcessorBuilder WithCarryInitialPrompt(bool carryInitialPrompt = true)
    {
        whisperProcessorOptions.CarryInitialPrompt = carryInitialPrompt;
        return this;
    }

    /// <summary>
    /// Configures the processor with the language to be used for detection.
    /// </summary>
    /// <param name="language">The language (2 letters) to be used.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Default value is "en".
    /// Example: "en", "ro"
    /// </remarks>
    public WhisperProcessorBuilder WithLanguage(string language)
    {
        whisperProcessorOptions.Language = language;
        return this;
    }

    /// <summary>
    /// Configures the processor to auto-detect the language based on initial samples.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Note: Processing time will slightly increase.
    /// </remarks>
    public WhisperProcessorBuilder WithLanguageDetection()
    {
        whisperProcessorOptions.Language = string.Empty;
        return this;
    }

    /// <summary>
    /// Configures the processor to NOT suppress blank outputs.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, blanks are automatically suppressed.
    /// </remarks>
    public WhisperProcessorBuilder WithoutSuppressBlank()
    {
        whisperProcessorOptions.SuppressBlank = false;
        return this;
    }

    /// <summary>
    /// Configures the temperature for the processor.
    /// </summary>
    /// <param name="temperature">The temperature value to be used</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, default value is: 0.2f.
    /// More details: https://ai.stackexchange.com/questions/32477/what-is-the-temperature-in-the-gpt-models/32478#32478
    /// </remarks>
    public WhisperProcessorBuilder WithTemperature(float temperature)
    {
        whisperProcessorOptions.Temperature = temperature;
        return this;
    }

    /// <summary>
    /// Configures the processor with a value indicating that the initial timestamp cannot be later than this.
    /// </summary>
    /// <param name="maxInitialTs">The initial max timestamp.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, default value is: 1f.
    /// </remarks>
    public WhisperProcessorBuilder WithMaxInitialTs(float maxInitialTs)
    {
        whisperProcessorOptions.MaxInitialTs = maxInitialTs;
        return this;
    }

    /// <summary>
    /// Configures the processor with a value indicating the length penalty (alpha).
    /// </summary>
    /// <param name="lengthPenalty">The initial max timestamp.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not specified, the processor will use simple length normalization by default.
    /// More information about the length penalty can be found here: https://arxiv.org/abs/1609.08144.
    /// </remarks>
    public WhisperProcessorBuilder WithLengthPenalty(float lengthPenalty)
    {
        whisperProcessorOptions.LengthPenalty = lengthPenalty;
        return this;
    }

    /// <summary>
    /// Configures the processor with a temperature to increase when falling back.
    /// </summary>
    /// <param name="temperature">The temperature to increase when falling back.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Falling back can happen when the decoding fails to meet either of the thresholds in: <seealso cref="WithEntropyThreshold(float)"/>, <seealso cref="WithLogProbThreshold(float)"/> or <seealso cref="WithNoSpeechThreshold(float)"/>.
    /// Default value is 0.2f.
    /// </remarks>
    public WhisperProcessorBuilder WithTemperatureInc(float temperature)
    {
        whisperProcessorOptions.TemperatureInc = temperature;
        return this;
    }

    /// <summary>
    /// Configures the processor with the entropy threshold for falling back.
    /// </summary>
    /// <param name="entropyThreshold">The entropy threshold</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Default value is 2.4f.
    /// </remarks>
    public WhisperProcessorBuilder WithEntropyThreshold(float entropyThreshold)
    {
        whisperProcessorOptions.EntropyThreshold = entropyThreshold;
        return this;
    }

    /// <summary>
    /// Configures the processor with a average log probability threshold over sampled tokens.
    /// </summary>
    /// <param name="logProbThreshold">The average log probability threshold.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Default value is -1.0f.
    /// </remarks>
    public WhisperProcessorBuilder WithLogProbThreshold(float logProbThreshold)
    {
        whisperProcessorOptions.LogProbThreshold = logProbThreshold;
        return this;
    }

    /// <summary>
    /// [EXPERIMENTAL] Configures the processor with a no_speech probability.
    /// </summary>
    /// <param name="noSpeechThreshold">The no_speech probability</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// Default value is 0.6f.
    /// </remarks>
    public WhisperProcessorBuilder WithNoSpeechThreshold(float noSpeechThreshold)
    {
        whisperProcessorOptions.NoSpeechThreshold = noSpeechThreshold;
        return this;
    }

    /// <summary>
    /// Adds a <seealso cref="OnSegmentEventHandler"/> which will be called every time a new segment is detected.
    /// </summary>
    /// <param name="segmentEventHandler">The event handler to be added.</param>
    /// <returns>An instance to the same builder.</returns>
    public WhisperProcessorBuilder WithSegmentEventHandler(OnSegmentEventHandler segmentEventHandler)
    {
        whisperProcessorOptions.OnSegmentEventHandlers.Add(segmentEventHandler);
        return this;
    }

    /// <summary>
    /// Adds a <seealso cref="OnProgressHandler"/> which will report the progress in percentage.
    /// </summary>
    /// <param name="progressHandler">The event handler to be added.</param>
    /// <returns>An instance to the same builder.</returns>
    public WhisperProcessorBuilder WithProgressHandler(OnProgressHandler progressHandler)
    {
        whisperProcessorOptions.OnProgressHandlers.Add(progressHandler);
        return this;
    }

    /// <summary>
    /// Adds a <seealso cref="OnEncoderBeginEventHandler"/> which will be called when encoder will begin.
    /// </summary>
    /// <param name="encoderBeginEventHandler">The event handler to be added.</param>
    /// <returns>An instance to the same builder.</returns>
    public WhisperProcessorBuilder WithEncoderBeginHandler(OnEncoderBeginEventHandler encoderBeginEventHandler)
    {
        whisperProcessorOptions.OnEncoderBeginEventHandlers.Add(encoderBeginEventHandler);
        return this;
    }

    /// <summary>
    /// Adds the functionlity of pooling the strings that are generated reducing the number of allocations.
    /// </summary>
    /// <remarks>
    /// When using this option designed for high-performance use-cases,
    /// ensure that you're returning the <seealso cref="SegmentData"/> object back to the <seealso cref="WhisperProcessor"/>
    /// using the method <see cref="WhisperProcessor.Return(SegmentData)"/>.
    ///
    /// By default, this option is disabled.
    /// When calling this method with null, a default implementation of <seealso cref="IStringPool"/> will be used (reshared between all processors created for the <seealso cref="WhisperFactory"/>.
    /// </remarks>
    /// <returns>An instance to the same builder.</returns>
    public WhisperProcessorBuilder WithStringPool(IStringPool? stringPool = null)
    {
        whisperProcessorOptions.StringPool = stringPool ?? this.stringPool;
        return this;
    }

    /// <summary>
    /// Disables the string pooling.
    /// </summary>
    /// <remarks>
    /// This will disable the pooling of strings that are generated (have effect only if <seealso cref="WithStringPool"/> was called).
    /// </remarks>
    /// <returns>An instance to the same builder.</returns>
    public WhisperProcessorBuilder WithoutStringPool()
    {
        whisperProcessorOptions.StringPool = null;
        return this;
    }

    /// <summary>
    /// Configures the processor to use the Greedy Sampling strategy.
    /// </summary>
    /// <param name="configure">An optional action to configure the <seealso cref="GreedySamplingStrategy"/> that will be used.</param>
    /// <returns>A new <seealso cref="GreedySamplingStrategyBuilder"/> for configuring the <seealso cref="GreedySamplingStrategy"/></returns>
    public WhisperProcessorBuilder WithGreedySamplingStrategy(Action<GreedySamplingStrategyBuilder>? configure = null)
    {
        var greedySamplingStrategy = new GreedySamplingStrategy();
        whisperProcessorOptions.SamplingStrategy = greedySamplingStrategy;
        if (configure != null)
        {
            var greedySamplingStrategyBuilder = new GreedySamplingStrategyBuilder(greedySamplingStrategy);
            configure(greedySamplingStrategyBuilder);
        }

        return this;
    }

    /// <summary>
    /// Configures the processor to use the Beam Search Sampling Strategy.
    /// </summary>
    /// <param name="configure">An optional action to configure the <seealso cref="BeamSearchSamplingStrategy"/> that will be used.</param>
    /// <returns>A new <seealso cref="BeamSearchSamplingStrategyBuilder"/> for configuring the <seealso cref="BeamSearchSamplingStrategy"/></returns>
    public WhisperProcessorBuilder WithBeamSearchSamplingStrategy(Action<BeamSearchSamplingStrategyBuilder>? configure = null)
    {
        var beamSearchSamplingStrategy = new BeamSearchSamplingStrategy();
        whisperProcessorOptions.SamplingStrategy = beamSearchSamplingStrategy;
        if (configure != null)
        {
            var beamSearchBuilder = new BeamSearchSamplingStrategyBuilder(beamSearchSamplingStrategy);
            configure(beamSearchBuilder);
        }

        return this;
    }

    /// <summary>
    /// Confiugres the processor to return probabilities during segment decoding <seealso cref="SegmentData.MaxProbability"/>, <seealso cref="SegmentData.MinProbability"/> and <seealso cref="SegmentData.Probability"/>.
    /// </summary>
    /// <returns>An instance to the same builder.</returns>
    public WhisperProcessorBuilder WithProbabilities()
    {
        whisperProcessorOptions.ComputeProbabilities = true;
        return this;
    }

    /// <summary>
    /// Configures the options for OpenVino encoder.
    /// </summary>
    /// <param name="openVinoEncoderPath">
    /// Optional path to OpenVINO encoder IR model. If set to null, the path will be generated from the ggml model path that was passed if loaded from a file.
    /// </param>
    /// <param name="openVinoDevice">
    /// OpenVINO device to run inference on ("CPU", "GPU", etc.)
    /// </param>
    /// <param name="openVinoCachePath">
    /// Optional cache directory that can speed up init time, especially for  GPU, by caching compiled 'blobs' there. Null if not used.
    /// </param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// These options will be applied only if using OpenVino runtime.
    /// </remarks>
    public WhisperProcessorBuilder WithOpenVinoEncoder(string? openVinoEncoderPath, string? openVinoDevice, string? openVinoCachePath)
    {
        whisperProcessorOptions.OpenVinoModelPath = openVinoEncoderPath;
        whisperProcessorOptions.OpenVinoDevice = openVinoDevice;
        whisperProcessorOptions.OpenVinoCacheDir = openVinoCachePath;
        return this;
    }

    /// <summary>
    /// Builds the processor.
    /// </summary>
    /// <returns>The <seealso cref="WhisperProcessor"/> build with these configs.</returns>
    public WhisperProcessor Build()
    {
        return new WhisperProcessor(whisperProcessorOptions, nativeWhisper);
    }
}
