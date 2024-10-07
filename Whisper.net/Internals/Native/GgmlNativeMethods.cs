// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Internals.Native;
internal static class GgmlNativeMethods
{
    // Needed for Apple apps when linking library.
#if IOS || MACCATALYST || TVOS
    const string libraryName = "__Internal";
#else
    const string libraryName = "ggml";
#endif

    [DllImport(libraryName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void ggml_log_set(IntPtr logCallback, IntPtr user_data);

}
