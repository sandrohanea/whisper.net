// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Provides model bytes to the native whisper loader from managed code.
/// </summary>
/// <remarks>
/// Implementations may copy fewer bytes than requested from <see cref="CopyTo"/>;
/// Whisper.net will keep calling it until the native buffer is filled or EOF is reached.
/// </remarks>
public interface IWhisperModelLoader : IDisposable
{
    /// <summary>
    /// Resets the loader to the beginning of the model data before a native load starts.
    /// </summary>
    void Reset();

    /// <summary>
    /// Copies model bytes into <paramref name="destination"/>.
    /// </summary>
    /// <param name="destination">The destination buffer requested by the native loader.</param>
    /// <returns>The number of bytes copied into <paramref name="destination"/>.</returns>
    int CopyTo(Span<byte> destination);

    /// <summary>
    /// Gets a value indicating whether the model source reached EOF.
    /// </summary>
    bool IsEof { get; }
}
