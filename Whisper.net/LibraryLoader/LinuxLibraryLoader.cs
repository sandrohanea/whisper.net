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

    public IntPtr OpenLibrary(string fileName)
    {
        try
        {
            // open with rtld now + global
            return NativeOpenLibraryLibdl2(fileName, 0x00102);
        }
        catch (DllNotFoundException)
        {
            return NativeOpenLibraryLibdl(fileName, 0x00102);
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
