// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Ggml;

public enum GgmlType
{
    Tiny,
    TinyEn,
    Base,
    BaseEn,
    Small,
    SmallEn,
    Medium,
    MediumEn,
    LargeV1,
    LargeV2,
    LargeV3,
    LargeV3Turbo
}

public enum WhisperAlignmentHeadsPreset
{
    None,
    NTopMost,  // All heads from the N-top-most text-layers
    Custom,
    TinyEn,
    Tiny,
    BaseEn,
    Base,
    SmallEn,
    Small,
    MediumEn,
    Medium,
    LargeV1,
    LargeV2,
    LargeV3,
    LargeV3Turbo
}

public class WhisperAlignmentHead
{
    public int TextLayer;
    public int Head;

    public WhisperAlignmentHead(int textLayer, int head)
    {
        TextLayer = textLayer;
        Head = head;
    }
}
