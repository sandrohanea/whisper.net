// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Native;

internal enum WhisperSamplingStrategy
{
    StrategyGreedy,      // GreedyDecoder
    StrategyBeamSearch, // BeamSearchDecoder
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

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WhisperNewSegmentCallback(IntPtr ctx, IntPtr state, int n_new, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate byte WhisperEncoderBeginCallback(IntPtr ctx, IntPtr state, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void WhisperProgressCallback(IntPtr ctx, IntPtr state, int progress, IntPtr user_data);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate bool WhisperLogitsFilterCallback(IntPtr ctx, IntPtr state, IntPtr tokens, int tokens_count, IntPtr logits, IntPtr user_data);

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
    // note: these can significantly reduce the quality of the output
    // speed-up the audio by 2x using Phase Vocoder
    public byte SpeedUp2x;

    // enable debug_mode provides extra info (eg. Dump log_mel)
    public byte DebugMode;

    // overwrite the audio context size (0 = use default)
    public int AudioContextSize;

    // [EXPERIMENTAL] [TDRZ] tinydiarize
    // enable tinydiarize speaker turn detection
    public byte TinyDiarizeSpeakerTurnDirection;

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

    public IntPtr LogitsFilterCallback;

    public IntPtr LogitsFilterCallbackData;
}

[StructLayout(LayoutKind.Sequential)]
internal struct WhisperContext
{
}
