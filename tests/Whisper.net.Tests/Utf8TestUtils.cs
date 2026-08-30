// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using System.Text;

namespace Whisper.net.Tests;

internal static class Utf8TestUtils
{
    public static string Decode(ReadOnlySpan<byte> utf8)
    {
#if NETFRAMEWORK
        return Encoding.UTF8.GetString(utf8.ToArray());
#else
        return Encoding.UTF8.GetString(utf8);
#endif
    }

    public static IntPtr AllocateNullTerminated(string value)
    {
        var utf8 = Encoding.UTF8.GetBytes(value + '\0');
        var pointer = Marshal.AllocCoTaskMem(utf8.Length);
        Marshal.Copy(utf8, 0, pointer, utf8.Length);
        return pointer;
    }
}
