// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals;
using Whisper.net.Internals.Native;

namespace Whisper.net;

/// <summary>
/// Provides a borrowed view of a recognized segment and its UTF-8 text.
/// </summary>
/// <remarks>
/// Instances and their spans are valid only while the <see cref="Utf8SegmentHandler"/> callback is running.
/// Copy or decode any data that needs to be retained after the callback returns.
/// </remarks>
public readonly ref struct Utf8SegmentData
{
    private readonly IntPtr context;
    private readonly IntPtr state;
    private readonly int segmentIndex;
    private readonly INativeWhisper nativeWhisper;

    internal Utf8SegmentData(
        IntPtr context,
        IntPtr state,
        int segmentIndex,
        INativeWhisper nativeWhisper,
        bool computeProbabilities)
    {
        this.context = context;
        this.state = state;
        this.segmentIndex = segmentIndex;
        this.nativeWhisper = nativeWhisper;

        TextUtf8 = MarshalUtils.GetUtf8Span(
            nativeWhisper.Whisper_Full_Get_Segment_Text_From_State(state, segmentIndex));
        Start = TimeSpan.FromMilliseconds(
            nativeWhisper.Whisper_Full_Get_Segment_T0_From_State(state, segmentIndex) * 10);
        End = TimeSpan.FromMilliseconds(
            nativeWhisper.Whisper_Full_Get_Segment_T1_From_State(state, segmentIndex) * 10);
        NoSpeechProbability = nativeWhisper.Whisper_Full_Get_Segment_No_Speech_Prob_From_State(state, segmentIndex);
        TokenCount = nativeWhisper.Whisper_Full_N_Tokens_From_State(state, segmentIndex);

        LanguageId = nativeWhisper.Whisper_Full_Lang_Id_From_State(state);
        LanguageUtf8 = MarshalUtils.GetUtf8Span(nativeWhisper.Whisper_Lang_Str(LanguageId));

        var minimumProbability = 0f;
        var maximumProbability = 0f;
        var sumProbability = 0d;

        if (computeProbabilities)
        {
            for (var tokenIndex = 0; tokenIndex < TokenCount; tokenIndex++)
            {
                var tokenProbability = nativeWhisper.Whisper_Full_Get_Token_P_From_State(
                    state,
                    segmentIndex,
                    tokenIndex);
                sumProbability += tokenProbability;

                if (tokenIndex == 0)
                {
                    minimumProbability = tokenProbability;
                    maximumProbability = tokenProbability;
                }
                else
                {
                    minimumProbability = Math.Min(minimumProbability, tokenProbability);
                    maximumProbability = Math.Max(maximumProbability, tokenProbability);
                }
            }
        }

        MinProbability = minimumProbability;
        MaxProbability = maximumProbability;
        Probability = TokenCount == 0 ? 0f : (float)(sumProbability / TokenCount);
    }

    /// <summary>
    /// Gets the segment text as borrowed UTF-8 bytes.
    /// </summary>
    public ReadOnlySpan<byte> TextUtf8 { get; }

    /// <summary>
    /// Gets the time when the segment started.
    /// </summary>
    public TimeSpan Start { get; }

    /// <summary>
    /// Gets the time when the segment ended.
    /// </summary>
    public TimeSpan End { get; }

    /// <summary>
    /// Gets the minimum token probability when probability computation is enabled.
    /// </summary>
    public float MinProbability { get; }

    /// <summary>
    /// Gets the maximum token probability when probability computation is enabled.
    /// </summary>
    public float MaxProbability { get; }

    /// <summary>
    /// Gets the average token probability when probability computation is enabled.
    /// </summary>
    public float Probability { get; }

    /// <summary>
    /// Gets the no-speech probability for the segment.
    /// </summary>
    public float NoSpeechProbability { get; }

    /// <summary>
    /// Gets the detected language identifier.
    /// </summary>
    public int LanguageId { get; }

    /// <summary>
    /// Gets the detected language as borrowed UTF-8 bytes.
    /// </summary>
    public ReadOnlySpan<byte> LanguageUtf8 { get; }

    /// <summary>
    /// Gets the number of tokens in the segment.
    /// </summary>
    public int TokenCount { get; }

    /// <summary>
    /// Gets a borrowed view of a token.
    /// </summary>
    /// <param name="tokenIndex">The zero-based token index.</param>
    public Utf8TokenData GetToken(int tokenIndex)
    {
        if ((uint)tokenIndex >= (uint)TokenCount)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenIndex));
        }

        var tokenData = nativeWhisper.Whisper_Full_Get_Token_Data_From_State(
            state,
            segmentIndex,
            tokenIndex);
        var textUtf8 = MarshalUtils.GetUtf8Span(
            nativeWhisper.Whisper_Full_Get_Token_Text_From_State(
                context,
                state,
                segmentIndex,
                tokenIndex));

        return new Utf8TokenData(tokenData, textUtf8);
    }
}
