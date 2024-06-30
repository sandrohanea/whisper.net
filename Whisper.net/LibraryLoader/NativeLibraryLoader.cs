// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if !IOS && !MACCATALYST && !TVOS && !ANDROID
using System.Runtime.InteropServices;
using Whisper.net.Native;
#endif

namespace Whisper.net.LibraryLoader;

public static class NativeLibraryLoader
{
    internal static LoadResult LoadNativeLibrary(bool bypassLoading = false)
    {
#if IOS || MACCATALYST || TVOS || ANDROID
        return LoadResult.Success;
#else
        // If the user has handled loading the library themselves, we don't need to do anything.
        if (bypassLoading || RuntimeInformation.OSArchitecture.ToString().Equals("wasm", StringComparison.OrdinalIgnoreCase))
        {
            return LoadResult.Success;
        }

        var loadGgml = LoadLibraryComponent("ggml");
        if (!loadGgml.IsSuccess)
        {
            return loadGgml;
        }

        return LoadLibraryComponent("whisper");
    }

    private static LoadResult LoadLibraryComponent(string libraryName)
    {

        var architecture = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

        var (platform, dynamicLibraryName) = Environment.OSVersion.Platform switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => ("win", $"{libraryName}.dll"),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => ("linux", "libwhisper.so"),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => ("macos", "libwhisper.dylib"),
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

        var assemblySearchPath = new[]
            {
                AppDomain.CurrentDomain.RelativeSearchPath,
                Path.GetDirectoryName(typeof(NativeMethods).Assembly.Location),
                Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])
            }.Where(it => !string.IsNullOrEmpty(it)).FirstOrDefault();

        var path = string.IsNullOrEmpty(assemblySearchPath)
             ? Path.Combine("runtimes", $"{platform}-{architecture}", dynamicLibraryName)
             : Path.Combine(assemblySearchPath, "runtimes", $"{platform}-{architecture}", dynamicLibraryName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Native Library not found in path {path}. " +
                $"Verify you have have included the native Whisper library in your application, " +
                $"or install the default libraries with the Whisper.net.Runtime NuGet.");
        }

        ILibraryLoader libraryLoader = platform switch
        {
            "win" => new WindowsLibraryLoader(),
            "macos" => new MacOsLibraryLoader(),
            "linux" => new LinuxLibraryLoader(),
            _ => throw new PlatformNotSupportedException($"Currently {platform} platform is not supported")
        };

        var result = libraryLoader.OpenLibrary(path);
        return result;
#endif
    }
}
