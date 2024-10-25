// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if !NETSTANDARD
using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;
internal class UniversalLibraryLoader : ILibraryLoader
{
    public string GetLastError()
    {
        return "Cannot load the library on this platform using NativeLibrary";
    }

    public bool TryOpenLibrary(string fileName, out IntPtr libHandle)
    {
        return NativeLibrary.TryLoad(fileName, System.Reflection.Assembly.GetExecutingAssembly(), DllImportSearchPath.AssemblyDirectory, out libHandle);
    }
}
#endif
