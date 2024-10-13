// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.LibraryLoader;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal class WhisperProcessorModelBufferLoader(byte[] buffer) : IWhisperProcessorModelLoader
{
    private readonly GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

    public void Dispose()
    {
        pinnedBuffer.Free();
    }

    public IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
    {
        var bufferLength = new UIntPtr((uint)buffer.Length);
        return nativeWhisper.Whisper_Init_From_Buffer_With_Params_No_State(pinnedBuffer.AddrOfPinnedObject(), bufferLength,
            new WhisperContextParams()
            {
                UseGpu = RuntimeOptions.Instance.UseGpu ? (byte)1 : (byte)0,
                FlashAttention = 0,
                GpuDevice = RuntimeOptions.Instance.GpuDevice,
                DtwTokenLevelTimestamp = 0,
                HeadsPreset = WhisperAlignmentHeadsPreset.WHISPER_AHEADS_NONE,
                DtwNTop = -1,
                WhisperAheads = new WhisperAheads()
                {
                    NHeads = 0,
                    Heads = IntPtr.Zero
                },
                Dtw_mem_size = 1024 * 1024 * 128,
            });
    }
}
