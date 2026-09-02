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

        var unsupportedOption = GetFirstUnsupportedParakeetOption();
        if (unsupportedOption is not null)
        {
            throw new NotSupportedException($"{unsupportedOption} is not supported by the Parakeet engine.");
        }
    }

    private string? GetFirstUnsupportedParakeetOption()
    {
        if (MaxLastTextTokens.HasValue) return "Previous-text prompt tokens";
        if (Translate.HasValue) return "Translation";
        if (SingleSegment.HasValue) return "Single-segment output";
        if (PrintSpecialTokens.HasValue) return "Printing special tokens";
        if (PrintResults.HasValue) return "Printing results";
        if (PrintTimestamps.HasValue) return "Printing timestamps";
        if (UseTokenTimestamps.HasValue) return "Whisper token timestamps";
        if (TokenTimestampsThreshold.HasValue) return "Token timestamp threshold";
        if (TokenTimestampsSumThreshold.HasValue) return "Token timestamp sum threshold";
        if (MaxSegmentLength.HasValue) return "Maximum segment length";
        if (SplitOnWord.HasValue) return "Split-on-word";
        if (MaxTokensPerSegment.HasValue) return "Maximum tokens per segment";
        if (SuppressRegex is not null) return "Token suppression regex";
        if (Prompt is not null) return "Initial prompts";
        if (CarryInitialPrompt.HasValue) return "Initial prompt carry-over";
        if (Language is not null) return "Language selection or detection";
        if (SuppressBlank.HasValue) return "Blank suppression";
        if (Temperature.HasValue) return "Temperature";
        if (MaxInitialTs.HasValue) return "Maximum initial timestamp";
        if (LengthPenalty.HasValue) return "Length penalty";
        if (TemperatureInc.HasValue) return "Temperature fallback";
        if (EntropyThreshold.HasValue) return "Entropy fallback threshold";
        if (LogProbThreshold.HasValue) return "Log-probability fallback threshold";
        if (NoSpeechThreshold.HasValue) return "No-speech threshold";
        if (SamplingStrategy is BeamSearchSamplingStrategy) return "Beam-search sampling";
        if (SamplingStrategy is GreedySamplingStrategy { BestOf: not null }) return "Greedy best-of sampling";
        if (OpenVinoModelPath is not null || OpenVinoDevice is not null || OpenVinoCacheDir is not null) return "OpenVINO encoding";

        return null;
    }
}
