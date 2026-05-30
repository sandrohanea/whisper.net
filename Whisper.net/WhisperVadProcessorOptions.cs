// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Native;

namespace Whisper.net;

internal sealed class WhisperVadProcessorOptions
{
    public IntPtr ContextHandle { get; set; }

    public WhisperVadParams VadParams { get; set; }
}
