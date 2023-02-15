namespace Whisper.net
{
    public class WhisperProcessResult
    {
        public IEnumerable<OnSegmentEventArgs> Segments { get; internal set; }

        public string? Language { get; internal set; }
    }
}
