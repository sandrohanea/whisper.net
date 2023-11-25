// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal class WhisperProcessorModelBufferLoader(byte[] buffer, bool useGpu) : IWhisperProcessorModelLoader
{
    private readonly GCHandle pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

    public void Dispose()
    {
        pinnedBuffer.Free();
    }

    public IntPtr LoadNativeContext()
    {
        var bufferLength = new UIntPtr((uint)buffer.Length);
        return NativeMethods.whisper_init_from_buffer_with_params_no_state(pinnedBuffer.AddrOfPinnedObject(), bufferLength, new WhisperContextParams() { UseGpu = useGpu ? (byte)1 : (byte)0 });
    }
}
