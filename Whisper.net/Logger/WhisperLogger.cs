// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Logger;

internal class WhisperLogger : IDisposable
{
    private readonly Action<WhisperLogLevel, string?> logAction;

    public WhisperLogger(Action<WhisperLogLevel, string?> logAction)
    {
        this.logAction = logAction;
        OnLog += logAction;
    }

    public static event Action<WhisperLogLevel, string?>? OnLog;

    public void Dispose()
    {
        OnLog -= logAction;
    }

    public static void Log(WhisperLogLevel level, string? message)
    {
        OnLog?.Invoke(level, message);
    }
}
