namespace Whisper.net.ModelLoader;

internal interface IWhisperProcessorModelLoader : IDisposable
{
    public IntPtr LoadNativeContext();
}
