// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using System.Text;

namespace Whisper.net.Internals;

internal static class MarshalUtils
{
    public static string? GetString(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
        {
            return null;
        }
#if NETSTANDARD
        var len = 0;

        while (Marshal.ReadByte(ptr, len) != 0)
        {
            len++;
        }

        var buffer = new byte[len];
        Marshal.Copy(ptr, buffer, 0, buffer.Length);
        return System.Text.Encoding.UTF8.GetString(buffer);
#else
        return Marshal.PtrToStringUTF8(ptr);
#endif
    }

    public static IntPtr GetStringHGlobalPtr(string? str)
    {
        if (str is null)
        {
            return IntPtr.Zero;
        }

        var byteCount = Encoding.UTF8.GetByteCount(str);
        var mem = Marshal.AllocHGlobal(byteCount + 1); // +1 for '\0'
        CopyStringToPtr(str, mem, byteCount);
        return mem;
    }

    public static void TryReleaseStringHGlobal(IntPtr? ptr)
    {
        if (ptr.HasValue && ptr.Value != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(ptr.Value);
        }
    }

    public static unsafe void CopyStringToPtr(string? str, IntPtr ptr, int byteCount)
    {
        var dest = new Span<byte>((void*)ptr, byteCount + 1);
#if NETSTANDARD
            var managedMemoryBuffer = Encoding.UTF8.GetBytes(str);
            managedMemoryBuffer.CopyTo(dest);
            var written = managedMemoryBuffer.Length;
#else
        var written = Encoding.UTF8.GetBytes(str, dest);
#endif
        dest[written] = 0; // NUL-terminate
    }

}
