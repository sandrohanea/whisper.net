// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.SamplingStrategy;

namespace Whisper.net;

#pragma warning disable CS0618 // Retained for backward compatibility until the next major version.

internal sealed class WhisperProcessorOptions
{
    public WhisperModelFamily ModelFamily { get; set; }

    public string? OpenVinoModelPath { get; set; }

    public string? OpenVinoDevice { get; set; }

    public string? OpenVinoCacheDir { get; set; }

    public IWhisperSamplingStrategy SamplingStrategy { get; set; } = new GreedySamplingStrategy();

    public IntPtr ContextHandle { get; set; }

    public int? Threads { get; set; }

    public int? MaxLastTextTokens { get; set; }

    public TimeSpan? Offset { get; set; }

    public TimeSpan? Duration { get; set; }

    public bool? Translate { get; set; }

    public bool? NoContext { get; set; }

    public bool? SingleSegment { get; set; }

    public bool? PrintSpecialTokens { get; set; }

    public bool? PrintProgress { get; set; } = false;

    public bool? PrintResults { get; set; }

    public bool? PrintTimestamps { get; set; }

    public bool? UseTokenTimestamps { get; set; }

    public float? TokenTimestampsThreshold { get; set; }

    public float? TokenTimestampsSumThreshold { get; set; }

    public int? MaxSegmentLength { get; set; }

    public bool? SplitOnWord { get; set; }

    public int? MaxTokensPerSegment { get; set; }

    public int? AudioContextSize { get; set; }

    public Internals.Grammar.ParsedGrammar? Grammar { get; set; }

    public float GrammarPenalty { get; set; } = 100f;

    public string? SuppressRegex { get; set; }

    public string? Prompt { get; set; }

    public bool? CarryInitialPrompt { get; set; }

    public string? Language { get; set; }

    public bool? SuppressBlank { get; set; }

    public float? Temperature { get; set; }

    public float? MaxInitialTs { get; set; }

    public float? LengthPenalty { get; set; }

    public float? TemperatureInc { get; set; }

    public float? EntropyThreshold { get; set; }

    public float? LogProbThreshold { get; set; }

    public float? NoSpeechThreshold { get; set; }

    public List<OnSegmentEventHandler> OnSegmentEventHandlers { get; set; } = [];

    public List<OnProgressHandler> OnProgressHandlers { get; set; } = [];

    public List<OnEncoderBeginEventHandler> OnEncoderBeginEventHandlers { get; set; } = [];

    public WhisperAbortEventHandler? WhisperAbortEventHandler { get; set; }

    public bool ComputeProbabilities { get; set; }

    public IStringPool? StringPool { get; set; }

    public void ValidateModelFamilyCompatibility()
    {
        if (ModelFamily != WhisperModelFamily.Parakeet)
        {
            return;
        }

        var unsupportedOptions = GetUnsupportedParakeetOptions().ToArray();
        if (unsupportedOptions.Length > 0)
        {
            throw new NotSupportedException($"The following options are not supported by the Parakeet engine: {string.Join(", ", unsupportedOptions)}.");
        }
    }

    public IEnumerable<string> GetUnsupportedParakeetOptions()
    {
        if (MaxLastTextTokens.HasValue)
        {
            yield return "Previous-text prompt tokens";
        }

        if (Translate.HasValue)
        {
            yield return "Translation";
        }

        if (SingleSegment.HasValue)
        {
            yield return "Single-segment output";
        }

        if (PrintSpecialTokens.HasValue)
        {
            yield return "Printing special tokens";
        }

        if (PrintProgress == true)
        {
            yield return "Printing progress";
        }

        if (PrintResults.HasValue)
        {
            yield return "Printing results";
        }

        if (PrintTimestamps.HasValue)
        {
            yield return "Printing timestamps";
        }

        if (UseTokenTimestamps.HasValue)
        {
            yield return "Whisper token timestamps";
        }

        if (TokenTimestampsThreshold.HasValue)
        {
            yield return "Token timestamp threshold";
        }

        if (TokenTimestampsSumThreshold.HasValue)
        {
            yield return "Token timestamp sum threshold";
        }

        if (MaxSegmentLength.HasValue)
        {
            yield return "Maximum segment length";
        }

        if (SplitOnWord.HasValue)
        {
            yield return "Split-on-word";
        }

        if (MaxTokensPerSegment.HasValue)
        {
            yield return "Maximum tokens per segment";
        }

        if (Grammar is not null)
        {
            yield return "Grammar constraints";
        }

        if (SuppressRegex is not null)
        {
            yield return "Token suppression regex";
        }

        if (Prompt is not null)
        {
            yield return "Initial prompts";
        }

        if (CarryInitialPrompt.HasValue)
        {
            yield return "Initial prompt carry-over";
        }

        if (Language is not null)
        {
            yield return "Language selection or detection";
        }

        if (SuppressBlank.HasValue)
        {
            yield return "Blank suppression";
        }

        if (Temperature.HasValue)
        {
            yield return "Temperature";
        }

        if (MaxInitialTs.HasValue)
        {
            yield return "Maximum initial timestamp";
        }

        if (LengthPenalty.HasValue)
        {
            yield return "Length penalty";
        }

        if (TemperatureInc.HasValue)
        {
            yield return "Temperature fallback";
        }

        if (EntropyThreshold.HasValue)
        {
            yield return "Entropy fallback threshold";
        }

        if (LogProbThreshold.HasValue)
        {
            yield return "Log-probability fallback threshold";
        }

        if (NoSpeechThreshold.HasValue)
        {
            yield return "No-speech threshold";
        }

        if (SamplingStrategy is BeamSearchSamplingStrategy)
        {
            yield return "Beam-search sampling";
        }

        if (SamplingStrategy is GreedySamplingStrategy { BestOf: not null })
        {
            yield return "Greedy best-of sampling";
        }

        if (OpenVinoModelPath is not null || OpenVinoDevice is not null || OpenVinoCacheDir is not null)
        {
            yield return "OpenVINO encoding";
        }
    }
}
