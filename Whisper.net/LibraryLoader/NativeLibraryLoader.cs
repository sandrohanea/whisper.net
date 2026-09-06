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
    internal static ParakeetLoadResult LoadParakeetLibrary()
    {
#if IOS || MACCATALYST || TVOS
        WhisperLogger.Log(WhisperLogLevel.Debug, "Using the statically linked Parakeet library.");
        return ParakeetLoadResult.Success(new LibraryImportInternalParakeet());
#elif ANDROID
        WhisperLogger.Log(WhisperLogLevel.Debug, "Using libparakeet for Android.");
        return ParakeetLoadResult.Success(new LibraryImportLibParakeet());
#else
        if (RuntimeOptions.LoadedParakeetLibrary.HasValue
            || RuntimeInformation.OSArchitecture.ToString().Equals("wasm", StringComparison.OrdinalIgnoreCase))
        {
#if NET8_0_OR_GREATER
            return ParakeetLoadResult.Success(new LibraryImportLibParakeet());
#else
            return ParakeetLoadResult.Success(new DllImportsNativeParakeet());
#endif
        }

        return LoadParakeetLibraryComponent();
#endif
    }

    internal static LoadResult LoadNativeLibrary()
    {
#if IOS || MACCATALYST || TVOS
        WhisperLogger.Log(WhisperLogLevel.Debug, "Using LibraryImportInternalWhisper for whisper librar for ios.");
        return LoadResult.Success(new LibraryImportInternalWhisper());
#elif ANDROID
        WhisperLogger.Log(WhisperLogLevel.Debug, "Using LibraryImportLibWhisper for whisper librar for Android.");
        return LoadResult.Success(new LibraryImportLibWhisper());
#else
        // If the user has handled loading the library themselves, we don't need to do anything.
        if (RuntimeOptions.LoadedLibrary.HasValue
            || RuntimeInformation.OSArchitecture.ToString().Equals("wasm", StringComparison.OrdinalIgnoreCase))
        {
#if NET8_0_OR_GREATER
            WhisperLogger.Log(WhisperLogLevel.Debug, "Using LibraryImportLibWhisper for whisper library with bypassed loading.");
            return LoadResult.Success(new LibraryImportLibWhisper());
#else
            WhisperLogger.Log(WhisperLogLevel.Debug,
                "Using DllImportsNativeLibWhisper for whisper library with bypassed loading.");
            return LoadResult.Success(new DllImportsNativeLibWhisper());
#endif
        }

        return LoadLibraryComponent();
    }

    private static readonly string[] dependencyOrder =
    [
        "ggml-base-whisper",
        "ggml-cpu-whisper",
        "ggml-blas-whisper",
        "ggml-metal-whisper",
        "ggml-cuda-whisper",
        "ggml-vulkan-whisper",
        "ggml-whisper",
        "whisper.coreml"
    ];

    private static readonly string[] parakeetDependencyOrder =
    [
        "ggml-base-parakeet",
        "ggml-cpu-parakeet",
        "ggml-blas-parakeet",
        "ggml-metal-parakeet",
        "ggml-cuda-parakeet",
        "ggml-vulkan-parakeet",
        "ggml-parakeet"
    ];

    private static LoadResult LoadLibraryComponent()
    {
        var platform = Environment.OSVersion.Platform switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "macos",
            _ => throw new PlatformNotSupportedException($"Unsupported OS Version")
        };

        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported process architecture: {RuntimeInformation.ProcessArchitecture}")
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

        var runtimeSelection = RuntimeLibrarySelector.Select(
            RuntimeOptions.ForcedRuntimeLibrary,
            RuntimeOptions.RuntimeLibraryOrder);

        if (runtimeSelection.BypassCompatibilityChecks)
        {
            WhisperLogger.Log(WhisperLogLevel.Warning,
                $"Runtime {runtimeSelection.RuntimeLibraryOrder[0]} was forced. Automatic runtime compatibility checks will be bypassed.");
        }

        var availableRuntimes = GetRuntimePaths(architecture, platform, runtimeSelection.RuntimeLibraryOrder).ToList();
        var availableRuntimeTypes = availableRuntimes.Select(x => x.RuntimeLibrary).ToList();

        foreach (var (runtimePath, runtimeLibrary) in availableRuntimes)
        {
            if (!runtimeSelection.BypassCompatibilityChecks
                && !IsRuntimeSupported(runtimeLibrary, platform, architecture, availableRuntimeTypes, runtimeSelection.RuntimeLibraryOrder))
            {
                continue;
            }

            var whisperPath = GetLibraryPath(platform, "whisper", runtimePath);
            if (!File.Exists(whisperPath))
            {
                continue;
            }

            List<IntPtr> dependenciesHandles = [];
            lastError = null;
            // We need to use this special order to load the dependencies in the correct order.
            foreach (var dependency in dependencyOrder)
            {
                // We only try to load it if it's available in the directory
                var dependencyPath = GetLibraryPath(platform, dependency, runtimePath);
                if (File.Exists(dependencyPath))
                {
                    WhisperLogger.Log(WhisperLogLevel.Debug, $"Loading dependency at {dependencyPath}");
                    if (!libraryLoader.TryOpenLibrary(dependencyPath, out var dependencyHandle))
                    {
                        // We cannot open one of the dependencies, we need to close all the opened dependencies and return false.
                        lastError = libraryLoader.GetLastError();
                        WhisperLogger.Log(WhisperLogLevel.Debug,
                            $"Couldn't load dependency at {dependencyPath}. Received: {lastError}");
                        foreach (var handle in dependenciesHandles)
                        {
                            libraryLoader.CloseLibrary(handle);
                        }

                        dependenciesHandles.Clear();
                        continue;
                    }

                    dependenciesHandles.Add(dependencyHandle);
                }
            }

            WhisperLogger.Log(WhisperLogLevel.Debug, $"Trying to load whisper library from {whisperPath}");
            // Ggml was loaded, for this runtimePath, we need to load whisper as well
            if (!libraryLoader.TryOpenLibrary(whisperPath, out var whisperHandle))
            {
                lastError = libraryLoader.GetLastError();
                // We couldn't load the whisper library, we need to close all the dependencies and continue to the next runtime.
                foreach (var dependency in dependenciesHandles)
                {
                    libraryLoader.CloseLibrary(dependency);
                }

                WhisperLogger.Log(WhisperLogLevel.Debug,
                    $"Failed to load whisper library from {whisperPath}. Error: {lastError}");
                continue;
            }

            WhisperLogger.Log(WhisperLogLevel.Debug, $"Successfully loaded whisper library from {whisperPath}");
            RuntimeOptions.LoadedLibrary = runtimeLibrary;
#if NETSTANDARD
            WhisperLogger.Log(WhisperLogLevel.Debug, $"Using DllImportsNativeWhisper for whisper library");
            var nativeWhisper = new DllImportsNativeWhisper();
#else
            WhisperLogger.Log(WhisperLogLevel.Debug, $"Using NativeLibraryWhisper for whisper library");
            var nativeWhisper = new NativeLibraryWhisper(whisperHandle);
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

    private static ParakeetLoadResult LoadParakeetLibraryComponent()
    {
        var platform = Environment.OSVersion.Platform switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "macos",
            _ => throw new PlatformNotSupportedException("Unsupported OS version")
        };

        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported process architecture: {RuntimeInformation.ProcessArchitecture}")
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

        var runtimeSelection = RuntimeLibrarySelector.Select(
            RuntimeOptions.ForcedParakeetRuntimeLibrary,
            RuntimeOptions.ParakeetRuntimeLibraryOrder);
        if (runtimeSelection.RuntimeLibraryOrder.Any(
                runtime => runtime is RuntimeLibrary.CoreML or RuntimeLibrary.OpenVino))
        {
            throw new NotSupportedException("CoreML and OpenVINO are not supported by the Parakeet engine.");
        }

        string? lastError = null;
        var availableRuntimes = GetParakeetRuntimePaths(
            architecture,
            platform,
            runtimeSelection.RuntimeLibraryOrder).ToList();
        var availableRuntimeTypes = availableRuntimes.Select(x => x.RuntimeLibrary).ToList();

        foreach (var (runtimePath, runtimeLibrary) in availableRuntimes)
        {
            if (!runtimeSelection.BypassCompatibilityChecks
                && !IsRuntimeSupported(
                    runtimeLibrary,
                    platform,
                    architecture,
                    availableRuntimeTypes,
                    runtimeSelection.RuntimeLibraryOrder))
            {
                continue;
            }

            var parakeetPath = GetLibraryPath(platform, "parakeet", runtimePath);
            if (!File.Exists(parakeetPath))
            {
                continue;
            }

            List<IntPtr> dependencyHandles = [];
            lastError = null;
            var dependencyFailed = false;
            foreach (var dependency in parakeetDependencyOrder)
            {
                var dependencyPath = GetLibraryPath(platform, dependency, runtimePath);
                if (!File.Exists(dependencyPath))
                {
                    continue;
                }

                if (!libraryLoader.TryOpenLibrary(dependencyPath, out var dependencyHandle))
                {
                    lastError = libraryLoader.GetLastError();
                    dependencyFailed = true;
                    foreach (var handle in dependencyHandles)
                    {
                        libraryLoader.CloseLibrary(handle);
                    }
                    break;
                }

                dependencyHandles.Add(dependencyHandle);
            }

            if (dependencyFailed)
            {
                continue;
            }

            if (!libraryLoader.TryOpenLibrary(parakeetPath, out var parakeetHandle))
            {
                lastError = libraryLoader.GetLastError();
                foreach (var handle in dependencyHandles)
                {
                    libraryLoader.CloseLibrary(handle);
                }
                continue;
            }

            RuntimeOptions.LoadedParakeetLibrary = runtimeLibrary;
#if NETSTANDARD
            return ParakeetLoadResult.Success(new DllImportsNativeParakeet());
#else
            return ParakeetLoadResult.Success(new NativeLibraryParakeet(parakeetHandle));
#endif
        }

        if (lastError is null)
        {
            throw new FileNotFoundException(
                "Native Parakeet library not found in default paths. " +
                "Install the matching Whisper.net.Runtime.Parakeet NuGet package.");
        }

        return ParakeetLoadResult.Failure(lastError);
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

    private static bool IsRuntimeSupported(RuntimeLibrary runtime, string platform, string architecture,
        List<RuntimeLibrary> runtimeLibraries, IReadOnlyList<RuntimeLibrary> configuredRuntimeOrder)
    {
        WhisperLogger.Log(WhisperLogLevel.Debug,
            $"Checking if runtime {runtime} is supported on the platform: {platform}");
#if !NETSTANDARD
        // If AVX is not supported, we can't use CPU runtime on Windows and linux (we should use noavx runtime instead).
        if (runtime == RuntimeLibrary.Cpu
            && (platform == "win" || platform == "linux")
            && (architecture == "x86" || architecture == "x64")
            && (!Avx.IsSupported || !Avx2.IsSupported || !Fma.IsSupported))
        {
            WhisperLogger.Log(WhisperLogLevel.Debug, $"No AVX, AVX2 or Fma support is identified on this host. AVX: {Avx.IsSupported} AVX2: {Avx2.IsSupported} FMA: {Fma.IsSupported}");
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
        if (IsCudaRuntime(runtime) && !CudaHelper.IsCudaAvailable(runtime))
        {
            var cudaIndex = runtimeLibraries.IndexOf(runtime);

            if (cudaIndex == configuredRuntimeOrder.Count - 1)
            {
                // We still can use Cuda as a fallback to the CPU if it's the last runtime in the list.
                // This scenario can be used to not install 2 runtimes (CPU and Cuda) on the same host,
                // + override the default RuntimeLibraryOrder to have only [ Cuda ].
                // This way, the user can use Cuda if it's available, otherwise, the CPU runtime will be used.
                // However, the cudart library should be available in the system.
                WhisperLogger.Log(WhisperLogLevel.Debug,
                    $"Cuda runtime {runtime} is not available, but it's the last runtime in the list. " +
                    "It will be used as a fallback to the CPU runtime.");
                return true;
            }

            WhisperLogger.Log(WhisperLogLevel.Debug, $"Cuda driver for {runtime} is not available or no cuda device is identified.");
            return false;
        }

        return true;
    }

    private static IEnumerable<(string RuntimePath, RuntimeLibrary RuntimeLibrary)> GetParakeetRuntimePaths(
        string architecture,
        string platform,
        IReadOnlyList<RuntimeLibrary> runtimeLibraryOrder)
    {
        var assemblyLocation = typeof(NativeLibraryLoader).Assembly.Location;
        var assemblySearchPaths = new[]
        {
            GetSafeDirectoryName(RuntimeOptions.ParakeetLibraryPath),
            AppDomain.CurrentDomain.RelativeSearchPath,
            AppDomain.CurrentDomain.BaseDirectory,
            GetSafeDirectoryName(assemblyLocation),
            GetSafeDirectoryName(Environment.GetCommandLineArgs().FirstOrDefault()),
        }.Where(it => !string.IsNullOrEmpty(it)).Distinct();

        foreach (var library in runtimeLibraryOrder)
        {
            foreach (var assemblySearchPath in assemblySearchPaths)
            {
                var runtimesPath = string.IsNullOrEmpty(assemblySearchPath)
                    ? "runtimes"
                    : Path.Combine(assemblySearchPath, "runtimes");
                var runtimePath = RuntimePathResolver.GetRuntimePath(
                    runtimesPath,
                    WhisperModelFamily.Parakeet,
                    library,
                    platform,
                    architecture);
                if (Directory.Exists(runtimePath))
                {
                    yield return (runtimePath, library);
                }
            }
        }
    }

    private static IEnumerable<(string RuntimePath, RuntimeLibrary RuntimeLibrary)> GetRuntimePaths(string architecture,
        string platform, IReadOnlyList<RuntimeLibrary> runtimeLibraryOrder)
    {
        var assemblyLocation = typeof(NativeLibraryLoader).Assembly.Location;
        // NetFramework and Mono will crash if we try to get the directory of an empty string.
        var assemblySearchPaths = new[]
        {
            GetSafeDirectoryName(RuntimeOptions.LibraryPath), AppDomain.CurrentDomain.RelativeSearchPath,
            AppDomain.CurrentDomain.BaseDirectory, GetSafeDirectoryName(assemblyLocation),
            GetSafeDirectoryName(Environment.GetCommandLineArgs().FirstOrDefault()),
        }.Where(it => !string.IsNullOrEmpty(it)).Distinct();

        foreach (var library in runtimeLibraryOrder)
        {
            foreach (var assemblySearchPath in assemblySearchPaths)
            {
                var runtimesPath = string.IsNullOrEmpty(assemblySearchPath)
                    ? "runtimes"
                    : Path.Combine(assemblySearchPath, "runtimes");
                var runtimePath = RuntimePathResolver.GetRuntimePath(
                    runtimesPath,
                    WhisperModelFamily.Whisper,
                    library,
                    platform,
                    architecture);
                WhisperLogger.Log(WhisperLogLevel.Debug, $"Searching for runtime directory {library} in {runtimePath}");

                if (Directory.Exists(runtimePath))
                {
                    yield return (runtimePath, library);
                }
                else
                {
                    WhisperLogger.Log(WhisperLogLevel.Debug,
                        $"Runtime directory for {library} not found in {runtimePath}");
                }
            }
        }
    }

    private static string? GetSafeDirectoryName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        try
        {
            return Path.GetDirectoryName(path);
        }
        catch (Exception ex)
        {
            WhisperLogger.Log(WhisperLogLevel.Debug,
                $"Failed to get directory name from path: {path}. Error: {ex.Message}");
            return null;
        }
#endif
    }

    private static bool IsCudaRuntime(RuntimeLibrary runtime)
        => runtime == RuntimeLibrary.Cuda || runtime == RuntimeLibrary.Cuda12;
}
