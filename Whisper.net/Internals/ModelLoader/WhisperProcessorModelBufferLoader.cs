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
        if (Environment.Is64BitProcess)
        {
            return NativeMethods.whisper_init_from_buffer_x64(pinnedBuffer.AddrOfPinnedObject(), buffer.Length);
        }

        return NativeMethods.whisper_init_from_buffer_x32(pinnedBuffer.AddrOfPinnedObject(), buffer.Length);
    }
}
