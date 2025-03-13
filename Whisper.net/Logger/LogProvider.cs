// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net.Logger;
public static class LogProvider
{
    /// <summary>
    /// Adds a console logger that logs messages with a severity greater than or equal to the specified level.
    /// </summary>
    /// <param name="minLevel">The minimum severity level to log.</param>
    /// <returns>
    /// Returns a disposable object that can be used to remove the logger.
    /// </returns>
    public static IDisposable AddConsoleLogging(WhisperLogLevel minLevel = WhisperLogLevel.Info)
    {
        return new WhisperLogger((level, message) =>
        {
            // Higher values are less severe
            if (level <= minLevel)
            {
                Console.WriteLine($"[{level}] {message}");
            }
        });
    }

    /// <summary>
    /// Adds a logger that logs messages with a custom action.
    /// </summary>
    /// <param name="logAction">The action to log.</param>
    /// <returns>
    /// Returns a disposable object that can be used to remove the logger.
    /// </returns>
    public static IDisposable AddLogger(Action<WhisperLogLevel, string?> logAction)
    {
        return new WhisperLogger(logAction);
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

        WhisperLogger.Log(managedLevel, messageString);
    }
}
