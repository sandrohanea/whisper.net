// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;

internal class MacOsLibraryLoader : ILibraryLoader
{
    [DllImport("libdl.dylib", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl(string? filename, int flags);

    [DllImport("libdl.dylib", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError();

    public IntPtr OpenLibrary(string fileName, bool global)
    {
        return NativeOpenLibraryLibdl(fileName, global ? 0x00102 : 0x00001);
    }

    public string GetLastError()
    {
        return Marshal.PtrToStringAnsi(GetLoadError()) ?? "Unknown error";
    }
}
