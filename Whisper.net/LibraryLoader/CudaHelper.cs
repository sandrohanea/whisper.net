// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Internals.Native.Implementations.Cuda;
using Whisper.net.Logger;

namespace Whisper.net.LibraryLoader;
internal static class CudaHelper
{
    public static bool IsCudaAvailable(RuntimeLibrary runtime)
    {
        WhisperLogger.Log(WhisperLogLevel.Debug, $"Checking for CUDA availability for runtime {runtime}.");
        var expectedMajor = GetCudaMajorVersion(runtime);
        var cudaDevices = 0;
#if NET8_0_OR_GREATER
        foreach (var libraryName in GetCudaLibraryNames(runtime))
        {
            INativeCuda? nativeCuda = null;
            try
            {
                if (!NativeLibrary.TryLoad(libraryName, out var library))
                {
                    continue;
                }

                WhisperLogger.Log(WhisperLogLevel.Debug, $"Loaded cudart library '{libraryName}'.");
                nativeCuda = new NativeLibraryCuda(library);
                if (!IsRuntimeVersionSupported(nativeCuda, expectedMajor))
                {
                    continue;
                }

                if (HasCudaDevices(nativeCuda, runtime, out cudaDevices))
                {
                    return true;
                }
            }
            finally
            {
                nativeCuda?.Dispose();
            }
        }
#else
        foreach (var factory in GetNativeCudaFactories(runtime))
        {
            INativeCuda? nativeCuda = null;
            try
            {
                nativeCuda = factory();
                if (!IsRuntimeVersionSupported(nativeCuda, expectedMajor))
                {
                    continue;
                }

                if (HasCudaDevices(nativeCuda, runtime, out cudaDevices))
                {
                    return true;
                }
            }
            catch
            {
                // Failed to load this particular cudart variant, try the next one.
            }
            finally
            {
                nativeCuda?.Dispose();
            }
        }
#endif

        WhisperLogger.Log(WhisperLogLevel.Debug, $"CUDA runtime {runtime} is not available.");
        return false;
    }

#if !NET8_0_OR_GREATER
    private static IEnumerable<Func<INativeCuda>> GetNativeCudaFactories(RuntimeLibrary runtime)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (runtime == RuntimeLibrary.Cuda12)
            {
                yield return () => new DllImportNativeCuda_64_12();
            }
            else
            {
                yield return () => new DllImportNativeCuda_64_13();
            }
            yield break;
        }

        if (runtime == RuntimeLibrary.Cuda12)
        {
            yield return () => new DllImportNativeLibcuda_12();
        }
        else
        {
            yield return () => new DllImportNativeLibcuda_13();
        }

        yield return () => new DllImportNativeLibcuda();
    }
#endif

#if NET8_0_OR_GREATER
    private static IEnumerable<string> GetCudaLibraryNames(RuntimeLibrary runtime)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            yield return runtime == RuntimeLibrary.Cuda12
                ? DllImportNativeCuda_64_12.LibraryName
                : DllImportNativeCuda_64_13.LibraryName;
            yield break;
        }

        if (runtime == RuntimeLibrary.Cuda12)
        {
            yield return DllImportNativeLibcuda_12.LibraryName;
        }
        else
        {
            yield return DllImportNativeLibcuda_13.LibraryName;
        }

        yield return DllImportNativeLibcuda.LibraryName;
    }
#endif

    private static bool HasCudaDevices(INativeCuda nativeCuda, RuntimeLibrary runtime, out int cudaDevices)
    {
        cudaDevices = 0;
        var cudaResult = nativeCuda.CudaGetDeviceCount(out cudaDevices);
        if (cudaResult != 0)
        {
            WhisperLogger.Log(WhisperLogLevel.Debug,
                $"cudaGetDeviceCount returned error {cudaResult} for runtime {runtime}.");
            return false;
        }

        WhisperLogger.Log(WhisperLogLevel.Debug,
            $"Number of CUDA devices found for runtime {runtime}: {cudaDevices}");
        return cudaDevices > 0;
    }

    private static bool IsRuntimeVersionSupported(INativeCuda nativeCuda, int expectedMajor)
    {
        var status = nativeCuda.CudaRuntimeGetVersion(out var detectedVersion);
        if (status != 0)
        {
            WhisperLogger.Log(WhisperLogLevel.Debug,
                $"cudaRuntimeGetVersion returned error {status} while checking CUDA runtime.");
            return false;
        }

        var detectedMajor = detectedVersion / 1000;
        if (detectedMajor != expectedMajor)
        {
            WhisperLogger.Log(WhisperLogLevel.Debug,
                $"Detected CUDA runtime major version {detectedMajor}, expected {expectedMajor}.");
            return false;
        }

        WhisperLogger.Log(WhisperLogLevel.Debug,
            $"CUDA runtime major version {detectedMajor} matches expected {expectedMajor}.");
        return true;
    }

    private static int GetCudaMajorVersion(RuntimeLibrary runtime)
    {
        return runtime switch
        {
            RuntimeLibrary.Cuda => 13,
            RuntimeLibrary.Cuda12 => 12,
            _ => throw new ArgumentOutOfRangeException(nameof(runtime), runtime, "Unexpected CUDA runtime.")
        };
    }
}
