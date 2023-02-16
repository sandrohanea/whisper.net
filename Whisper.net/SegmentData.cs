// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

public class SegmentData
{
    public SegmentData(string? segment, TimeSpan start, TimeSpan end)
    {
        Segment = segment;
        Start = start;
        End = end;
    }

    public string? Segment { get; }

    public TimeSpan Start { get; }

    public TimeSpan End { get; }
}
