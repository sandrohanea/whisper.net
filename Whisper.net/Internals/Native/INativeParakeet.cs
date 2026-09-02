// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Native;

namespace Whisper.net.Internals.Native;

internal interface INativeParakeet : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr parakeet_init_with_params_no_state(ref WhisperModelLoader loader, ParakeetContextParams parameters);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void parakeet_free(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr parakeet_init_state(IntPtr context);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void parakeet_free_state(IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr parakeet_full_default_params_by_ref(ParakeetSamplingStrategy strategy);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void parakeet_free_params(IntPtr parameters);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int parakeet_full_with_state(IntPtr context, IntPtr state, ParakeetFullParams parameters, IntPtr samples, int sampleCount);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int parakeet_full_n_segments_from_state(IntPtr state);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long parakeet_full_get_segment_t0_from_state(IntPtr state, int segmentIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate long parakeet_full_get_segment_t1_from_state(IntPtr state, int segmentIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr parakeet_full_get_segment_text_from_state(IntPtr state, int segmentIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int parakeet_full_n_tokens_from_state(IntPtr state, int segmentIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ParakeetTokenData parakeet_full_get_token_data_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr parakeet_full_get_token_text_from_state(IntPtr context, IntPtr state, int segmentIndex, int tokenIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate float parakeet_full_get_token_p_from_state(IntPtr state, int segmentIndex, int tokenIndex);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr parakeet_print_system_info();

    parakeet_init_with_params_no_state Parakeet_Init_With_Params_No_State { get; }
    parakeet_free Parakeet_Free { get; }
    parakeet_init_state Parakeet_Init_State { get; }
    parakeet_free_state Parakeet_Free_State { get; }
    parakeet_full_default_params_by_ref Parakeet_Full_Default_Params_By_Ref { get; }
    parakeet_free_params Parakeet_Free_Params { get; }
    parakeet_full_with_state Parakeet_Full_With_State { get; }
    parakeet_full_n_segments_from_state Parakeet_Full_N_Segments_From_State { get; }
    parakeet_full_get_segment_t0_from_state Parakeet_Full_Get_Segment_T0_From_State { get; }
    parakeet_full_get_segment_t1_from_state Parakeet_Full_Get_Segment_T1_From_State { get; }
    parakeet_full_get_segment_text_from_state Parakeet_Full_Get_Segment_Text_From_State { get; }
    parakeet_full_n_tokens_from_state Parakeet_Full_N_Tokens_From_State { get; }
    parakeet_full_get_token_data_from_state Parakeet_Full_Get_Token_Data_From_State { get; }
    parakeet_full_get_token_text_from_state Parakeet_Full_Get_Token_Text_From_State { get; }
    parakeet_full_get_token_p_from_state Parakeet_Full_Get_Token_P_From_State { get; }
    parakeet_print_system_info Parakeet_Print_System_Info { get; }
}
