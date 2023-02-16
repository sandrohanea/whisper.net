// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Native.LibraryLoader;

internal class WindowsLibraryLoader : ILibraryLoader
{
    public IntPtr OpenLibrary(string filename, int flags)
    {
        return LoadLibrary(filename);
    }

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);
}
