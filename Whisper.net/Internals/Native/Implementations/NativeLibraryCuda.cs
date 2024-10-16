// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if NET6_0_OR_GREATER
using static Whisper.net.Internals.Native.INativeWhisper;
using System.Runtime.InteropServices;
using static Whisper.net.Internals.Native.INativeCuda;

namespace Whisper.net.Internals.Native.Implementations;
internal class NativeLibraryCuda : INativeCuda
{
    private readonly nint cudaHandle;

    public cudaGetDeviceCount CudaGetDeviceCount { get; }
    public NativeLibraryCuda(IntPtr cudaHandle)
    {
        CudaGetDeviceCount = Marshal.GetDelegateForFunctionPointer<cudaGetDeviceCount>(NativeLibrary.GetExport(cudaHandle, nameof(whisper_init_from_file_with_params_no_state)));
        this.cudaHandle = cudaHandle;
    }

    public void Dispose()
    {
        NativeLibrary.Free(cudaHandle);
    }
}
#endif
