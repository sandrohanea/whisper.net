// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal class WhisperProcessorModelBufferLoader : IWhisperProcessorModelLoader
{
    private readonly byte[] buffer;
    private readonly GCHandle pinnedBuffer;

    public WhisperProcessorModelBufferLoader(byte[] buffer)
    {
        this.buffer = buffer;
        pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);
    }

    public void Dispose()
    {
        pinnedBuffer.Free();
    }

    public IntPtr LoadNativeContext()
    {
        var bufferLength = new UIntPtr((uint)buffer.Length);
        return NativeMethods.whisper_init_from_buffer_no_state(pinnedBuffer.AddrOfPinnedObject(), bufferLength);
    }
}
