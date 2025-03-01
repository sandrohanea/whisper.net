// Licensed under the MIT license: https://opensource.org/licenses/MIT
using System.ComponentModel;
using System.Runtime.InteropServices;
using WhisperNetDependencyChecker.DependencyWalker;

namespace Whisper.net.LibraryLoader;

internal class WindowsLibraryLoader : INativeLibraryLoader
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string? lpFileName);

    [DllImport("kernel32", SetLastError = true)]
    private static extern bool FreeLibrary(IntPtr hModule);

    public void CloseLibrary(nint handle)
    {
        FreeLibrary(handle);
    }

    public bool TryOpenLibrary(string fileName, out IntPtr libHandle)
    {
        try
        {
            libHandle = LoadLibrary(fileName);
            return libHandle != IntPtr.Zero;
        }
        catch
        {
            libHandle = IntPtr.Zero;
            return false;
        }
    }

    public string GetLastError()
    {
        var errorCode = Marshal.GetLastWin32Error();
        return new Win32Exception(errorCode).Message;
    }
}
