// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;
using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal class WhisperProcessorModelMemoryLoader : IWhisperProcessorModelLoader
{
    private readonly MemoryHandle pinnedMemory;
    private readonly WhisperAheads aHeads;
    private readonly GCHandle? aheadsHandle;
    private readonly UIntPtr bufferLength;

    private readonly WhisperFactoryOptions options;

    public WhisperProcessorModelMemoryLoader(Memory<byte> buffer, WhisperFactoryOptions options)
    {
        this.options = options;
        pinnedMemory = buffer.Pin();
        aHeads = ModelLoaderUtils.GetWhisperAlignmentHeads(options.CustomAlignmentHeads, out aheadsHandle);
        bufferLength = new UIntPtr((uint)buffer.Length);
    }

    public void Dispose()
    {
        pinnedMemory.Dispose();
        if (aheadsHandle.HasValue)
        {
            aheadsHandle.Value.Free();
        }
    }

    public unsafe IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
    {
        return nativeWhisper.Whisper_Init_From_Buffer_With_Params_No_State((IntPtr)pinnedMemory.Pointer, bufferLength,
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
