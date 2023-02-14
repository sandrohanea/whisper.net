using System.Runtime.InteropServices;
using Whisper.net.Native.LibraryLoader;

namespace Whisper.net.Native
{
    internal static class NativeLibraryLoader
    {
        internal static bool LoadNativeLibrary()
        {
            var architecture = Environment.Is64BitProcess ? "x64" : "x86";
            var (platform, extension) = Environment.OSVersion.Platform switch
            {
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => ("win", "dll"),
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux)=> ("linux", "so"),
                _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => ("osx", "dylib"),
                _ => throw new PlatformNotSupportedException($"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
            };

            Console.WriteLine(platform);

            var assemblySearchPath = string.IsNullOrEmpty(AppDomain.CurrentDomain.RelativeSearchPath)
                    ? Path.GetDirectoryName(typeof(NativeMethods).Assembly.Location)!
                    : AppDomain.CurrentDomain.RelativeSearchPath;

            var path = Path.Combine(assemblySearchPath, "runtimes", $"{platform}-{architecture}", $"whisper.{extension}");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Native Library not found in path {path}.");
            }

            ILibraryLoader libraryLoader;

            switch (platform)
            {
                case "win":
                    libraryLoader = new WindowsLibraryLoader();
                    break;
                case "linux":
                case "osx":
                    libraryLoader = new PosixLibraryLoader();
                    break;
                default:
                    throw new PlatformNotSupportedException($"Currently {platform} platform is not supported");
            }

            // open with rtls lazy flag
            var result = libraryLoader.OpenLibrary(path, 0x00001);
            return result != IntPtr.Zero;
        }
    }
}
