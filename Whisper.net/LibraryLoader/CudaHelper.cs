// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Internals.Native.Implementations.Cuda;
using Whisper.net.Logger;

namespace Whisper.net.LibraryLoader;
internal static class CudaHelper
{
    public static bool IsCudaAvailable()
    {
        LogProvider.Log(WhisperLogLevel.Debug, "Checking for CUDA availability.");
        INativeCuda? nativeCuda = null;
        var cudaDevices = 0;
        try
        {
#if NET6_0_OR_GREATER
            var libName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                               ? DllImportNativeCuda_64_12.LibraryName // Only 64-bit Windows is supported for now
                               : DllImportNativeLibcuda.LibraryName;

            if (!NativeLibrary.TryLoad(libName, out var library))
            {
                LogProvider.Log(WhisperLogLevel.Debug, "Cudart library couldn't be loaded.");
                return false;
            }
            nativeCuda = new NativeLibraryCuda(library);
            nativeCuda.CudaGetDeviceCount(out cudaDevices);
#else
            try
            {
                nativeCuda = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                ? new DllImportNativeCuda_64_12()
                                : new DllImportNativeLibcuda();
                nativeCuda.CudaGetDeviceCount(out cudaDevices);
            }
            catch
            {
                LogProvider.Log(WhisperLogLevel.Debug, "Cudart library couldn't be loaded.");
                return false;
            }
#endif
            LogProvider.Log(WhisperLogLevel.Debug, $"NUmber of CUDA devices found: {cudaDevices}");
            return cudaDevices > 0;
        }
        finally
        {
            nativeCuda?.Dispose();
        }
    }
}
