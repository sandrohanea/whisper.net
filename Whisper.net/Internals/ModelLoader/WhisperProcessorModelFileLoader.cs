using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class WhisperProcessorModelFileLoader : IWhisperProcessorModelLoader
{
    private string pathModel;

    public WhisperProcessorModelFileLoader(string pathModel)
    {
        this.pathModel = pathModel;
    }

    public void Dispose()
    {

    }

    public IntPtr LoadNativeContext()
    {
        return NativeMethods.whisper_init_from_file(pathModel);
    }
}
