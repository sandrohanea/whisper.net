// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.LibraryLoader;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class WhisperProcessorModelFileLoader(string pathModel) : IWhisperProcessorModelLoader
{
    private GCHandle aheadsHandle;

    public void Dispose()
    {
        if (aheadsHandle.IsAllocated)
        {
            aheadsHandle.Free();
        }
    }

    public static WhisperAheads GetWhisperAlignmentHeads(Ggml.WhisperAlignmentHead[]? alignmentHeads, ref GCHandle aHeadsHandle)
    {
        var aHeadsPtr = IntPtr.Zero;
        var nHeads = alignmentHeads?.Length ?? 0;

        if (nHeads > 0)
        {
            var aHeads = new int[nHeads * 2];
            if (aHeadsHandle.IsAllocated)
            {
                aHeadsHandle.Free();
            }
            aHeadsHandle = GCHandle.Alloc(aHeads, GCHandleType.Pinned);
            aHeadsPtr = aHeadsHandle.AddrOfPinnedObject();

            for (var i = 0; i < nHeads; i++)
            {
                aHeads[i * 2] = alignmentHeads![i].TextLayer;
                aHeads[i * 2 + 1] = alignmentHeads[i].Head;
            }
        }

        return new WhisperAheads()
        { 
            NHeads = (nuint)nHeads,
            Heads = aHeadsPtr
        };
    }

    public IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
    {
        var aHeads = GetWhisperAlignmentHeads(RuntimeOptions.Instance.CustomAlignmentHeads, ref aheadsHandle);

        return nativeWhisper.Whisper_Init_From_File_With_Params_No_State(pathModel,
           new WhisperContextParams()
           {
               UseGpu = RuntimeOptions.Instance.UseGpu ? (byte)1 : (byte)0,
               FlashAttention = RuntimeOptions.Instance.UseFlashAttention ? (byte)1 : (byte)0,
               GpuDevice = RuntimeOptions.Instance.GpuDevice,
               DtwTokenLevelTimestamp = RuntimeOptions.Instance.UseDtwTimeStamps ? (byte)1 : (byte)0,
               HeadsPreset = (WhisperAlignmentHeadsPreset)RuntimeOptions.Instance.HeadsPreset,
               DtwNTop = -1,
               WhisperAheads = aHeads,
               Dtw_mem_size = 1024 * 1024 * 128,
           });
    }
}
