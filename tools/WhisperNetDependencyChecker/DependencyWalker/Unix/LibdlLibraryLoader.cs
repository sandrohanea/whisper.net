// Licensed under the MIT license: https://opensource.org/licenses/MIT
using System.Runtime.InteropServices;
using WhisperNetDependencyChecker.DependencyWalker;

namespace Whisper.net.LibraryLoader;

internal class LibdlLibraryLoader : INativeLibraryLoader
{
    // We need to use the libdl.so.2 library on some systems
    // We use this flag to remember which one to use, so we don't have to check every time
    private bool? useLibdl2;

    [DllImport("libdl", CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl(string? filename, int flags);

    [DllImport("libdl", CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError();

    [DllImport("libdl", CharSet = CharSet.Auto, EntryPoint = "dlclose")]
    public static extern int NativeCloseLibraryLibdl(IntPtr handle);

    [DllImport("libdl.so.2", CharSet = CharSet.Auto, EntryPoint = "dlopen")]
    public static extern IntPtr NativeOpenLibraryLibdl2(string? filename, int flags);

    [DllImport("libdl.so.2", CharSet = CharSet.Auto, EntryPoint = "dlerror")]
    public static extern IntPtr GetLoadError2();

    [DllImport("libdl.so.2", CharSet = CharSet.Auto, EntryPoint = "dlclose")]
    public static extern int NativeCloseLibraryLibdl2(IntPtr handle);

    public void CloseLibrary(IntPtr handle)
    {
        if (useLibdl2.HasValue)
        {
            if (useLibdl2.Value)
            {
                NativeCloseLibraryLibdl2(handle);
                return;
            }
            NativeCloseLibraryLibdl(handle);
            return;
        }

        // We don't know which one can be used, so we try both
        try
        {
            NativeCloseLibraryLibdl(handle);
            useLibdl2 = false;
        }
        catch (DllNotFoundException)
        {
            NativeCloseLibraryLibdl2(handle);
            useLibdl2 = true;
        }
    }

    public bool TryOpenLibrary(string fileName, out IntPtr libHandle)
    {
        try
        {
            // open with rtld now + global
            if (useLibdl2.HasValue)
            {
                libHandle = useLibdl2.Value ? NativeOpenLibraryLibdl2(fileName, 0x00102) : NativeOpenLibraryLibdl(fileName, 0x00102);
                return libHandle != IntPtr.Zero;
            }

            // We don't know which one can be used, so we try both
            try
            {
                libHandle = NativeOpenLibraryLibdl(fileName, 0x00102);
                useLibdl2 = false;
                return libHandle != IntPtr.Zero;
            }
            catch (DllNotFoundException)
            {
                libHandle = NativeOpenLibraryLibdl2(fileName, 0x00102);
                useLibdl2 = true;
                return libHandle != IntPtr.Zero;
            }
        }
        catch (DllNotFoundException)
        {
            libHandle = IntPtr.Zero;
            return false;
        }
    }

    public string GetLastError()
    {
        if (useLibdl2.HasValue)
        {
            var error = useLibdl2.Value ? GetLoadError2() : GetLoadError();
            return GetError(error);
        }

        // We don't know which one can be used, so we try both
        try
        {
            var error = GetLoadError();
            useLibdl2 = false;
            return GetError(error);
        }
        catch (DllNotFoundException)
        {
            var error = GetLoadError2();
            useLibdl2 = true;
            return GetError(error);
        }
    }

    private string GetError(IntPtr error)
    {
        return Marshal.PtrToStringAnsi(error) ?? "Unknown error";
    }
}
