// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if !NETSTANDARD
using System.Reflection;
using System.Runtime.InteropServices;

namespace Whisper.net.LibraryLoader;
internal class UniversalLibraryLoader : ILibraryLoader
{
    private readonly Assembly whisperAssembly;
    private string? libPath;

    public UniversalLibraryLoader()
    {
        this.whisperAssembly = typeof(UniversalLibraryLoader).Assembly;
        NativeLibrary.SetDllImportResolver(whisperAssembly, CustomDllImportResolver);
    }

    public string GetLastError()
    {
        return "Cannot load the library on this platform using NativeLibrary";
    }

    public bool TryOpenLibrary(string fileName, out IntPtr libHandle)
    {
        libPath = Path.GetDirectoryName(fileName);
        return NativeLibrary.TryLoad(fileName, whisperAssembly, DllImportSearchPath.LegacyBehavior, out libHandle);
    }

    private nint CustomDllImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        var libName = Path.GetFileNameWithoutExtension(libraryName);
        if (!libName.EndsWith("whisper") || string.IsNullOrEmpty(libPath))
        {
            return NativeLibrary.Load(libraryName);
        }

        var fileExtension = Path.GetExtension(libraryName);
        var libFullPath = Path.Combine(libPath, $"{libName}.{fileExtension}");
        return NativeLibrary.Load(libFullPath);
    }
}
#endif
