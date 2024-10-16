// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
using static Whisper.net.Internals.Native.INativeCuda;

namespace Whisper.net.Internals.Native.Implementations.Cuda;
internal class NativeLibraryCuda(IntPtr cudaHandle) : INativeCuda
{

    public cudaGetDeviceCount CudaGetDeviceCount { get; } = Marshal.GetDelegateForFunctionPointer<cudaGetDeviceCount>(NativeLibrary.GetExport(cudaHandle, nameof(cudaGetDeviceCount)));

    public void Dispose()
    {
        NativeLibrary.Free(cudaHandle);
    }
}
#endif
