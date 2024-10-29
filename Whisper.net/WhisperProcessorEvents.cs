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
/// <param name="id"></param>
/// <param name="Tid"></param>
/// <param name="P"></param>
/// <param name="Plog"></param>
/// <param name="Pt"></param>
/// <param name="PtSum"></param>
/// <param name="T0"></param>
/// <param name="T1"></param>
/// <param name="T_Dtw"></param>
/// <param name="Vlen"></param>
/// <param name="text"></param>
public class WhisperToken
{
    public int Id;
    public int Tid;
    public float P;
    public float Plog;
    public float Pt;
    public float PtSum;
    public long T0;
    public long T1;
    public long T_Dtw;
    public float Vlen;
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
public class SegmentData(string text, TimeSpan start, TimeSpan end, float minProbability, float maxProbability, float probability, string language, WhisperToken[] tokens)
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
}
