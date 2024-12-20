// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal class WhisperProcessorModelBufferLoader : IWhisperProcessorModelLoader
{
    private readonly GCHandle pinnedBuffer;
    private readonly WhisperAheads aHeads;
    private readonly GCHandle? aheadsHandle;
    private readonly UIntPtr bufferLength;

    private readonly WhisperFactoryOptions options;

    public WhisperProcessorModelBufferLoader(byte[] buffer, WhisperFactoryOptions options)
    {
        this.options = options;

        pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        aHeads = ModelLoaderUtils.GetWhisperAlignmentHeads(options.CustomAlignmentHeads, out aheadsHandle);
        bufferLength = new UIntPtr((uint)buffer.Length);
    }

    public void Dispose()
    {
        pinnedBuffer.Free();
        if (aheadsHandle.HasValue)
        {
            aheadsHandle.Value.Free();
        }
    }

    public IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
    {
        return nativeWhisper.Whisper_Init_From_Buffer_With_Params_No_State(pinnedBuffer.AddrOfPinnedObject(), bufferLength,
            new WhisperContextParams()
            {
                UseGpu = options.UseGpu.AsByte(),
                FlashAttention = options.UseFlashAttention.AsByte(),
                GpuDevice = options.GpuDevice,
                DtwTokenLevelTimestamp = options.UseDtwTimeStamps.AsByte(),
                HeadsPreset = ModelLoaderUtils.Map(options.HeadsPreset),
                DtwNTop = options.DtwNTop,
                WhisperAheads = aHeads,
                Dtw_mem_size = new UIntPtr(options.DtwMemSize),
            });
    }
}
