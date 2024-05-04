// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Internals.ModelLoader;

internal interface IWhisperProcessorModelLoader : IDisposable {
  public IntPtr LoadNativeContext();
}
