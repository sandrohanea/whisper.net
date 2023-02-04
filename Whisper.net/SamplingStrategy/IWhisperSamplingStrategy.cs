using Whisper.net.Native;

namespace Whisper.net.SamplingStrategy;

internal interface IWhisperSamplingStrategy
{
    public WhisperSamplingStrategy GetNativeStrategy();
}
