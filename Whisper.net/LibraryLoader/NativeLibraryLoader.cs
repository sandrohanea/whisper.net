// Licensed under the MIT license: https://opensource.org/licenses/MIT
using Whisper.net.Internals.Native.Implementations;
#if !IOS && !MACCATALYST && !TVOS && !ANDROID
#if !NETSTANDARD
using System.Runtime.Intrinsics.X86;
#endif
using System.Runtime.InteropServices;
#endif

namespace Whisper.net.LibraryLoader;

public static class NativeLibraryLoader
{
    internal static LoadResult LoadNativeLibrary()
    {
#if IOS || MACCATALYST || TVOS
        return LoadResult.Success(new LibraryImportInternalWhisper());
#elif ANDROID       
        return LoadResult.Success(new LibraryImportLibWhisper());
#else
        // If the user has handled loading the library themselves, we don't need to do anything.
        if (RuntimeOptions.Instance.BypassLoading
            || RuntimeInformation.OSArchitecture.ToString().Equals("wasm", StringComparison.OrdinalIgnoreCase))
        {
#if NET8_0_OR_GREATER
            return LoadResult.Success(new LibraryImportLibWhisper());
#else
            return LoadResult.Success(new DllImportsNativeLibWhisper());
#endif
        }
        return LoadLibraryComponent();
    }

    private static LoadResult LoadLibraryComponent()
    {
        var platform = Environment.OSVersion.Platform switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "macos",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

#if NETSTANDARD
        ILibraryLoader libraryLoader = platform switch
        {
            "win" => new WindowsLibraryLoader(),
            "macos" => new MacOsLibraryLoader(),
            "linux" => new LinuxLibraryLoader(),
            _ => throw new PlatformNotSupportedException($"Currently {platform} platform is not supported")
        };
#else
        var libraryLoader = new UniversalLibraryLoader();
#endif

        string? lastError = null;

        foreach (var (runtimePath, runtimeLibrary) in GetRuntimePaths(platform))
        {
            var ggmlPath = GetLibraryPath(platform, "ggml", runtimePath);
            if (!File.Exists(ggmlPath))
            {
                continue;
            }
            var whisperPath = GetLibraryPath(platform, "whisper", runtimePath);

            var ggmlLibraryHandle = libraryLoader.OpenLibrary(ggmlPath, global: true);
            // Maybe GPU is not available but we still have other runtime installed
            if (ggmlLibraryHandle == IntPtr.Zero)
            {
                lastError = libraryLoader.GetLastError();
                continue;
            }

            // Ggml was loaded, for this runtimePath, we need to load whisper as well
            var whisperHandle = libraryLoader.OpenLibrary(whisperPath, global: true);
            if (whisperHandle != IntPtr.Zero)
            {
                RuntimeOptions.Instance.SetLoadedLibrary(runtimeLibrary);
#if NETSTANDARD
                var nativeWhisper = new DllImportsNativeWhisper();
#else
                var nativeWhisper = new NativeLibraryWhisper(whisperHandle, ggmlLibraryHandle);
#endif

                return LoadResult.Success(nativeWhisper);
            }
        }

        // We don't have any error, so we couldn't even find some library to load.
        if (lastError == null)
        {
            throw new FileNotFoundException($"Native Library not found in default paths." +
                $"Verify you have have included the native Whisper library in your application, " +
                $"or install the default libraries with the Whisper.net.Runtime NuGet.");
        }

        // Runtime was found but couldn't be loaded.
        return LoadResult.Failure(lastError);
    }

    private static string GetLibraryPath(string platform, string libraryName, string runtimePath)
    {
        var libraryFileName = platform switch
        {
            "win" => $"{libraryName}.dll",
            "macos" => $"lib{libraryName}.dylib",
            "linux" => $"lib{libraryName}.so",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform: {platform}")
        };
        return Path.Combine(runtimePath, libraryFileName);
    }

    private static IEnumerable<(string, RuntimeLibrary)> GetRuntimePaths(string platform)
    {
        var architecture = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

        var assemblySearchPaths = new[]
            {
                AppDomain.CurrentDomain.RelativeSearchPath,
                AppDomain.CurrentDomain.BaseDirectory,
                Path.GetDirectoryName(typeof(NativeLibraryLoader).Assembly.Location),
                Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
            }.Where(it => !string.IsNullOrEmpty(it));

        static bool IsRuntimeSupported(RuntimeLibrary runtime, string platform)
        {

#if !NETSTANDARD
            // If AVX is not supported, we can't use CPU runtime on windows and linux (we should use noavx runtime instead).
            if (runtime == RuntimeLibrary.Cpu && (platform == "win" || platform == "linux") && !Avx.IsSupported && !Avx2.IsSupported)
            {
                return false;
            }
#endif
            // If Cuda is not available, we can't use Cuda runtime (unless there is no other runtime available, where CUDA runtime can be used as a fallback to the CPU)
            if (runtime == RuntimeLibrary.Cuda && !CudaHelper.IsCudaAvailable())
            {
                var cudaIndex = RuntimeOptions.Instance.RuntimeLibraryOrder.IndexOf(RuntimeLibrary.Cuda);

                if (cudaIndex == RuntimeOptions.Instance.RuntimeLibraryOrder.Count - 1)
                {
                    // We still can use Cuda as a fallback to the CPU if it's the last runtime in the list.

                    // This scenario can be used to not install 2 runtimes (CPU and Cuda) on the same host,
                    // + override the default RuntimeLibraryOrder to have only [ Cuda ].
                    return true;
                }

                return false;
            }

            return true;

        }

        foreach (var library in RuntimeOptions.Instance.RuntimeLibraryOrder)
        {
            if (!IsRuntimeSupported(library, platform))
            {
                continue;
            }
            foreach (var assemblySearchPath in assemblySearchPaths)
            {

                var runtimesPath = string.IsNullOrEmpty(assemblySearchPath)
                     ? "runtimes"
                     : Path.Combine(assemblySearchPath, "runtimes");
                var runtimePath = library switch
                {
                    RuntimeLibrary.Cuda => Path.Combine(runtimesPath, "cuda", $"{platform}-{architecture}"),
                    RuntimeLibrary.Vulkan => Path.Combine(runtimesPath, "vulkan", $"{platform}-{architecture}"),
                    RuntimeLibrary.Cpu => Path.Combine(runtimesPath, $"{platform}-{architecture}"),
                    RuntimeLibrary.CpuNoAvx => Path.Combine(runtimesPath, "noavx", $"{platform}-{architecture}"),
                    RuntimeLibrary.CoreML => Path.Combine(runtimesPath, "coreml", $"{platform}-{architecture}"),
                    RuntimeLibrary.OpenVino => Path.Combine(runtimesPath, "openvino", $"{platform}-{architecture}"),
                    _ => throw new InvalidOperationException("Unknown runtime library")
                };

                if (Directory.Exists(runtimePath))
                {
                    yield return (runtimePath, library);
                }
            }

        }

#endif
    }
}
