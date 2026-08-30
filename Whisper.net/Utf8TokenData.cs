// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Native;

namespace Whisper.net;

/// <summary>
/// Provides a borrowed view of a recognized token and its UTF-8 text.
/// </summary>
/// <remarks>
/// Instances and their spans are valid only while the enclosing <see cref="Utf8SegmentHandler"/> callback is running.
/// </remarks>
public readonly ref struct Utf8TokenData
{
    internal Utf8TokenData(WhisperTokenData tokenData, ReadOnlySpan<byte> textUtf8)
    {
        Id = tokenData.id;
        TimestampId = tokenData.tid;
        Probability = tokenData.p;
        ProbabilityLog = tokenData.plog;
        TimestampProbability = tokenData.pt;
        TimestampProbabilitySum = tokenData.ptsum;
        Start = tokenData.t0;
        End = tokenData.t1;
        DtwTimestamp = tokenData.t_dtw;
        VoiceLen = tokenData.vlen;
        TextUtf8 = textUtf8;
    }

    public int Id { get; }

    public int TimestampId { get; }

    public float Probability { get; }

    public float ProbabilityLog { get; }

    public float TimestampProbability { get; }

    public float TimestampProbabilitySum { get; }

    public long Start { get; }

    public long End { get; }

    public long DtwTimestamp { get; }

    public float VoiceLen { get; }

    /// <summary>
    /// Gets the token text as borrowed UTF-8 bytes.
    /// </summary>
    public ReadOnlySpan<byte> TextUtf8 { get; }
}
