// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Internals.Native.Implementations.Cuda;
using Whisper.net.Logger;

namespace Whisper.net.LibraryLoader;
internal static class CudaHelper
{
    public static bool IsCudaAvailable()
    {
        WhisperLogger.Log(WhisperLogLevel.Debug, "Checking for CUDA availability.");
        INativeCuda? nativeCuda = null;
        var cudaDevices = 0;
        try
        {
#if NET8_0_OR_GREATER
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                foreach (var libraryName in new[]
                         {
                             DllImportNativeCuda_64_13.LibraryName,
                             DllImportNativeCuda_64_12.LibraryName
                         })
                {
                    WhisperLogger.Log(WhisperLogLevel.Debug, $"Trying to load CUDA runtime library: {libraryName}.");
                    if (!NativeLibrary.TryLoad(libraryName, out var libraryHandle))
                    {
                        WhisperLogger.Log(WhisperLogLevel.Debug,
                            $"CUDA runtime library {libraryName} couldn't be loaded.");
                        continue;
                    }

                    nativeCuda = new NativeLibraryCuda(libraryHandle);
                    nativeCuda.CudaGetDeviceCount(out cudaDevices);
                    break;
                }

                if (nativeCuda is null)
                {
                    WhisperLogger.Log(WhisperLogLevel.Debug, "Cudart library couldn't be loaded.");
                    return false;
                }
            }
            else
            {
                if (!NativeLibrary.TryLoad(DllImportNativeLibcuda.LibraryName, out var library))
                {
                    WhisperLogger.Log(WhisperLogLevel.Debug, "Cudart library couldn't be loaded.");
                    return false;
                }

                nativeCuda = new NativeLibraryCuda(library);
                nativeCuda.CudaGetDeviceCount(out cudaDevices);
            }
#else
            try
            {
                nativeCuda = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                ? TryCreateWindowsCuda()
                                : new DllImportNativeLibcuda();

                if (nativeCuda is null)
                {
                    WhisperLogger.Log(WhisperLogLevel.Debug, "Cudart library couldn't be loaded.");
                    return false;
                }
                nativeCuda.CudaGetDeviceCount(out cudaDevices);
            }
            catch
            {
                WhisperLogger.Log(WhisperLogLevel.Debug, "Cudart library couldn't be loaded.");
                return false;
            }
#endif
            WhisperLogger.Log(WhisperLogLevel.Debug, $"Number of CUDA devices found: {cudaDevices}");
            return cudaDevices > 0;
        }
        finally
        {
            nativeCuda?.Dispose();
        }
    }

#if !NET8_0_OR_GREATER
    private static INativeCuda? TryCreateWindowsCuda()
    {
        foreach (var factory in new Func<INativeCuda>[]
                 {
                     () => new DllImportNativeCuda_64_13(),
                     () => new DllImportNativeCuda_64_12()
                 })
        {
            try
            {
                var instance = factory();
                WhisperLogger.Log(WhisperLogLevel.Debug,
                    $"Successfully created CUDA runtime binding: {instance.GetType().Name}.");
                return instance;
            }
            catch
            {
                // ignored - we'll try the next available runtime
            }
        }

        return null;
    }
#endif
}
