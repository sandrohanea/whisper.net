// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Internals.Native;
internal interface INativeCuda : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int cudaGetDeviceCount(out int count);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int cudaRuntimeGetVersion(out int version);

    cudaGetDeviceCount CudaGetDeviceCount { get; }
    cudaRuntimeGetVersion CudaRuntimeGetVersion { get; }
}
