using Whisper.net.SamplingStrategy;

namespace Whisper.net;

internal sealed class WhisperProcessorOptions
{
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

    public bool? SpeedUp2x { get; set; }
    public int? AudioContextSize { get; set; }

    public string? Prompt { get; set; }

    public string? Language { get; set; }

    public bool? SuppressBlank { get; set; }

    public float? Temperature { get; set; }

    public float? MaxInitialTs { get; set; }

    public float? LengthPenalty { get; set; }

    public float? TemperatureInc { get; set; }

    public float? EntropyThreshold { get; set; }

    public float? LogProbThreshold { get; set; }

    public float? NoSpeechThreshold { get; set; }
    
    public List<OnSegmentEventHandler> OnSegmentEventHandlers { get; set; } = new();

    public List<OnEncoderBeginEventHandler> OnEncoderBeginEventHandlers { get; set; } = new();
}
