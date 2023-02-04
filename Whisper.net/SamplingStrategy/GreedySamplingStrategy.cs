using Whisper.net.Native;

namespace Whisper.net.SamplingStrategy;

internal class GreedySamplingStrategy : IWhisperSamplingStrategy
{
    public WhisperSamplingStrategy GetNativeStrategy()
    {
        return WhisperSamplingStrategy.StrategyGreedy;
    }

    public int? BestOf { get; set; }
}
