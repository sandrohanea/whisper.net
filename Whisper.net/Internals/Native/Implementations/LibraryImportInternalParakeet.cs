// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
using Whisper.net.Native;

namespace Whisper.net.Internals.Native.Implementations;

internal partial class LibraryImportInternalParakeet : INativeParakeet
{
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial IntPtr parakeet_init_with_params_no_state(ref WhisperModelLoader loader, ParakeetContextParams parameters);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial void parakeet_free(IntPtr context);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial IntPtr parakeet_init_state(IntPtr context);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial void parakeet_free_state(IntPtr state);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial IntPtr parakeet_full_default_params_by_ref(ParakeetSamplingStrategy strategy);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial void parakeet_free_params(IntPtr parameters);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial int parakeet_full_with_state(IntPtr context, IntPtr state, ParakeetFullParams parameters, IntPtr samples, int sampleCount);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial int parakeet_full_n_segments_from_state(IntPtr state);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial long parakeet_full_get_segment_t0_from_state(IntPtr state, int segmentIndex);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial long parakeet_full_get_segment_t1_from_state(IntPtr state, int segmentIndex);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial IntPtr parakeet_full_get_segment_text_from_state(IntPtr state, int segmentIndex);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial int parakeet_full_n_tokens_from_state(IntPtr state, int segmentIndex);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial ParakeetTokenData parakeet_full_get_token_data_from_state(IntPtr state, int segmentIndex, int tokenIndex);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial IntPtr parakeet_full_get_token_text_from_state(IntPtr context, IntPtr state, int segmentIndex, int tokenIndex);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial float parakeet_full_get_token_p_from_state(IntPtr state, int segmentIndex, int tokenIndex);
    [LibraryImport(NativeConstants.InternalLibraryName)]
    private static partial IntPtr parakeet_print_system_info();

    public INativeParakeet.parakeet_init_with_params_no_state Parakeet_Init_With_Params_No_State => parakeet_init_with_params_no_state;
    public INativeParakeet.parakeet_free Parakeet_Free => parakeet_free;
    public INativeParakeet.parakeet_init_state Parakeet_Init_State => parakeet_init_state;
    public INativeParakeet.parakeet_free_state Parakeet_Free_State => parakeet_free_state;
    public INativeParakeet.parakeet_full_default_params_by_ref Parakeet_Full_Default_Params_By_Ref => parakeet_full_default_params_by_ref;
    public INativeParakeet.parakeet_free_params Parakeet_Free_Params => parakeet_free_params;
    public INativeParakeet.parakeet_full_with_state Parakeet_Full_With_State => parakeet_full_with_state;
    public INativeParakeet.parakeet_full_n_segments_from_state Parakeet_Full_N_Segments_From_State => parakeet_full_n_segments_from_state;
    public INativeParakeet.parakeet_full_get_segment_t0_from_state Parakeet_Full_Get_Segment_T0_From_State => parakeet_full_get_segment_t0_from_state;
    public INativeParakeet.parakeet_full_get_segment_t1_from_state Parakeet_Full_Get_Segment_T1_From_State => parakeet_full_get_segment_t1_from_state;
    public INativeParakeet.parakeet_full_get_segment_text_from_state Parakeet_Full_Get_Segment_Text_From_State => parakeet_full_get_segment_text_from_state;
    public INativeParakeet.parakeet_full_n_tokens_from_state Parakeet_Full_N_Tokens_From_State => parakeet_full_n_tokens_from_state;
    public INativeParakeet.parakeet_full_get_token_data_from_state Parakeet_Full_Get_Token_Data_From_State => parakeet_full_get_token_data_from_state;
    public INativeParakeet.parakeet_full_get_token_text_from_state Parakeet_Full_Get_Token_Text_From_State => parakeet_full_get_token_text_from_state;
    public INativeParakeet.parakeet_full_get_token_p_from_state Parakeet_Full_Get_Token_P_From_State => parakeet_full_get_token_p_from_state;
    public INativeParakeet.parakeet_print_system_info Parakeet_Print_System_Info => parakeet_print_system_info;

    public void Dispose()
    {
    }
}
#endif
