// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;

namespace Whisper.net.Internals.ModelLoader;

internal interface IWhisperProcessorModelLoader : IDisposable
{
    public IntPtr LoadNativeContext(INativeWhisper nativeWhisper);
}
