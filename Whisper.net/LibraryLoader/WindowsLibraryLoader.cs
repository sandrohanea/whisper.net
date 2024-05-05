// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;

internal class WindowsLibraryLoader : ILibraryLoader
{
  public LoadResult OpenLibrary(string? fileName)
  {
    var loadedLib = LoadLibrary(fileName);

    if (loadedLib == IntPtr.Zero)
    {
      var errorCode = Marshal.GetLastWin32Error();
      var errorMessage = new Win32Exception(errorCode).Message;
      return LoadResult.Failure(errorMessage);
    }

    return LoadResult.Success;
  }

  [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Auto)]
  private static extern IntPtr
      LoadLibrary([MarshalAs(UnmanagedType.LPTStr)] string? lpFileName);
}
