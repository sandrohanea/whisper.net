// Licensed under the MIT license: https://opensource.org/licenses/MIT

#if NET6_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using Whisper.net.Internals.Native;
using Whisper.net.Internals.Native.Implementations;

namespace Whisper.net.LibraryLoader;
internal static class CudaHelper
{
    public static bool IsCudaAvailable()
    {
        INativeCuda nativeCuda;
#if NET6_0_OR_GREATER
        if (!NativeLibrary.TryLoad("cuda", out var library))
        {
            return false;
        }
        nativeCuda = new NativeLibraryCuda(library);
#else
        try
        {
            nativeCuda = new DllImportNativeCuda();
        }
        catch
        {
            return false;
        }
#endif
        nativeCuda.CudaGetDeviceCount(out var count);
        return count > 0;
    }
}
