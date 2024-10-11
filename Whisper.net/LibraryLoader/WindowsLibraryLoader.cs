// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;

internal class WindowsLibraryLoader : ILibraryLoader
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string? lpFileName);

    public IntPtr OpenLibrary(string fileName, bool global)
    {
        return LoadLibrary(fileName);
    }

    public string GetLastError()
    {
        var errorCode = Marshal.GetLastWin32Error();
        return new Win32Exception(errorCode).Message;
    }
}
