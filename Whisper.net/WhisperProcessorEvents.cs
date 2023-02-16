namespace Whisper.net;

public delegate void OnSegmentEventHandler(OnSegmentEventArgs e);

public delegate bool OnEncoderBeginEventHandler(OnEncoderBeginEventArgs e);

public class OnEncoderBeginEventArgs
{
      
}

public class OnSegmentEventArgs
{
    public OnSegmentEventArgs(string segment, TimeSpan start, TimeSpan end)
    {
        Segment = segment;
        Start = start;
        End = end;
    }

    public string Segment { get; }
    
    public TimeSpan Start { get; }
    
    public TimeSpan End { get; }
}