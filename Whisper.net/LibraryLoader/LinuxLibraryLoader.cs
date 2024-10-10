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

    public LoadResult OpenLibrary(string? fileName, bool global)
    {
        IntPtr loadedLib;
        try
        {
            // open with rtls lazy flag
            loadedLib = NativeOpenLibraryLibdl2(fileName, global ? 0x00102 : 0x00002);
        }
        catch (DllNotFoundException)
        {
            loadedLib = NativeOpenLibraryLibdl(fileName, global ? 0x00102 : 0x00002);
        }

        if (loadedLib == IntPtr.Zero)
        {
            string errorMessage;
            try
            {
                errorMessage = Marshal.PtrToStringAnsi(GetLoadError2()) ?? "Unknown error";
            }
            catch (DllNotFoundException)
            {
                errorMessage = Marshal.PtrToStringAnsi(GetLoadError()) ?? "Unknown error";
            }

            return LoadResult.Failure(errorMessage);
        }

        return LoadResult.Success;
    }
}
