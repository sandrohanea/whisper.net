// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class WhisperProcessorModelFileLoader : IWhisperProcessorModelLoader
{
    private readonly string pathModel;

    public WhisperProcessorModelFileLoader(string pathModel)
    {
        this.pathModel = pathModel;
    }

    public void Dispose()
    {

    }

    public IntPtr LoadNativeContext()
    {
        return NativeMethods.whisper_init_from_file_no_state(pathModel);
    }
}
