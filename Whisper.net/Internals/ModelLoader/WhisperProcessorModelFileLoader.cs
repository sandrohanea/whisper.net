// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class WhisperProcessorModelFileLoader
    : IWhisperProcessorModelLoader {
  private readonly string pathModel;
  private readonly bool useGpu;

  public WhisperProcessorModelFileLoader(string pathModel, bool useGpu) {
    this.pathModel = pathModel;
    this.useGpu = useGpu;
  }

  public void Dispose() {}

  public IntPtr LoadNativeContext() {
    return NativeMethods.whisper_init_from_file_with_params_no_state(
        pathModel,
        new WhisperContextParams() { UseGpu = useGpu ? (byte)1 : (byte)0 });
  }
}
