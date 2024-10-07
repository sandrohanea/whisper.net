// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if !IOS && !MACCATALYST && !TVOS && !ANDROID
using System.Runtime.InteropServices;
using Whisper.net.Native;
#endif

namespace Whisper.net.LibraryLoader;

public static class NativeLibraryLoader
{
    internal static LoadResult LoadNativeLibrary(bool bypassLoading, List<RuntimeLibrary> runtimeLibraries)
    {
#if IOS || MACCATALYST || TVOS || ANDROID
        return LoadResult.Success;
#else
        // If the user has handled loading the library themselves, we don't need to do anything.
        if (bypassLoading || RuntimeInformation.OSArchitecture.ToString().Equals("wasm", StringComparison.OrdinalIgnoreCase))
        {
            return LoadResult.Success;
        }

        return LoadLibraryComponent(runtimeLibraries);
    }

    private static LoadResult LoadLibraryComponent(List<RuntimeLibrary> runtimeLibraries)
    {
        var platform = Environment.OSVersion.Platform switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux",
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "macos",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

        ILibraryLoader libraryLoader = platform switch
        {
            "win" => new WindowsLibraryLoader(),
            "macos" => new MacOsLibraryLoader(),
            "linux" => new LinuxLibraryLoader(),
            _ => throw new PlatformNotSupportedException($"Currently {platform} platform is not supported")
        };

        LoadResult? lastError = null;

        foreach (var runtimePath in GetRuntimePaths(platform, runtimeLibraries))
        {
            var ggmlPath = GetLibraryPath(platform, "ggml", runtimePath);
            if (!File.Exists(ggmlPath))
            {
                continue;
            }

            var ggmlLoadResult = libraryLoader.OpenLibrary(ggmlPath);

            // Maybe GPU is not available but we still have other runtime installed
            if (!ggmlLoadResult.IsSuccess)
            {
                lastError = ggmlLoadResult;
                continue;
            }

            // Ggml was loaded, for this runtimePath, we need to load whisper as well
            var whisperPath = GetLibraryPath(platform, "whisper", runtimePath);
            return libraryLoader.OpenLibrary(whisperPath);

        }

        // We don't have any error, so we couldn't even find some library to load.
        if (lastError == null)
        {
            throw new FileNotFoundException($"Native Library not found in default paths. " +
                $"Verify you have have included the native Whisper library in your application, " +
                $"or install the default libraries with the Whisper.net.Runtime NuGet.");
        }

        // Runtime was found but couldn't be loaded.
        return lastError;
    }

    private static string GetLibraryPath(string platform, string libraryName, string runtimePath)
    {
        var libraryFileName = platform switch
        {
            "win" => $"{libraryName}.dll",
            "macos" => $"lib{libraryName}.dylib",
            "linux" => $"lib{libraryName}.so",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };
        return Path.Combine(runtimePath, libraryFileName);
    }

    private static IEnumerable<string> GetRuntimePaths(string platform, List<RuntimeLibrary> runtimeLibraries)
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
                Path.GetDirectoryName(typeof(NativeMethods).Assembly.Location),
                Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
            }.Where(it => !string.IsNullOrEmpty(it)).FirstOrDefault();

        var runtimesPath = string.IsNullOrEmpty(assemblySearchPath)
             ? "runtimes"
             : Path.Combine(assemblySearchPath, "runtimes");

        foreach (var library in runtimeLibraries)
        {
            var runtimePath = library switch
            {
                RuntimeLibrary.Cuda => Path.Combine(runtimesPath, "cuda", $"{platform}-{architecture}"),
                RuntimeLibrary.Vulkan => Path.Combine(runtimesPath, "vulkan", $"{platform}-{architecture}"),
                RuntimeLibrary.Cpu => Path.Combine(runtimesPath, $"{platform}-{architecture}"),
                _ => throw new InvalidOperationException("Unknown runtime library")
            };

            if (Directory.Exists(runtimePath))
            {
                yield return runtimePath;
            }
        }

#endif
    }

}
