// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if NETSTANDARD
using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;

internal class LibdlLibraryLoader : ILibraryLoader
{
    [DllImport("libdl", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl(string? filename, int flags);

    [DllImport("libdl", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError();

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
