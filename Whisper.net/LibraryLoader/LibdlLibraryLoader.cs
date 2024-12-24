// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if NETSTANDARD
using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;

internal class LibdlLibraryLoader : ILibraryLoader
{
    [DllImport("libdl", CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl(string? filename, int flags);

    [DllImport("libdl", CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError();

    [DllImport("libdl", CharSet = CharSet.Auto, EntryPoint = "dlclose")]
    public static extern int NativeCloseLibraryLibdl(IntPtr handle);

    public void CloseLibrary(IntPtr handle)
    {
        NativeCloseLibraryLibdl(handle);
    }

    public bool TryOpenLibrary(string fileName, out IntPtr libHandle)
    {
        try
        {
            // open with rtld now + global
            libHandle = NativeOpenLibraryLibdl(fileName, 0x00102);
            return libHandle != IntPtr.Zero;
        }
        catch (DllNotFoundException)
        {
            libHandle = IntPtr.Zero;
            return false;
        }
    }

    public string GetLastError()
    {
        return Marshal.PtrToStringAnsi(GetLoadError()) ?? "Unknown error";
    }
}
#endif
