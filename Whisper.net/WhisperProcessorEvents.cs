// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

public delegate void OnProgressHandler(int progress);

public delegate void OnSegmentEventHandler(SegmentData e);

public delegate bool OnEncoderBeginEventHandler(EncoderBeginData e);

public delegate bool WhisperAbortEventHandler();

public class EncoderBeginData
{

}

/// <summary>
/// Represents data about a recognized token.
/// </summary>
public class WhisperToken
{
    public int Id;
    public int TimestampId;
    public float Probability;
    public float ProbabilityLog;
    public float TimestampProbability;
    public float TimestampProbabilitySum;
    public long Start;
    public long End;
    public long DtwTimestamp;
    public float VoiceLen;
    public string? Text;
}

/// <summary>
/// Represents data about a recognized segment.
/// </summary>
/// <param name="text"></param>
/// <param name="start"></param>
/// <param name="end"></param>
/// <param name="minProbability"></param>
/// <param name="maxProbability"></param>
/// <param name="probability"></param>
/// <param name="language"></param>
public class SegmentData(
    string text,
    TimeSpan start,
    TimeSpan end,
    float minProbability,
    float maxProbability,
    float probability,
    float noSpeechProbability,
    string language,
    WhisperToken[] tokens)
{

    /// <summary>
    /// Gets the text of the segment.
    /// </summary>
    public string Text { get; } = text;

    /// <summary>
    /// Gets the time when the segment started.
    /// </summary>
    public TimeSpan Start { get; } = start;

    /// <summary>
    /// Gets the time when the segment ended.
    /// </summary>
    public TimeSpan End { get; } = end;

    /// <summary>
    /// Gets the minimum probability found in any of the tokens.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1
    /// </remarks>
    public float MinProbability { get; } = minProbability;

    /// <summary>
    /// Gets the maximum probability found in any of the tokens.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1
    /// </remarks>
    public float MaxProbability { get; } = maxProbability;

    /// <summary>
    /// Gets the average probability of the segment.
    /// </summary>
    /// <remarks>
    /// The possible values are from 0 to 1
    /// </remarks>
    public float Probability { get; } = probability;

    /// <summary>
    /// Gets the language of the current segment.
    /// </summary>
    public string Language { get; } = language;

    /// <summary>
    /// The tokens of the current segment.
    /// </summary>
    public WhisperToken[] Tokens { get; } = tokens;

    public float NoSpeechProbability { get; } = noSpeechProbability;
}
