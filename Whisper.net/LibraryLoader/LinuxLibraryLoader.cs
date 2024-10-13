// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;

internal class LinuxLibraryLoader : ILibraryLoader
{
    [DllImport("libdl.so", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl(string? filename, int flags);

    [DllImport("libdl.so.2", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl2(string? filename, int flags);

    [DllImport("libdl.so", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError();

    [DllImport("libdl.so.2", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError2();

    public IntPtr OpenLibrary(string fileName, bool global)
    {
        try
        {
            // open with rtld now + (global if true)
            return NativeOpenLibraryLibdl2(fileName, global ? 0x00102 : 0x00002);
        }
        catch (DllNotFoundException)
        {
            return NativeOpenLibraryLibdl(fileName, global ? 0x00102 : 0x00002);
        }
    }

    public string GetLastError()
    {
        try
        {
            return Marshal.PtrToStringAnsi(GetLoadError2()) ?? "Unknown error";
        }
        catch (DllNotFoundException)
        {
            return Marshal.PtrToStringAnsi(GetLoadError()) ?? "Unknown error";
        }

    }
}
