// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Exception thrown when the underlying native whisper library fails during processing.
/// </summary>
public class WhisperProcessingException : Exception
{
    /// <summary>
    /// Gets the native error code returned by whisper.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// Creates a new instance of <see cref="WhisperProcessingException"/>, using a descriptive message based on the error code.
    /// </summary>
    /// <param name="errorCode">Native error code returned by whisper.</param>
    public WhisperProcessingException(int errorCode)
        : base(GetErrorMessage(errorCode))
    {
        ErrorCode = errorCode;
    }

    private static string GetErrorMessage(int errorCode)
    {
        return errorCode switch
        {
            -1 => "Failed to compute voice activity detection.",
            -2 => "Failed to compute log mel spectrogram.",
            -3 => "Failed to auto-detect language.",
            -4 => "Too many decoders requested.",
            -5 => "Audio context is larger than the maximum allowed.",
            -6 => "Failed to encode audio features.",
            -7 => "Failed to initialize key-value cache for self-attention.",
            -8 => "Failed to decode audio features.",
            -9 => "Failed to decode during token processing.",
            _  => $"Native whisper stopped processing with error code {errorCode}."
        };
    }
}
