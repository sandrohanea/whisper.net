// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
using Whisper.net.Native;

namespace Whisper.net.Internals.Native.Implementations;

/// <summary>
/// This way of loading INativeWhisper is using the LibraryImport attribute to load the native library.
/// </summary>
internal partial class LibraryImportLibWhisper : INativeWhisper
{
    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_init_from_file_with_params_no_state(IntPtr path, WhisperContextParams whisperContextParams);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_init_from_buffer_with_params_no_state(IntPtr buffer, nuint buffer_size, WhisperContextParams whisperContextParams);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_free(IntPtr context);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_free_params(IntPtr paramsPtr);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_full_default_params_by_ref(WhisperSamplingStrategy strategy);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_full_with_state(IntPtr context, IntPtr state, WhisperFullParams parameters, IntPtr samples, int nSamples);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_full_n_segments_from_state(IntPtr state);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial long whisper_full_get_segment_t0_from_state(IntPtr state, int index);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial long whisper_full_get_segment_t1_from_state(IntPtr state, int index);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_full_get_segment_text_from_state(IntPtr state, int index);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_full_n_tokens_from_state(IntPtr state, int index);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial float whisper_full_get_token_p_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_lang_max_id();

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_lang_id(IntPtr lang);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_lang_auto_detect_with_state(IntPtr context, IntPtr state, int offset_ms, int n_threads, IntPtr lang_probs);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_pcm_to_mel_with_state(IntPtr context, IntPtr state, IntPtr samples, int nSamples, int nThreads);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_lang_str(int lang_id);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_init_state(IntPtr context);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_free_state(IntPtr state);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_full_lang_id_from_state(IntPtr state);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_log_set(IntPtr logCallback, IntPtr user_data);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_ctx_init_openvino_encoder_with_state(IntPtr context, IntPtr state, IntPtr modelPath, IntPtr device, IntPtr cacheDir);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial WhisperTokenData whisper_full_get_token_data_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_full_get_token_text_from_state(IntPtr context, IntPtr state, int segmentIndex, int tokenIndex);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_print_system_info();

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial float whisper_full_get_segment_no_speech_prob_from_state(IntPtr state, int index);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial WhisperVadParams whisper_vad_default_params();

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial WhisperVadContextParams whisper_vad_default_context_params();

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_vad_init_from_file_with_params(IntPtr path, WhisperVadContextParams parameters);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial byte whisper_vad_detect_speech(IntPtr context, IntPtr samples, int nSamples);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial byte whisper_vad_detect_speech_no_reset(IntPtr context, IntPtr samples, int nSamples);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_vad_reset_state(IntPtr context);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial IntPtr whisper_vad_segments_from_probs(IntPtr context, WhisperVadParams parameters);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial int whisper_vad_segments_n_segments(IntPtr segments);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial float whisper_vad_segments_get_segment_t0(IntPtr segments, int index);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial float whisper_vad_segments_get_segment_t1(IntPtr segments, int index);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_vad_free_segments(IntPtr segments);

    [LibraryImport(NativeConstants.LibWhisperLibraryName)]
    private static partial void whisper_vad_free(IntPtr context);

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

    public INativeWhisper.whisper_lang_id Whisper_Lang_Id => whisper_lang_id;

    public INativeWhisper.whisper_lang_auto_detect_with_state Whisper_Lang_Auto_Detect_With_State => whisper_lang_auto_detect_with_state;

    public INativeWhisper.whisper_pcm_to_mel_with_state Whisper_PCM_To_Mel_With_State => whisper_pcm_to_mel_with_state;

    public INativeWhisper.whisper_lang_str Whisper_Lang_Str => whisper_lang_str;

    public INativeWhisper.whisper_init_state Whisper_Init_State => whisper_init_state;

    public INativeWhisper.whisper_free_state Whisper_Free_State => whisper_free_state;

    public INativeWhisper.whisper_full_lang_id_from_state Whisper_Full_Lang_Id_From_State => whisper_full_lang_id_from_state;

    public INativeWhisper.whisper_log_set Whisper_Log_Set => whisper_log_set;

    public INativeWhisper.whisper_ctx_init_openvino_encoder_with_state Whisper_Ctx_Init_Openvino_Encoder_With_State => whisper_ctx_init_openvino_encoder_with_state;

    public INativeWhisper.whisper_full_get_token_data_from_state Whisper_Full_Get_Token_Data_From_State => whisper_full_get_token_data_from_state;

    public INativeWhisper.whisper_full_get_token_text_from_state Whisper_Full_Get_Token_Text_From_State =>
        whisper_full_get_token_text_from_state;

    public INativeWhisper.whisper_print_system_info WhisperPrintSystemInfo => whisper_print_system_info;

    public INativeWhisper.whisper_full_get_segment_no_speech_prob_from_state
        Whisper_Full_Get_Segment_No_Speech_Prob_From_State => whisper_full_get_segment_no_speech_prob_from_state;

    public INativeWhisper.whisper_vad_default_params Whisper_Vad_Default_Params => whisper_vad_default_params;

    public INativeWhisper.whisper_vad_default_context_params Whisper_Vad_Default_Context_Params => whisper_vad_default_context_params;

    public INativeWhisper.whisper_vad_init_from_file_with_params Whisper_Vad_Init_From_File_With_Params => whisper_vad_init_from_file_with_params;

    public INativeWhisper.whisper_vad_detect_speech Whisper_Vad_Detect_Speech => whisper_vad_detect_speech;

    public INativeWhisper.whisper_vad_detect_speech_no_reset Whisper_Vad_Detect_Speech_No_Reset => whisper_vad_detect_speech_no_reset;

    public INativeWhisper.whisper_vad_reset_state Whisper_Vad_Reset_State => whisper_vad_reset_state;

    public INativeWhisper.whisper_vad_segments_from_probs Whisper_Vad_Segments_From_Probs => whisper_vad_segments_from_probs;

    public INativeWhisper.whisper_vad_segments_n_segments Whisper_Vad_Segments_N_Segments => whisper_vad_segments_n_segments;

    public INativeWhisper.whisper_vad_segments_get_segment_t0 Whisper_Vad_Segments_Get_Segment_T0 => whisper_vad_segments_get_segment_t0;

    public INativeWhisper.whisper_vad_segments_get_segment_t1 Whisper_Vad_Segments_Get_Segment_T1 => whisper_vad_segments_get_segment_t1;

    public INativeWhisper.whisper_vad_free_segments Whisper_Vad_Free_Segments => whisper_vad_free_segments;

    public INativeWhisper.whisper_vad_free Whisper_Vad_Free => whisper_vad_free;

    public void Dispose()
    {
    }
}

#endif
