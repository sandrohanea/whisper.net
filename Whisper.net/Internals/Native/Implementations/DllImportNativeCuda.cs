// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Internals.Native.Implementations;

internal class DllImportNativeCuda : INativeCuda
{
    const string libraryName = "cuda";

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;

    public void Dispose()
    {
    }
}
