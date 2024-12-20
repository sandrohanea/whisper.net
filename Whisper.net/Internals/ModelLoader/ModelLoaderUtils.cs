// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Native;
using NativeHeadsPreset = Whisper.net.Native.WhisperAlignmentHeadsPreset;

namespace Whisper.net.Internals.ModelLoader;

internal static class ModelLoaderUtils
{
    public static NativeHeadsPreset Map(WhisperAlignmentHeadsPreset preset)
    {
        return preset switch
        {
            WhisperAlignmentHeadsPreset.None => NativeHeadsPreset.WHISPER_AHEADS_NONE,
            WhisperAlignmentHeadsPreset.NTopMost => NativeHeadsPreset.WHISPER_AHEADS_N_TOP_MOST,
            WhisperAlignmentHeadsPreset.Custom => NativeHeadsPreset.WHISPER_AHEADS_CUSTOM,
            WhisperAlignmentHeadsPreset.TinyEn => NativeHeadsPreset.WHISPER_AHEADS_TINY_EN,
            WhisperAlignmentHeadsPreset.Tiny => NativeHeadsPreset.WHISPER_AHEADS_TINY,
            WhisperAlignmentHeadsPreset.Base => NativeHeadsPreset.WHISPER_AHEADS_BASE,
            WhisperAlignmentHeadsPreset.BaseEn => NativeHeadsPreset.WHISPER_AHEADS_BASE_EN,
            WhisperAlignmentHeadsPreset.Small => NativeHeadsPreset.WHISPER_AHEADS_SMALL,
            WhisperAlignmentHeadsPreset.SmallEn => NativeHeadsPreset.WHISPER_AHEADS_SMALL_EN,
            WhisperAlignmentHeadsPreset.Medium => NativeHeadsPreset.WHISPER_AHEADS_MEDIUM,
            WhisperAlignmentHeadsPreset.MediumEn => NativeHeadsPreset.WHISPER_AHEADS_MEDIUM_EN,
            WhisperAlignmentHeadsPreset.LargeV1 => NativeHeadsPreset.WHISPER_AHEADS_LARGE_V1,
            WhisperAlignmentHeadsPreset.LargeV2 => NativeHeadsPreset.WHISPER_AHEADS_LARGE_V2,
            WhisperAlignmentHeadsPreset.LargeV3 => NativeHeadsPreset.WHISPER_AHEADS_LARGE_V3,
            WhisperAlignmentHeadsPreset.LargeV3Turbo => NativeHeadsPreset.WHISPER_AHEADS_LARGE_V3_TURBO,
            _ => throw new ArgumentOutOfRangeException(nameof(preset), preset, null)
        };
    }

    public static WhisperAheads GetWhisperAlignmentHeads(WhisperAlignmentHead[]? alignmentHeads, out GCHandle? aHeadsHandle)
    {
        if (alignmentHeads == null || alignmentHeads.Length == 0)
        {
            aHeadsHandle = null;
            return default;
        }

        var nHeads = alignmentHeads.Length;
        var aHeads = new int[nHeads * 2];

        aHeadsHandle = GCHandle.Alloc(aHeads, GCHandleType.Pinned);

        for (var i = 0; i < nHeads; i++)
        {
            aHeads[i * 2] = alignmentHeads![i].TextLayer;
            aHeads[i * 2 + 1] = alignmentHeads[i].Head;
        }

        return new WhisperAheads()
        {
            NHeads = (UIntPtr)nHeads,
            Heads = aHeadsHandle.Value.AddrOfPinnedObject()
        };
    }

    public static byte AsByte(this bool value)
    {
        return value ? (byte)1 : (byte)0;
    }
}
