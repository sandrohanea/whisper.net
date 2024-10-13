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

    public IntPtr OpenLibrary(string fileName, bool global)
    {
        return NativeLibrary.Load(fileName, System.Reflection.Assembly.GetExecutingAssembly(), DllImportSearchPath.AssemblyDirectory);
    }
}
#endif
