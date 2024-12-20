// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;
public enum WhisperAlignmentHeadsPreset
{
    /// <summary>
    /// No alignment heads.
    /// </summary>
    None,
    /// <summary>
    /// All heads from the N-top-most text-layers
    /// </summary>
    NTopMost,
    /// <summary>
    /// Custom alignment heads, provided by <see cref="WhisperFactoryOptions.CustomAlignmentHeads" />
    /// </summary>
    Custom,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.TinyEn"/>
    /// </summary>
    TinyEn,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.Tiny"/>
    /// </summary>
    Tiny,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.BaseEn"/>
    /// </summary>
    BaseEn,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.Base"/>
    /// </summary>
    Base,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.SmallEn"/>
    /// </summary>
    SmallEn,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.Small"/>
    /// </summary>
    Small,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.Medium"/>
    /// </summary>
    MediumEn,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.MediumEn"/>
    /// </summary>
    Medium,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.LargeV1"/>
    /// </summary>
    LargeV1,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.LargeV2"/>
    /// </summary>
    LargeV2,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.LargeV3"/>
    /// </summary>
    LargeV3,
    /// <summary>
    /// Alignment heads for the model <see cref="Ggml.GgmlType.LargeV3Turbo"/>
    /// </summary>
    LargeV3Turbo
}

public readonly struct WhisperAlignmentHead(int textLayer, int head)
{
    public int TextLayer { get; } = textLayer;
    public int Head { get; } = head;
}
