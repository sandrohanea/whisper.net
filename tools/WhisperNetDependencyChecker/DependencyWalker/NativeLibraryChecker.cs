namespace WhisperNetDependencyChecker.DependencyWalker;

using System;
using System.Runtime.InteropServices;

public static class NativeLibraryChecker
{
    // Windows API (GetModuleHandle)
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    // Linux/macOS API (dlopen with RTLD_NOLOAD)
    private const int RTLD_NOLOAD = 0x4; // Flag to check if the library is already loaded

    [DllImport("libdl.so.2", EntryPoint = "dlopen", SetLastError = true)]
    private static extern IntPtr dlopen_linux(string filename, int flags);

    [DllImport("libSystem.dylib", EntryPoint = "dlopen", SetLastError = true)]
    private static extern IntPtr dlopen_macos(string filename, int flags);

    public static IntPtr GetLibraryHandle(string libraryName)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return GetModuleHandle(libraryName);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return dlopen_linux(libraryName, RTLD_NOLOAD);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return dlopen_macos(libraryName, RTLD_NOLOAD);
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported operating system.");
        }
    }

    public static bool IsLibraryLoaded(string libraryName)
    {
        return GetLibraryHandle(libraryName) != IntPtr.Zero;
    }
}
