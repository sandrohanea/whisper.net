// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Native;

namespace Whisper.net.Internals.Native;

internal interface INativeWhisper : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_init_from_file_with_params_no_state(IntPtr path, WhisperContextParams whisperContextParams);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_init_from_buffer_with_params_no_state(IntPtr buffer, nuint buffer_size, WhisperContextParams whisperContextParams);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void whisper_free(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void whisper_free_params(IntPtr paramsPtr);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_full_default_params_by_ref(WhisperSamplingStrategy strategy);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate int whisper_full_with_state(IntPtr context, IntPtr state, WhisperFullParams parameters, IntPtr samples, int nSamples);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate int whisper_full_n_segments_from_state(IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate long whisper_full_get_segment_t0_from_state(IntPtr state, int index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate long whisper_full_get_segment_t1_from_state(IntPtr state, int index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_full_get_segment_text_from_state(IntPtr state, int index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate int whisper_full_n_tokens_from_state(IntPtr state, int index);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate float whisper_full_get_token_p_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate int whisper_lang_max_id();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate int whisper_lang_auto_detect_with_state(IntPtr context, IntPtr state, int offset_ms, int n_threads, IntPtr lang_probs);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate int whisper_pcm_to_mel_with_state(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_lang_str(int lang_id);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_init_state(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void whisper_free_state(IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate int whisper_full_lang_id_from_state(IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void whisper_log_set(IntPtr logCallback, IntPtr user_data);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate void whisper_ctx_init_openvino_encoder_with_state(IntPtr context, IntPtr state, IntPtr modelPath, IntPtr device, IntPtr cacheDir);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate WhisperTokenData whisper_full_get_token_data_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_full_get_token_text_from_state(IntPtr context, IntPtr state, int segmentIndex, int tokenIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate IntPtr whisper_print_system_info();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public delegate float whisper_full_get_segment_no_speech_prob_from_state(IntPtr state, int index);

    whisper_init_from_file_with_params_no_state Whisper_Init_From_File_With_Params_No_State { get; }
    whisper_init_from_buffer_with_params_no_state Whisper_Init_From_Buffer_With_Params_No_State { get; }
    whisper_free Whisper_Free { get; }
    whisper_free_params Whisper_Free_Params { get; }
    whisper_full_default_params_by_ref Whisper_Full_Default_Params_By_Ref { get; }
    whisper_full_with_state Whisper_Full_With_State { get; }
    whisper_full_n_segments_from_state Whisper_Full_N_Segments_From_State { get; }
    whisper_full_get_segment_t0_from_state Whisper_Full_Get_Segment_T0_From_State { get; }
    whisper_full_get_segment_t1_from_state Whisper_Full_Get_Segment_T1_From_State { get; }
    whisper_full_get_segment_text_from_state Whisper_Full_Get_Segment_Text_From_State { get; }
    whisper_full_n_tokens_from_state Whisper_Full_N_Tokens_From_State { get; }
    whisper_full_get_token_p_from_state Whisper_Full_Get_Token_P_From_State { get; }
    whisper_lang_max_id Whisper_Lang_Max_Id { get; }
    whisper_lang_auto_detect_with_state Whisper_Lang_Auto_Detect_With_State { get; }
    whisper_pcm_to_mel_with_state Whisper_PCM_To_Mel_With_State { get; }
    whisper_lang_str Whisper_Lang_Str { get; }
    whisper_init_state Whisper_Init_State { get; }
    whisper_free_state Whisper_Free_State { get; }
    whisper_full_lang_id_from_state Whisper_Full_Lang_Id_From_State { get; }
    whisper_log_set Whisper_Log_Set { get; }
    whisper_ctx_init_openvino_encoder_with_state Whisper_Ctx_Init_Openvino_Encoder_With_State { get; }
    whisper_full_get_token_data_from_state Whisper_Full_Get_Token_Data_From_State { get; }
    whisper_full_get_token_text_from_state Whisper_Full_Get_Token_Text_From_State { get; }
    whisper_print_system_info WhisperPrintSystemInfo { get; }

    whisper_full_get_segment_no_speech_prob_from_state Whisper_Full_Get_Segment_No_Speech_Prob_From_State { get; }
}
