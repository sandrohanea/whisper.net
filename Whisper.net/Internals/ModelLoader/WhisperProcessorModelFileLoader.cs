// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class WhisperProcessorModelFileLoader(string pathModel, bool useGpu) : IWhisperProcessorModelLoader
{
    public void Dispose()
    {

    }

    public IntPtr LoadNativeContext()
    {
         return NativeMethods.whisper_init_from_file_with_params_no_state(pathModel,
            new WhisperContextParams()
            {
                UseGpu = useGpu ? (byte)1 : (byte)0,
                FlashAttention = 0,
                GpuDevice = 0,
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
