// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if !NETSTANDARD
using System.Reflection;
using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;
internal class UniversalLibraryLoader : ILibraryLoader
{
    private readonly Assembly whisperAssembly;

    public UniversalLibraryLoader()
    {
        whisperAssembly = typeof(UniversalLibraryLoader).Assembly;
    }

    public void CloseLibrary(nint handle)
    {
        NativeLibrary.Free(handle);
    }

    public string GetLastError()
    {
        return "Cannot load the library on this platform using NativeLibrary";
    }

    public bool TryOpenLibrary(string fileName, out IntPtr libHandle)
    {
        return NativeLibrary.TryLoad(fileName, whisperAssembly, null, out libHandle);
    }
}
#endif
