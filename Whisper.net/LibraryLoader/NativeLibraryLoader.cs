// Licensed under the MIT license: https://opensource.org/licenses/MIT
using Whisper.net.Internals.Native.Implementations;

#if !IOS && !MACCATALYST && !TVOS && !ANDROID
using System.Runtime.InteropServices;
#endif

namespace Whisper.net.LibraryLoader;

public static class NativeLibraryLoader
{
    internal static LoadResult LoadNativeLibrary()
    {
#if IOS || MACCATALYST || TVOS || ANDROID
        return LoadResult.Success(new DllImportsInternalNativeWhisper());
#else
        // If the user has handled loading the library themselves, we don't need to do anything.
        if (RuntimeOptions.Instance.BypassLoading
            || RuntimeInformation.OSArchitecture.ToString().Equals("wasm", StringComparison.OrdinalIgnoreCase))
        {
            return LoadResult.Success(new DllImportsNativeWhisper());
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
            throw new FileNotFoundException($"Native Library not found in default paths. " +
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

        var assemblySearchPath = new[]
            {
                AppDomain.CurrentDomain.RelativeSearchPath,
                Path.GetDirectoryName(typeof(NativeLibraryLoader).Assembly.Location),
                Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
            }.Where(it => !string.IsNullOrEmpty(it)).FirstOrDefault();

        var runtimesPath = string.IsNullOrEmpty(assemblySearchPath)
             ? "runtimes"
             : Path.Combine(assemblySearchPath, "runtimes");

        foreach (var library in RuntimeOptions.Instance.RuntimeLibraryOrder)
        {
            var runtimePath = library switch
            {
                RuntimeLibrary.Cuda => Path.Combine(runtimesPath, "cuda", $"{platform}-{architecture}"),
                RuntimeLibrary.Vulkan => Path.Combine(runtimesPath, "vulkan", $"{platform}-{architecture}"),
                RuntimeLibrary.Cpu => Path.Combine(runtimesPath, $"{platform}-{architecture}"),
                RuntimeLibrary.CoreML => Path.Combine(runtimesPath, "coreml", $"{platform}-{architecture}"),
                RuntimeLibrary.OpenVino => Path.Combine(runtimesPath, "openvino", $"{platform}-{architecture}"),
                _ => throw new InvalidOperationException("Unknown runtime library")
            };

            if (Directory.Exists(runtimePath))
            {
                yield return (runtimePath, library);
            }
        }

#endif
            }

}
