// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Native.LibraryLoader;

namespace Whisper.net.Native;

internal static class NativeLibraryLoader
{
    internal static bool LoadNativeLibrary()
    {
        var architecture = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.X86 => "x86",
            Architecture.Arm => "arm",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

        var (platform, extension) = Environment.OSVersion.Platform switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => ("win", "dll"),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => ("linux", "so"),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => ("osx", "dylib"),
            _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
        };

        var assemblySearchPath = string.IsNullOrEmpty(AppDomain.CurrentDomain.RelativeSearchPath)
                ? Path.GetDirectoryName(typeof(NativeMethods).Assembly.Location)!
                : AppDomain.CurrentDomain.RelativeSearchPath;

        var path = Path.Combine(assemblySearchPath, "runtimes", $"{platform}-{architecture}", $"whisper.{extension}");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Native Library not found in path {path}. Probably it is not supported yet.");
        }

        ILibraryLoader libraryLoader = platform switch
        {
            "win" => new WindowsLibraryLoader(),
            "osx" => new MacOsLibraryLoader(),
            "linux" => new LinuxLibraryLoader(),
            _ => throw new PlatformNotSupportedException($"Currently {platform} platform is not supported")
        };

        // open with rtls lazy flag
        var result = libraryLoader.OpenLibrary(path, 0x00001);
        return result != IntPtr.Zero;
    }
}
