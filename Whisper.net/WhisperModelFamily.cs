// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Identifies the native engine and model family used by a <see cref="WhisperFactory"/>.
/// </summary>
public enum WhisperModelFamily
{
    /// <summary>
    /// Uses the Whisper engine and a Whisper GGML model.
    /// </summary>
    Whisper,

    /// <summary>
    /// Uses the Parakeet engine and a Parakeet GGML model.
    /// </summary>
    Parakeet
}
