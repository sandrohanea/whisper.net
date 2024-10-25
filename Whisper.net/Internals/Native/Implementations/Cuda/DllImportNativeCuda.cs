// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Internals.Native.Implementations.Cuda;

internal class DllImportNativeCuda_64_12 : INativeCuda
{
    public const string LibraryName = "cudart64_12";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;

    public void Dispose()
    {
    }
}

internal class DllImportNativeLibcuda : INativeCuda
{
    public const string LibraryName = "libcudart";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;

    public void Dispose()
    {
    }
}
