// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Wave;

/// <summary>
/// Options for configuring the <see cref="WaveParser"/>.
/// </summary>
public sealed class WaveParserOptions
{
    /// <summary>
    /// Allows reading truncated wave files without throwing <see cref="CorruptedWaveException"/>.
    /// </summary>
    public bool AllowLessSamples { get; set; }
}
