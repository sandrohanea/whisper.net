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

    internal static void InitializeLogging()
    {
        IntPtr funcPointer;
#if NET6_0_OR_GREATER
        unsafe
        {
            delegate* unmanaged[Cdecl]<GgmlLogLevel, IntPtr, IntPtr, void> onLogging = &LogUnmanaged;
            funcPointer = (IntPtr)onLogging;
        }
#else
        funcPointer = Marshal.GetFunctionPointerForDelegate(logCallback);
#endif
        NativeMethods.whisper_log_set(funcPointer, IntPtr.Zero);
        GgmlNativeMethods.ggml_log_set(funcPointer, IntPtr.Zero);
    }

#if NET6_0_OR_GREATER
    [UnmanagedCallersOnly(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
# else

    private static readonly WhisperGgmlLogCallback logCallback = LogUnmanaged;
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
