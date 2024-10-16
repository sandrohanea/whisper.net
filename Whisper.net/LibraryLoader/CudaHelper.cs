// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Internals.Native.Implementations.Cuda;

namespace Whisper.net.LibraryLoader;
internal static class CudaHelper
{
    public static bool IsCudaAvailable()
    {
        INativeCuda? nativeCuda = null;
        try
        {
#if NET6_0_OR_GREATER
            var libName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                               ? DllImportNativeCuda_64_12.LibraryName // Only 64-bit Windows is supported for now
                               : DllImportNativeLibcuda.LibraryName;

            if (!NativeLibrary.TryLoad(libName, out var library))
            {
                return false;
            }
            nativeCuda = new NativeLibraryCuda(library);
#else
            try
            {
                nativeCuda = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                ? new DllImportNativeCuda_64_12()
                                : new DllImportNativeLibcuda();
            }
            catch
            {
                return false;
            }
#endif
            nativeCuda.CudaGetDeviceCount(out var count);
            return count > 0;
        }
        finally
        {
            nativeCuda?.Dispose();
        }
    }
}
