// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Reflection;
using System.Runtime.InteropServices;
using WhisperNetDependencyChecker.DependencyWalker;

namespace Whisper.net.LibraryLoader;
internal class UniversalLibraryLoader : INativeLibraryLoader
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
        var pinvokeError = Marshal.GetLastPInvokeErrorMessage();
        return $"Cannot load the library on this platform using NativeLibrary. PInvokeError: {pinvokeError}";
    }

    public bool TryOpenLibrary(string fileName, out IntPtr libHandle)
    {
        return NativeLibrary.TryLoad(fileName, whisperAssembly, null, out libHandle);
    }
}
