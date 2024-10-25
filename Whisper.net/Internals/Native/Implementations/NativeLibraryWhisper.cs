// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if !NETSTANDARD
using System.Runtime.InteropServices;
using static Whisper.net.Internals.Native.INativeWhisper;

namespace Whisper.net.Internals.Native.Implementations;

internal class NativeLibraryWhisper : INativeWhisper
{
    private readonly IntPtr whisperLibraryHandle;
    private readonly IntPtr ggmlLibraryHandle;

    public NativeLibraryWhisper(IntPtr whisperLibraryHandle, IntPtr ggmlLibraryHandle)
    {
        Whisper_Init_From_File_With_Params_No_State = Marshal.GetDelegateForFunctionPointer<whisper_init_from_file_with_params_no_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_init_from_file_with_params_no_state)));
        Whisper_Init_From_Buffer_With_Params_No_State = Marshal.GetDelegateForFunctionPointer<whisper_init_from_buffer_with_params_no_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_init_from_buffer_with_params_no_state)));
        Whisper_Free = Marshal.GetDelegateForFunctionPointer<whisper_free>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_free)));
        Whisper_Free_Params = Marshal.GetDelegateForFunctionPointer<whisper_free_params>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_free_params)));
        Whisper_Full_Default_Params_By_Ref = Marshal.GetDelegateForFunctionPointer<whisper_full_default_params_by_ref>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_default_params_by_ref)));
        Whisper_Full_With_State = Marshal.GetDelegateForFunctionPointer<whisper_full_with_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_with_state)));
        Whisper_Full_N_Segments_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_n_segments_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_n_segments_from_state)));
        Whisper_Full_Get_Segment_T0_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_get_segment_t0_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_get_segment_t0_from_state)));
        Whisper_Full_Get_Segment_T1_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_get_segment_t1_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_get_segment_t1_from_state)));
        Whisper_Full_Get_Segment_Text_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_get_segment_text_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_get_segment_text_from_state)));
        Whisper_Full_N_Tokens_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_n_tokens_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_n_tokens_from_state)));
        Whisper_Full_Get_Token_P_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_get_token_p_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_get_token_p_from_state)));
        Whisper_Lang_Max_Id = Marshal.GetDelegateForFunctionPointer<whisper_lang_max_id>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_lang_max_id)));
        Whisper_Lang_Auto_Detect_With_State = Marshal.GetDelegateForFunctionPointer<whisper_lang_auto_detect_with_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_lang_auto_detect_with_state)));
        Whisper_PCM_To_Mel_With_State = Marshal.GetDelegateForFunctionPointer<whisper_pcm_to_mel_with_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_pcm_to_mel_with_state)));
        Whisper_Lang_Str = Marshal.GetDelegateForFunctionPointer<whisper_lang_str>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_lang_str)));
        Whisper_Init_State = Marshal.GetDelegateForFunctionPointer<whisper_init_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_init_state)));
        Whisper_Free_State = Marshal.GetDelegateForFunctionPointer<whisper_free_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_free_state)));
        Whisper_Full_Lang_Id_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_lang_id_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_lang_id_from_state)));
        Whisper_Log_Set = Marshal.GetDelegateForFunctionPointer<whisper_log_set>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_log_set)));
        Whisper_Ctx_Init_Openvino_Encoder_With_State = Marshal.GetDelegateForFunctionPointer<whisper_ctx_init_openvino_encoder_with_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_ctx_init_openvino_encoder_with_state)));
        Whisper_Full_Get_Token_Data_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_get_token_data_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_get_token_data_from_state)));
        Whisper_Full_Get_Token_Text_From_State = Marshal.GetDelegateForFunctionPointer<whisper_full_get_token_text_from_state>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_full_get_token_text_from_state)));
        Ggml_log_set = Marshal.GetDelegateForFunctionPointer<ggml_log_set>(NativeLibrary.GetExport(ggmlLibraryHandle, nameof(ggml_log_set)));
        WhisperPrintSystemInfo = Marshal.GetDelegateForFunctionPointer<whisper_print_system_info>(NativeLibrary.GetExport(whisperLibraryHandle, nameof(whisper_print_system_info)));

        this.whisperLibraryHandle = whisperLibraryHandle;
        this.ggmlLibraryHandle = ggmlLibraryHandle;
    }

    public whisper_init_from_file_with_params_no_state Whisper_Init_From_File_With_Params_No_State { get; }

    public whisper_init_from_buffer_with_params_no_state Whisper_Init_From_Buffer_With_Params_No_State { get; }

    public whisper_free Whisper_Free { get; }

    public whisper_free_params Whisper_Free_Params { get; }

    public whisper_full_default_params_by_ref Whisper_Full_Default_Params_By_Ref { get; }

    public whisper_full_with_state Whisper_Full_With_State { get; }

    public whisper_full_n_segments_from_state Whisper_Full_N_Segments_From_State { get; }

    public whisper_full_get_segment_t0_from_state Whisper_Full_Get_Segment_T0_From_State { get; }

    public whisper_full_get_segment_t1_from_state Whisper_Full_Get_Segment_T1_From_State { get; }

    public whisper_full_get_segment_text_from_state Whisper_Full_Get_Segment_Text_From_State { get; }

    public whisper_full_n_tokens_from_state Whisper_Full_N_Tokens_From_State { get; }

    public whisper_full_get_token_p_from_state Whisper_Full_Get_Token_P_From_State { get; }

    public whisper_lang_max_id Whisper_Lang_Max_Id { get; }

    public whisper_lang_auto_detect_with_state Whisper_Lang_Auto_Detect_With_State { get; }

    public whisper_pcm_to_mel_with_state Whisper_PCM_To_Mel_With_State { get; }

    public whisper_lang_str Whisper_Lang_Str { get; }

    public whisper_init_state Whisper_Init_State { get; }

    public whisper_free_state Whisper_Free_State { get; }

    public whisper_full_lang_id_from_state Whisper_Full_Lang_Id_From_State { get; }

    public whisper_log_set Whisper_Log_Set { get; }

    public whisper_ctx_init_openvino_encoder_with_state Whisper_Ctx_Init_Openvino_Encoder_With_State { get; }

    public whisper_full_get_token_data_from_state Whisper_Full_Get_Token_Data_From_State { get; }

    public whisper_full_get_token_text_from_state Whisper_Full_Get_Token_Text_From_State { get; }

    public ggml_log_set Ggml_log_set { get; }

    public whisper_print_system_info WhisperPrintSystemInfo { get; }

    public void Dispose()
    {
        NativeLibrary.Free(whisperLibraryHandle);
        NativeLibrary.Free(ggmlLibraryHandle);
    }
}
#endif
