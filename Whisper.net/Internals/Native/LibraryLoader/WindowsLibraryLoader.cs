// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.ComponentModel;
using System.Runtime.InteropServices;
using Whisper.net.Internals.Native.LibraryLoader;

namespace Whisper.net.Native.LibraryLoader;

internal class WindowsLibraryLoader : ILibraryLoader
{
    private const uint LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000;
    public LoadResult OpenLibrary(string filename)
    {
        IntPtr loadedLib;

        try
        {
            loadedLib = LoadLibraryEx(filename, IntPtr.Zero, LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
        }
        catch (DllNotFoundException)
        {
            loadedLib = LoadLibrary(filename);
        }

        if (loadedLib == IntPtr.Zero)
        {
            var errorCode = Marshal.GetLastWin32Error();
            var errorMessage = new Win32Exception(errorCode).Message;
            return LoadResult.Failure(errorMessage);
        }

        return LoadResult.Success;
    }

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string lpFileName);

    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern IntPtr LoadLibraryEx([MarshalAs(UnmanagedType.LPTStr)] string lpFileName, IntPtr hFile, uint dwFlags);
}
