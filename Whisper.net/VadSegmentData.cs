// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Represents a speech segment detected by the VAD processor.
/// </summary>
/// <param name="start">The time when speech starts.</param>
/// <param name="end">The time when speech ends.</param>
public sealed class VadSegmentData(TimeSpan start, TimeSpan end)
{
    /// <summary>
    /// Gets the time when speech starts.
    /// </summary>
    public TimeSpan Start { get; } = start;

    /// <summary>
    /// Gets the time when speech ends.
    /// </summary>
    public TimeSpan End { get; } = end;
}
