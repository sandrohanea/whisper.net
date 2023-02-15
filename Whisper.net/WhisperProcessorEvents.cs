namespace Whisper.net;

public delegate void OnSegmentEventHandler(object sender, OnSegmentEventArgs e);

public delegate void OnEncoderBeginEventHandler(object sender, OnEncoderBeginEventArgs e);

public class OnEncoderBeginEventArgs : EventArgs
{
      
}

public class OnSegmentEventArgs : EventArgs
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