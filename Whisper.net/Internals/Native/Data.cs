// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Native;

internal enum WhisperSamplingStrategy
{
    StrategyGreedy,      // GreedyDecoder
    StrategyBeamSearch, // BeamSearchDecoder
}

internal enum WhisperAlignmentHeadsPreset
{
    WHISPER_AHEADS_NONE,
    WHISPER_AHEADS_N_TOP_MOST,  // All heads from the N-top-most text-layers
    WHISPER_AHEADS_CUSTOM,
    WHISPER_AHEADS_TINY_EN,
    WHISPER_AHEADS_TINY,
    WHISPER_AHEADS_BASE_EN,
    WHISPER_AHEADS_BASE,
    WHISPER_AHEADS_SMALL_EN,
    WHISPER_AHEADS_SMALL,
    WHISPER_AHEADS_MEDIUM_EN,
    WHISPER_AHEADS_MEDIUM,
    WHISPER_AHEADS_LARGE_V1,
    WHISPER_AHEADS_LARGE_V2,
    WHISPER_AHEADS_LARGE_V3,
    WHISPER_AHEADS_LARGE_V3_TURBO
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperParamGreedy
{
    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/transcribe.py#L264
    public int BestOf;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperParamBeamSearch
{
    public int BeamSize;

    // Note: not implemented, ref: https://arxiv.org/pdf/2204.05424.pdf
    public float Patience;
}

internal enum GgmlLogLevel
{
    Error = 2,
    Warning = 3,
    Info = 4,
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WhisperNewSegmentCallback(IntPtr ctx, IntPtr state, int n_new, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate byte WhisperEncoderBeginCallback(IntPtr ctx, IntPtr state, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WhisperProgressCallback(IntPtr ctx, IntPtr state, int progress, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate byte WhisperLogitsFilterCallback(IntPtr ctx, IntPtr state, IntPtr tokens, int tokens_count, IntPtr logits, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate byte WhisperAbortCallback(IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WhisperGgmlLogCallback(GgmlLogLevel level, IntPtr message, IntPtr user_data);

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperAhead
{
    public int n_text_layer;
    public int n_head;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperAheads
{
    public UIntPtr NHeads;
    public IntPtr Heads;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperContextParams
{
    public byte UseGpu;
    public byte FlashAttention;
    public int GpuDevice;
    public byte DtwTokenLevelTimestamp;
    public WhisperAlignmentHeadsPreset HeadsPreset;
    public int DtwNTop;
    public WhisperAheads WhisperAheads;
    public UIntPtr Dtw_mem_size;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperFullParams
{
    public WhisperSamplingStrategy Strategy;

    public int Threads;

    // max tokens to use from past text as prompt for the decoder
    public int MaxLastTextTokens;

    // start offset in ms
    public int OffsetMs;

    // audio duration to process in ms
    public int DurationMs;

    public byte Translate;

    // Do not use past transcription (if any) as prompt for the decoder
    public byte NoContext;

    public byte NoTimestamps;

    //force single segment output (useful for streaming)
    public byte SingleSegment;

    // print special tokens (e.g. <SOT>, <EOT>, <BEG>, etc.)
    public byte PrintSpecialTokens;

    //print progress information
    public byte PrintProgress;

    // print results from within whisper.cpp (avoid it, use callback instead)
    public byte PrintResults;

    // print timestamps for each text segment when printing real-time
    public byte PrintTimestamps;

    // [EXPERIMENTAL] token-level timestamps
    // enable token-level timestamps
    public byte UseTokenTimestamps;

    // timestamp token probability threshold (~0.01)
    public float TokenTimestampsThreshold;

    // timestamp token sum probability threshold (~0.01)
    public float TokenTimestampsSumThreshold;

    // max segment length in characters
    public int MaxSegmentLength;

    public byte SplitOnWord;

    // max tokens per segment (0 = no limit)
    public int MaxTokensPerSegment;

    // [EXPERIMENTAL] speed-up techniques

    // enable debug_mode provides extra info (eg. Dump log_mel)
    public byte DebugMode;

    // overwrite the audio context size (0 = use default)
    public int AudioContextSize;

    // [EXPERIMENTAL] [TDRZ] tinydiarize
    // enable tinydiarize speaker turn detection
    public byte TinyDiarizeSpeakerTurnDirection;

    public IntPtr SuppressRegex;

    public IntPtr InitialPrompt;

    // tokens to provide to the whisper decoder as initial prompt
    // these are prepended to any existing text context from a previous call
    public IntPtr PromptTokens;

    public int PromptNTokens;

    // for auto-detection, set to nullptr, "" or "auto"
    public IntPtr Language;

    // Will end the pipeline after detecting the language. Not used by whisper.net
    public byte DetectLanguage;

    // common decoding parameters:
    public byte SuppressBlank;

    // suppress non-speech tokens (e.g. `,`, `.`, etc.)
    public byte SupressNonSpeechTokens;

    // common decoding parameters:
    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/decoding.py#L89
    public float Temperature;

    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/decoding.py#L97
    public float MaxInitialTs;

    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/transcribe.py#L267
    public float LengthPenalty;

    // fallback parameters
    // ref: https://github.com/openai/whisper/blob/f82bc59f5ea234d4b97fb2860842ed38519f7e65/whisper/transcribe.py#L274-L278
    public float TemperatureInc;

    public float EntropyThreshold;

    public float LogProbThreshold;

    // Note: not implemented yet.
    public float NoSpeechThreshold;

    public WhisperParamGreedy WhisperParamGreedy;

    public WhisperParamBeamSearch WhisperParamBeamSearch;

    public IntPtr OnNewSegment;

    public IntPtr OnNewSegmentUserData;

    public IntPtr OnProgressCallback;

    public IntPtr OnProgressCallbackUserData;

    public IntPtr OnEncoderBegin;

    public IntPtr OnEncoderBeginUserData;

    // Called each time before ggml computation starts
    public IntPtr OnAbort;

    public IntPtr OnAbortUserData;

    public IntPtr LogitsFilterCallback;

    public IntPtr LogitsFilterCallbackData;

    public IntPtr WhisperGrammarElement;

    public UIntPtr NGrammarRules;

    public UIntPtr StartGrammarRule;

    public float GrammarPenalty;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperContext
{
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperTokenData
{
    public int id;
    public int tid;
    public float p;
    public float plog;
    public float pt;
    public float ptsum;
    public long t0;
    public long t1;
    public long t_dtw;
    public float vlen;
}
