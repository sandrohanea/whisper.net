// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Internals.Native.Implementations.Cuda;

internal class DllImportNativeCuda_64_13 : INativeCuda
{
    public const string LibraryName = "cudart64_13";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaRuntimeGetVersion(out int version);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;
    public INativeCuda.cudaRuntimeGetVersion CudaRuntimeGetVersion => cudaRuntimeGetVersion;

    public void Dispose()
    {
    }
}

internal class DllImportNativeCuda_64_12 : INativeCuda
{
    public const string LibraryName = "cudart64_12";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaRuntimeGetVersion(out int version);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;
    public INativeCuda.cudaRuntimeGetVersion CudaRuntimeGetVersion => cudaRuntimeGetVersion;

    public void Dispose()
    {
    }
}

internal class DllImportNativeLibcuda : INativeCuda
{
    public const string LibraryName = "libcudart.so";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaRuntimeGetVersion(out int version);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;
    public INativeCuda.cudaRuntimeGetVersion CudaRuntimeGetVersion => cudaRuntimeGetVersion;

    public void Dispose()
    {
    }
}

internal class DllImportNativeLibcuda_13 : INativeCuda
{
    public const string LibraryName = "libcudart.so.13";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaRuntimeGetVersion(out int version);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;
    public INativeCuda.cudaRuntimeGetVersion CudaRuntimeGetVersion => cudaRuntimeGetVersion;

    public void Dispose()
    {
    }
}

internal class DllImportNativeLibcuda_12 : INativeCuda
{
    public const string LibraryName = "libcudart.so.12";

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaGetDeviceCount(out int count);

    [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl)]
    private static extern int cudaRuntimeGetVersion(out int version);

    public INativeCuda.cudaGetDeviceCount CudaGetDeviceCount => cudaGetDeviceCount;
    public INativeCuda.cudaRuntimeGetVersion CudaRuntimeGetVersion => cudaRuntimeGetVersion;

    public void Dispose()
    {
    }
}
