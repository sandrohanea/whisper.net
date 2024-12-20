// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net.Logger;
public static class LogProvider
{
    public static event Action<WhisperLogLevel, string?>? OnLog;

    /// <summary>
    /// Adds a console logger that logs messages with a severity greater than or equal to the specified level.
    /// </summary>
    /// <param name="minLevel">The minimum severity level to log.</param>
    public static void AddConsoleLogging(WhisperLogLevel minLevel = WhisperLogLevel.Info)
    {
        OnLog += (level, message) =>
        {
            // Higher values are less severe
            if (level < minLevel)
            {
                Console.WriteLine($"[{level}] {message}");
            }
        };
    }

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
        nativeWhisper.Ggml_log_set(funcPointer, IntPtr.Zero);
        nativeWhisper.Whisper_Log_Set(funcPointer, IntPtr.Zero);
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
        OnLog?.Invoke(level, message);
    }
}
