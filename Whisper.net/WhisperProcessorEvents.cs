// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

public delegate void OnSegmentEventHandler(SegmentData e);

public delegate bool OnEncoderBeginEventHandler(EncoderBeginData e);

public class EncoderBeginData
{

}

public class SegmentData
{
    public SegmentData(string text, TimeSpan start, TimeSpan end)
    {
        Text = text;
        Start = start;
        End = end;
    }

    public string Text { get; }

    public TimeSpan Start { get; }

    public TimeSpan End { get; }
}
