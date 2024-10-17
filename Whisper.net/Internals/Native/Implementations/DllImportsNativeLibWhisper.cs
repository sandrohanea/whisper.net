// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if !NET8_0_OR_GREATER
using System.Runtime.InteropServices;
using Whisper.net.Native;

namespace Whisper.net.Internals.Native.Implementations;

/// <summary>
/// This way of loading INativeWhisper is used on NetFramework + Wasm (as they doesn't support NativeLibrary)
/// </summary>
internal class DllImportsNativeLibWhisper : INativeWhisper
{
    const string libraryName = "libwhisper";
    const string ggmlLibraryName = "libggml";

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_from_file_with_params_no_state(string path, WhisperContextParams whisperContextParams);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_from_buffer_with_params_no_state(IntPtr buffer, nuint buffer_size, WhisperContextParams whisperContextParams);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free(IntPtr context);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free_params(IntPtr paramsPtr);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_full_default_params_by_ref(WhisperSamplingStrategy strategy);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_with_state(IntPtr context, IntPtr state, WhisperFullParams parameters, IntPtr samples, int nSamples);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_n_segments_from_state(IntPtr state);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long whisper_full_get_segment_t0_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern long whisper_full_get_segment_t1_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_full_get_segment_text_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_n_tokens_from_state(IntPtr state, int index);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern float whisper_full_get_token_p_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_lang_max_id();

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_lang_auto_detect_with_state(IntPtr context, IntPtr state, int offset_ms, int n_threads, IntPtr lang_probs);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_pcm_to_mel_with_state(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_lang_str(int lang_id);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_init_state(IntPtr context);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_free_state(IntPtr state);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern int whisper_full_lang_id_from_state(IntPtr state);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_log_set(IntPtr logCallback, IntPtr user_data);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void whisper_ctx_init_openvino_encoder_with_state(IntPtr context, IntPtr state, IntPtr modelPath, IntPtr device, IntPtr cacheDir);

    [DllImport(ggmlLibraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void ggml_log_set(IntPtr logCallback, IntPtr user_data);

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern IntPtr whisper_print_system_info();

    public INativeWhisper.whisper_init_from_file_with_params_no_state Whisper_Init_From_File_With_Params_No_State => whisper_init_from_file_with_params_no_state;

    public INativeWhisper.whisper_init_from_buffer_with_params_no_state Whisper_Init_From_Buffer_With_Params_No_State => whisper_init_from_buffer_with_params_no_state;

    public INativeWhisper.whisper_free Whisper_Free => whisper_free;

    public INativeWhisper.whisper_free_params Whisper_Free_Params => whisper_free_params;

    public INativeWhisper.whisper_full_default_params_by_ref Whisper_Full_Default_Params_By_Ref => whisper_full_default_params_by_ref;

    public INativeWhisper.whisper_full_with_state Whisper_Full_With_State => whisper_full_with_state;

    public INativeWhisper.whisper_full_n_segments_from_state Whisper_Full_N_Segments_From_State => whisper_full_n_segments_from_state;

    public INativeWhisper.whisper_full_get_segment_t0_from_state Whisper_Full_Get_Segment_T0_From_State => whisper_full_get_segment_t0_from_state;

    public INativeWhisper.whisper_full_get_segment_t1_from_state Whisper_Full_Get_Segment_T1_From_State => whisper_full_get_segment_t1_from_state;

    public INativeWhisper.whisper_full_get_segment_text_from_state Whisper_Full_Get_Segment_Text_From_State => whisper_full_get_segment_text_from_state;

    public INativeWhisper.whisper_full_n_tokens_from_state Whisper_Full_N_Tokens_From_State => whisper_full_n_tokens_from_state;

    public INativeWhisper.whisper_full_get_token_p_from_state Whisper_Full_Get_Token_P_From_State => whisper_full_get_token_p_from_state;

    public INativeWhisper.whisper_lang_max_id Whisper_Lang_Max_Id => whisper_lang_max_id;

    public INativeWhisper.whisper_lang_auto_detect_with_state Whisper_Lang_Auto_Detect_With_State => whisper_lang_auto_detect_with_state;

    public INativeWhisper.whisper_pcm_to_mel_with_state Whisper_PCM_To_Mel_With_State => whisper_pcm_to_mel_with_state;

    public INativeWhisper.whisper_lang_str Whisper_Lang_Str => whisper_lang_str;

    public INativeWhisper.whisper_init_state Whisper_Init_State => whisper_init_state;

    public INativeWhisper.whisper_free_state Whisper_Free_State => whisper_free_state;

    public INativeWhisper.whisper_full_lang_id_from_state Whisper_Full_Lang_Id_From_State => whisper_full_lang_id_from_state;

    public INativeWhisper.whisper_log_set Whisper_Log_Set => whisper_log_set;

    public INativeWhisper.whisper_ctx_init_openvino_encoder_with_state Whisper_Ctx_Init_Openvino_Encoder_With_State => whisper_ctx_init_openvino_encoder_with_state;

    public INativeWhisper.ggml_log_set Ggml_log_set => ggml_log_set;

    public INativeWhisper.whisper_print_system_info WhisperPrintSystemInfo => whisper_print_system_info;

    public void Dispose()
    {

    }
}
#endif
