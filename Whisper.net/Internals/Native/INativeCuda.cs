// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Internals.Native;
internal interface INativeCuda : IDisposable
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int cudaGetDeviceCount(out int count);

    cudaGetDeviceCount CudaGetDeviceCount { get; }

}
