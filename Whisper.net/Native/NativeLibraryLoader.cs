using System.Runtime.InteropServices;

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
                return false;
            }

            if (platform == "win")
            {
                return LoadLibrary(path) != IntPtr.Zero;
            }
            if (platform == "linux" || platform == "osx")
            {
                // open with rtls lazy flag
                var result = dlopen(path, 0x00001);
                return result != IntPtr.Zero;
            }

            throw new PlatformNotSupportedException("Currently the platform is not supported");
        }

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

        [DllImport("libdl", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr dlopen(string filename, int flags);
    }
}
