// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

public delegate void OnProgressHandler(int progress);

public delegate void OnSegmentEventHandler(SegmentData e);

public delegate bool OnEncoderBeginEventHandler(EncoderBeginData e);

public delegate bool WhisperAbortEventHandler();

public class EncoderBeginData
{

}

public class SegmentData
{
    public SegmentData(string text, TimeSpan start, TimeSpan end, float minProbability, float maxProbability, float probability, string language)
    {
        Text = text;
        Start = start;
        End = end;
        MinProbability = minProbability;
        MaxProbability = maxProbability;
        Probability = probability;
        Language = language;
    }

    /// <summary>
    /// Gets the text of the segment.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the time when the segment started.
    /// </summary>
    public TimeSpan Start { get; }

    /// <summary>
    /// Gets the time when the segment ended.
    /// </summary>
    public TimeSpan End { get; }

    /// <summary>
    /// Gets the minimum probability found in any of the tokens.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1
    /// </remarks>
    public float MinProbability { get; }

    /// <summary>
    /// Gets the maximum probability found in any of the tokens.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1
    /// </remarks>
    public float MaxProbability { get; }

    /// <summary>
    /// Gets the average probability of the segment.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1
    /// </remarks>
    public float Probability { get; }

    /// <summary>
    /// Gets the language of the current segment.
    /// </summary>
    public string Language { get; }
}
