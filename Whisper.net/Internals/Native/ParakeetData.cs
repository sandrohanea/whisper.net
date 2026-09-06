// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Native;

internal enum ParakeetSamplingStrategy
{
    Greedy
}

[StructLayout(LayoutKind.Sequential)]
internal struct ParakeetContextParams
{
    public byte UseGpu;
    public int GpuDevice;
}

[StructLayout(LayoutKind.Sequential)]
internal struct ParakeetFullParams
{
    public ParakeetSamplingStrategy Strategy;
    public int Threads;
    public int OffsetMs;
    public int DurationMs;
    public byte NoContext;
    public int AudioContextSize;
    public IntPtr OnNewSegment;
    public IntPtr OnNewSegmentUserData;
    public IntPtr OnNewToken;
    public IntPtr OnNewTokenUserData;
    public IntPtr OnProgress;
    public IntPtr OnProgressUserData;
    public IntPtr OnEncoderBegin;
    public IntPtr OnEncoderBeginUserData;
    public IntPtr OnAbort;
    public IntPtr OnAbortUserData;
}

[StructLayout(LayoutKind.Sequential)]
internal struct ParakeetTokenData
{
    public int Id;
    public int DurationIndex;
    public int DurationValue;
    public int FrameIndex;
    public float Probability;
    public float ProbabilityLog;
    public long Start;
    public long End;
    public byte IsWordStart;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void ParakeetNewSegmentCallback(IntPtr context, IntPtr state, int newSegmentCount, IntPtr userData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate void ParakeetProgressCallback(IntPtr context, IntPtr state, int progress, IntPtr userData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate byte ParakeetEncoderBeginCallback(IntPtr context, IntPtr state, IntPtr userData);

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal delegate byte ParakeetAbortCallback(IntPtr userData);
