// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Ggml;

/// <summary>
/// Specifies a precision or quantization variant of a Parakeet model.
/// </summary>
public enum ParakeetQuantizationType
{
    /// <summary>
    /// Full 32-bit floating-point precision.
    /// </summary>
    F32,

    /// <summary>
    /// Half 16-bit floating-point precision.
    /// </summary>
    F16,

    /// <summary>
    /// 8-bit quantization.
    /// </summary>
    Q8_0,

    /// <summary>
    /// 4-bit quantization.
    /// </summary>
    Q4_0,

    /// <summary>
    /// 4-bit K-quantization.
    /// </summary>
    Q4_K
}
