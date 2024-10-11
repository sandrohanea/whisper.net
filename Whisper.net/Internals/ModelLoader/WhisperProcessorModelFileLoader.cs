// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;
using Whisper.net.LibraryLoader;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class WhisperProcessorModelFileLoader(string pathModel) : IWhisperProcessorModelLoader
{
    public void Dispose()
    {

    }

    public IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
    {
        return nativeWhisper.Whisper_Init_From_File_With_Params_No_State(pathModel,
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
