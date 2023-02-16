// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Native;

namespace Whisper.net.SamplingStrategy;

internal class BeamSearchSamplingStrategy : IWhisperSamplingStrategy
{
    public WhisperSamplingStrategy GetNativeStrategy()
    {
        return WhisperSamplingStrategy.StrategyBeamSearch;
    }

    public int? BeamSize { get; set; }

    public float? Patience { get; set; }
}
