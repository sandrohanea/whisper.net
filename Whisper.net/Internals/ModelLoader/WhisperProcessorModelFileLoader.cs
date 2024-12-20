// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class WhisperProcessorModelFileLoader : IWhisperProcessorModelLoader
{
    private readonly string pathModel;
    private readonly WhisperFactoryOptions options;
    private readonly WhisperAheads aHeads;
    private readonly GCHandle? aheadsHandle;

    public WhisperProcessorModelFileLoader(string pathModel, WhisperFactoryOptions options)
    {
        this.pathModel = pathModel;
        this.options = options;
        aHeads = ModelLoaderUtils.GetWhisperAlignmentHeads(options.CustomAlignmentHeads, out aheadsHandle);
    }

    public void Dispose()
    {
        if (aheadsHandle.HasValue)
        {
            aheadsHandle.Value.Free();
        }
    }

    public IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
    {
        ModelLoaderUtils.GetWhisperAlignmentHeads(options.CustomAlignmentHeads, out var aheadsHandle);

        return nativeWhisper.Whisper_Init_From_File_With_Params_No_State(pathModel,
           new WhisperContextParams()
           {
               UseGpu = options.UseGpu.AsByte(),
               FlashAttention = options.UseFlashAttention.AsByte(),
               GpuDevice = options.GpuDevice,
               DtwTokenLevelTimestamp = options.UseDtwTimeStamps.AsByte(),
               HeadsPreset = ModelLoaderUtils.Map(options.HeadsPreset),
               DtwNTop = options.DtwNTop,
               WhisperAheads = aHeads,
               Dtw_mem_size = new UIntPtr(options.DtwMemSize)
           });
    }
}
