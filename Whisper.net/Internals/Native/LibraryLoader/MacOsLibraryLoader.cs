// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native.LibraryLoader;

namespace Whisper.net.Native.LibraryLoader;

internal class MacOsLibraryLoader : ILibraryLoader
{
    [DllImport("libdl.dylib", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl(string filename, int flags);

    [DllImport("libdl.dylib", ExactSpelling = true, CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError();

    public LoadResult OpenLibrary(string filename)
    {
        var loadedLib = NativeOpenLibraryLibdl(filename, 0x00001);

        if (loadedLib == IntPtr.Zero)
        {
            var errorMessage = Marshal.PtrToStringAnsi(GetLoadError()) ?? "Unknown error";

            return LoadResult.Failure(errorMessage);
        }

        return LoadResult.Success;
    }
}
