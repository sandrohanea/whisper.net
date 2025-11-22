// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.SamplingStrategy;

namespace Whisper.net;

/// <summary>
/// Builder for <seealso cref="GreedySamplingStrategyBuilder"/>
/// </summary>
public class GreedySamplingStrategyBuilder
{
    private readonly GreedySamplingStrategy greedySamplingStrategy;

    internal GreedySamplingStrategyBuilder(GreedySamplingStrategy greedySamplingStrategy)
    {
        this.greedySamplingStrategy = greedySamplingStrategy;
    }

    /// <summary>
    /// Configures the Greedy Sampling Strategy with the specified <paramref name="bestOf"/>.
    /// </summary>
    /// <param name="bestOf">The best of to be used</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not configured, 1 decoder is used.
    /// </remarks>
    public GreedySamplingStrategyBuilder WithBestOf(int bestOf)
    {
        greedySamplingStrategy.BestOf = bestOf;
        return this;
    }
}
