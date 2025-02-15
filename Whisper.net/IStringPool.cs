// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// A pool for strings.
/// </summary>
public interface IStringPool
{
    /// <summary>
    /// Converts a native UTF8 pointer (null-terminated) into a pooled string.
    /// </summary>
    string? GetStringUtf8(IntPtr nativePointer);

    /// <summary>
    /// Returns a string to the pool.
    /// </summary>
    void ReturnString(string? returnedString);
}
