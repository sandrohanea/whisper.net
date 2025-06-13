// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Exception thrown when the underlying native whisper library fails during processing.
/// </summary>
/// <param name="message">Error message.</param>
/// <param name="errorCode">Native error code returned by whisper.</param>
public class WhisperProcessingException(string message, int errorCode) : Exception(message)
{
    /// <summary>
    /// Gets the native error code returned by whisper.
    /// </summary>
    public int ErrorCode { get; } = errorCode;
}
