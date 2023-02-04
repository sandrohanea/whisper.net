using Whisper.net.SamplingStrategy;

namespace Whisper.net;

/// <summary>
/// Builder for <seealso cref="BeamSearchSamplingStrategy"/>
/// </summary>
public class BeamSearchSamplingStrategyBuilder : IWhisperSamplingStrategyBuilder
{
    private BeamSearchSamplingStrategy beamSearchSamplingStrategy;

    internal BeamSearchSamplingStrategyBuilder(WhisperProcessorBuilder whisperProcessorBuilder, BeamSearchSamplingStrategy beamSearchSamplingStrategy)
    {
        ParentBuilder = whisperProcessorBuilder;
        this.beamSearchSamplingStrategy = beamSearchSamplingStrategy;
    }

    /// <inheritdoc/>
    public WhisperProcessorBuilder ParentBuilder { get; }

    /// <summary>
    /// Configures the Beam Search Sampling Strategy with the specified <paramref name="beamSize"/>.
    /// </summary>
    /// <param name="beamSize">The beam size to be used</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not configured, 5 beam sizes are used.
    /// </remarks>
    public BeamSearchSamplingStrategyBuilder WithBeamSize(int beamSize)
    {
        beamSearchSamplingStrategy.BeamSize = beamSize;
        return this;
    }

    /// <summary>
    /// Configures the Beam Search Sampling Strategy with the specified <paramref name="patience"/>.
    /// </summary>
    /// <param name="patience">The patience to be used</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If not configured, -1.0f patience is used.
    /// Note: This is not implmented yet in the native code.
    /// </remarks>
    public BeamSearchSamplingStrategyBuilder WithPatience(float patience)
    {
        beamSearchSamplingStrategy.Patience = patience;
        return this;
    }
}
