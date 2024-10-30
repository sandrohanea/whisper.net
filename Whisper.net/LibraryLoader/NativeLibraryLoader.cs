// Licensed under the MIT license: https://opensource.org/licenses/MIT
using Whisper.net.Internals.Native.Implementations;
using Whisper.net.Logger;

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
        LogProvider.Log(WhisperLogLevel.Debug, "Using LibraryImportInternalWhisper for whisper librar for ios.");
        return LoadResult.Success(new LibraryImportInternalWhisper());
#elif ANDROID       
        LogProvider.Log(WhisperLogLevel.Debug, "Using LibraryImportLibWhisper for whisper librar for Android.");
        return LoadResult.Success(new LibraryImportLibWhisper());
#else
        // If the user has handled loading the library themselves, we don't need to do anything.
        if (RuntimeOptions.Instance.BypassLoading
            || RuntimeInformation.OSArchitecture.ToString().Equals("wasm", StringComparison.OrdinalIgnoreCase))
        {
#if NET8_0_OR_GREATER
            LogProvider.Log(WhisperLogLevel.Debug, "Using LibraryImportLibWhisper for whisper library with bypassed loading.");
            return LoadResult.Success(new LibraryImportLibWhisper());
#else
            LogProvider.Log(WhisperLogLevel.Debug, "Using DllImportsNativeLibWhisper for whisper library with bypassed loading.");
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
            "macos" or "linux" => new LibdlLibraryLoader(),
            _ => throw new PlatformNotSupportedException($"Currently {platform} platform is not supported")
        };
#else
        var libraryLoader = new UniversalLibraryLoader();
#endif

        string? lastError = null;

        var availableRuntimes = GetRuntimePaths(platform).ToList();
        var availableRuntimeTypes = availableRuntimes.Select(x => x.RuntimeLibrary).ToList();

        foreach (var (runtimePath, runtimeLibrary) in availableRuntimes)
        {
            if (!IsRuntimeSupported(runtimeLibrary, platform, availableRuntimeTypes))
            {
                continue;
            }

            var ggmlPath = GetLibraryPath(platform, "ggml", runtimePath);
            if (!File.Exists(ggmlPath))
            {
                continue;
            }

            var whisperPath = GetLibraryPath(platform, "whisper", runtimePath);

            LogProvider.Log(WhisperLogLevel.Debug, $"Trying to load ggml library from {ggmlPath}");
            if (!libraryLoader.TryOpenLibrary(ggmlPath, out var ggmlLibraryHandle))
            {
                lastError = libraryLoader.GetLastError();
                LogProvider.Log(WhisperLogLevel.Debug, $"Failed to load ggml library from {ggmlPath}. Error: {lastError}");
                continue;
            }

            LogProvider.Log(WhisperLogLevel.Debug, $"Trying to load whisper library from {whisperPath}");
            // Ggml was loaded, for this runtimePath, we need to load whisper as well
            if (!libraryLoader.TryOpenLibrary(whisperPath, out var whisperHandle))
            {
                lastError = libraryLoader.GetLastError();
                LogProvider.Log(WhisperLogLevel.Debug, $"Failed to load whisper library from {whisperPath}. Error: {lastError}");
                continue;
            }

            LogProvider.Log(WhisperLogLevel.Debug, $"Successfully loaded whisper library from {whisperPath}");
            RuntimeOptions.Instance.SetLoadedLibrary(runtimeLibrary);
#if NETSTANDARD
            LogProvider.Log(WhisperLogLevel.Debug, $"Using DllImportsNativeWhisper for whisper library");
            var nativeWhisper = new DllImportsNativeWhisper();
#else
            LogProvider.Log(WhisperLogLevel.Debug, $"Using NativeLibraryWhisper for whisper library");
            var nativeWhisper = new NativeLibraryWhisper(whisperHandle, ggmlLibraryHandle);
#endif

            return LoadResult.Success(nativeWhisper);
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

    private static bool IsRuntimeSupported(RuntimeLibrary runtime, string platform, List<RuntimeLibrary> runtimeLibraries)
    {
        LogProvider.Log(WhisperLogLevel.Debug, $"Checking if runtime {runtime} is supported on the platform: {platform}");
#if !NETSTANDARD
        // If AVX is not supported, we can't use CPU runtime on Windows and linux (we should use noavx runtime instead).
        if (runtime == RuntimeLibrary.Cpu && (platform == "win" || platform == "linux") && !Avx.IsSupported && !Avx2.IsSupported)
        {
            LogProvider.Log(WhisperLogLevel.Debug, $"No AVX or AVX2 support is identified on this host.");
            // If noavx runtime is not available, we should throw an exception, because we can't use CPU runtime without AVX support.
            if (!runtimeLibraries.Contains(RuntimeLibrary.CpuNoAvx))
            {
                throw new PlatformNotSupportedException("AVX is not supported on this platform, and noavx runtime is not available." +
                    " Install Whisper.net.Runtime.NoAvx for support on this platform.");
            }
            return false;
        }
#endif
        // If Cuda is not available, we can't use Cuda runtime (unless there is no other runtime available, where CUDA runtime can be used as a fallback to the CPU)
        if (runtime == RuntimeLibrary.Cuda && !CudaHelper.IsCudaAvailable())
        {
            var cudaIndex = runtimeLibraries.IndexOf(RuntimeLibrary.Cuda);

            if (cudaIndex == RuntimeOptions.Instance.RuntimeLibraryOrder.Count - 1)
            {
                // We still can use Cuda as a fallback to the CPU if it's the last runtime in the list.
                // This scenario can be used to not install 2 runtimes (CPU and Cuda) on the same host,
                // + override the default RuntimeLibraryOrder to have only [ Cuda ].
                // This way, the user can use Cuda if it's available, otherwise, the CPU runtime will be used.
                // However, the cudart library should be available in the system.
                LogProvider.Log(WhisperLogLevel.Debug, "Cuda runtime is not available, but it's the last runtime in the list. " +
                    "It will be used as a fallback to the CPU runtime.");
                return true;
            }

            LogProvider.Log(WhisperLogLevel.Debug, "Cuda driver is not available or no cuda device is identified.");
            return false;
        }

        return true;

    }

    private static IEnumerable<(string RuntimePath, RuntimeLibrary RuntimeLibrary)> GetRuntimePaths(string platform)
    {
        var architecture = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

        var assemblyLocation = typeof(NativeLibraryLoader).Assembly.Location;
        var environmentAppStartLocation = Environment.GetCommandLineArgs()[0];
        // NetFramework and Mono will crash if we try to get the directory of an empty string.
        var assemblySearchPaths = new[]
            {
                AppDomain.CurrentDomain.RelativeSearchPath,
                AppDomain.CurrentDomain.BaseDirectory,
                string.IsNullOrWhiteSpace(assemblyLocation) ? null : Path.GetDirectoryName(assemblyLocation),
                string.IsNullOrWhiteSpace(environmentAppStartLocation) ? null : Path.GetDirectoryName(environmentAppStartLocation)
            }.Where(it => !string.IsNullOrEmpty(it)).Distinct();

        foreach (var library in RuntimeOptions.Instance.RuntimeLibraryOrder)
        {
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
                LogProvider.Log(WhisperLogLevel.Debug, $"Searching for runtime directory {library} in {runtimePath}");

                if (Directory.Exists(runtimePath))
                {
                    yield return (runtimePath, library);
                }
                else
                {
                    LogProvider.Log(WhisperLogLevel.Debug, $"Runtime directory for {library} not found in {runtimePath}");
                }
            }

        }

#endif
    }
}
