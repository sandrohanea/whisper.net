namespace Whisper.net.Internals.ModelLoader;

internal interface IWhisperProcessorModelLoader : IDisposable
{
    public IntPtr LoadNativeContext();
}
