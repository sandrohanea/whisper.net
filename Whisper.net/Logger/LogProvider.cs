// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net.Logger;
public class LogProvider
{

    private LogProvider()
    {

    }

    /// <summary>
    /// Returns the singleton instance of the <see cref="LogProvider"/> class used to log messages from the Whisper library.
    /// </summary>
    public static LogProvider Instance { get; } = new();

    public event Action<WhisperLogLevel, string?>? OnLog;

    internal static void InitializeLogging(INativeWhisper nativeWhisper)
    {
        IntPtr funcPointer;
#if NETSTANDARD
        funcPointer = Marshal.GetFunctionPointerForDelegate(logCallback);
#else
        unsafe
        {
            delegate* unmanaged[Cdecl]<GgmlLogLevel, IntPtr, IntPtr, void> onLogging = &LogUnmanaged;
            funcPointer = (IntPtr)onLogging;
        }
#endif
        nativeWhisper.Whisper_Log_Set(funcPointer, IntPtr.Zero);
        nativeWhisper.Ggml_log_set(funcPointer, IntPtr.Zero);
    }

#if NETSTANDARD
    private static readonly WhisperGgmlLogCallback logCallback = LogUnmanaged;
# else
    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
#endif
    internal static void LogUnmanaged(GgmlLogLevel level, IntPtr message, IntPtr user_data)
    {
        var messageString = Marshal.PtrToStringAnsi(message);
        var managedLevel = level switch
        {
            GgmlLogLevel.Error => WhisperLogLevel.Error,
            GgmlLogLevel.Warning => WhisperLogLevel.Warning,
            _ => WhisperLogLevel.Info
        };

        Log(managedLevel, messageString);
    }

    internal static void Log(WhisperLogLevel level, string? message)
    {
        Instance.OnLog?.Invoke(level, message);
    }
}
