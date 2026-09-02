// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if !NETSTANDARD
using System.Runtime.InteropServices;
using static Whisper.net.Internals.Native.INativeParakeet;

namespace Whisper.net.Internals.Native.Implementations;

internal sealed class NativeLibraryParakeet : INativeParakeet
{
    private readonly IntPtr libraryHandle;

    public NativeLibraryParakeet(IntPtr libraryHandle)
    {
        Parakeet_Init_With_Params_No_State = GetExport<parakeet_init_with_params_no_state>(libraryHandle);
        Parakeet_Free = GetExport<parakeet_free>(libraryHandle);
        Parakeet_Init_State = GetExport<parakeet_init_state>(libraryHandle);
        Parakeet_Free_State = GetExport<parakeet_free_state>(libraryHandle);
        Parakeet_Full_Default_Params_By_Ref = GetExport<parakeet_full_default_params_by_ref>(libraryHandle);
        Parakeet_Free_Params = GetExport<parakeet_free_params>(libraryHandle);
        Parakeet_Full_With_State = GetExport<parakeet_full_with_state>(libraryHandle);
        Parakeet_Full_N_Segments_From_State = GetExport<parakeet_full_n_segments_from_state>(libraryHandle);
        Parakeet_Full_Get_Segment_T0_From_State = GetExport<parakeet_full_get_segment_t0_from_state>(libraryHandle);
        Parakeet_Full_Get_Segment_T1_From_State = GetExport<parakeet_full_get_segment_t1_from_state>(libraryHandle);
        Parakeet_Full_Get_Segment_Text_From_State = GetExport<parakeet_full_get_segment_text_from_state>(libraryHandle);
        Parakeet_Full_N_Tokens_From_State = GetExport<parakeet_full_n_tokens_from_state>(libraryHandle);
        Parakeet_Full_Get_Token_Data_From_State = GetExport<parakeet_full_get_token_data_from_state>(libraryHandle);
        Parakeet_Full_Get_Token_Text_From_State = GetExport<parakeet_full_get_token_text_from_state>(libraryHandle);
        Parakeet_Full_Get_Token_P_From_State = GetExport<parakeet_full_get_token_p_from_state>(libraryHandle);
        Parakeet_Print_System_Info = GetExport<parakeet_print_system_info>(libraryHandle);
        this.libraryHandle = libraryHandle;
    }

    public parakeet_init_with_params_no_state Parakeet_Init_With_Params_No_State { get; }
    public parakeet_free Parakeet_Free { get; }
    public parakeet_init_state Parakeet_Init_State { get; }
    public parakeet_free_state Parakeet_Free_State { get; }
    public parakeet_full_default_params_by_ref Parakeet_Full_Default_Params_By_Ref { get; }
    public parakeet_free_params Parakeet_Free_Params { get; }
    public parakeet_full_with_state Parakeet_Full_With_State { get; }
    public parakeet_full_n_segments_from_state Parakeet_Full_N_Segments_From_State { get; }
    public parakeet_full_get_segment_t0_from_state Parakeet_Full_Get_Segment_T0_From_State { get; }
    public parakeet_full_get_segment_t1_from_state Parakeet_Full_Get_Segment_T1_From_State { get; }
    public parakeet_full_get_segment_text_from_state Parakeet_Full_Get_Segment_Text_From_State { get; }
    public parakeet_full_n_tokens_from_state Parakeet_Full_N_Tokens_From_State { get; }
    public parakeet_full_get_token_data_from_state Parakeet_Full_Get_Token_Data_From_State { get; }
    public parakeet_full_get_token_text_from_state Parakeet_Full_Get_Token_Text_From_State { get; }
    public parakeet_full_get_token_p_from_state Parakeet_Full_Get_Token_P_From_State { get; }
    public parakeet_print_system_info Parakeet_Print_System_Info { get; }

    public void Dispose()
    {
        NativeLibrary.Free(libraryHandle);
    }

    private static T GetExport<T>(IntPtr handle) where T : Delegate
    {
        var export = NativeLibrary.GetExport(handle, typeof(T).Name);
        return Marshal.GetDelegateForFunctionPointer<T>(export);
    }
}
#endif
