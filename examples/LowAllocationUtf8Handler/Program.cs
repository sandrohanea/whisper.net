// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.Buffers.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Wave;

public class Program
{
    private static readonly Stream StandardOutput = Console.OpenStandardOutput();
    private static readonly Utf8SegmentHandler SegmentHandler = WriteSegment;
    private static long handlerAllocatedBytes;
    private static int handledSegments;

    public static async Task Main()
    {
        const string modelFileName = "ggml-base.bin";
        const string wavFileName = "kennedy.wav";

        if (!File.Exists(modelFileName))
        {
            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
            using var fileWriter = File.OpenWrite(modelFileName);
            await modelStream.CopyToAsync(fileWriter);
        }

        using var whisperFactory = WhisperFactory.FromPath(modelFileName);
        await using var processor = whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .Build();

        using var fileStream = File.OpenRead(wavFileName);
        var samples = await new WaveParser(fileStream).GetAvgSamplesAsync();

        // Run once so JIT and native initialization allocations are not attributed to steady-state processing.
        StandardOutput.Write("Warming up...\n"u8);
        processor.ProcessWithUtf8Handler(samples, static _ => { });

        // Collect first so the counters below describe only the measured processing call.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var before = GcSnapshot.Capture();

        Volatile.Write(ref handlerAllocatedBytes, 0);
        Volatile.Write(ref handledSegments, 0);
        processor.ProcessWithUtf8Handler(samples, SegmentHandler);

        var after = GcSnapshot.Capture();
        WriteSnapshot("Before"u8, before);
        WriteSnapshot("After "u8, after);
        WriteDelta(before, after);
        WriteHandlerMeasurements();
    }

    private static void WriteSegment(Utf8SegmentData segment)
    {
        var allocatedBytesBefore = GC.GetAllocatedBytesForCurrentThread();

        // Write the borrowed UTF-8 bytes directly. Decoding them into a string would allocate.
        StandardOutput.Write(segment.TextUtf8);
        StandardOutput.Write("\n"u8);

        Interlocked.Add(
            ref handlerAllocatedBytes,
            GC.GetAllocatedBytesForCurrentThread() - allocatedBytesBefore);
        Interlocked.Increment(ref handledSegments);
    }

    private static void WriteSnapshot(ReadOnlySpan<byte> label, GcSnapshot snapshot)
    {
        StandardOutput.Write(label);
        StandardOutput.Write(": Gen0="u8);
        WriteNumber(snapshot.Gen0Collections);
        StandardOutput.Write(", Gen1="u8);
        WriteNumber(snapshot.Gen1Collections);
        StandardOutput.Write(", Gen2="u8);
        WriteNumber(snapshot.Gen2Collections);
        StandardOutput.Write(", allocated bytes="u8);
        WriteNumber(snapshot.CurrentThreadAllocatedBytes);
        StandardOutput.Write("\n"u8);
    }

    private static void WriteDelta(GcSnapshot before, GcSnapshot after)
    {
        // The synchronous overload avoids Task and worker-thread allocations.
        StandardOutput.Write("Process delta: Gen0="u8);
        WriteNumber(after.Gen0Collections - before.Gen0Collections);
        StandardOutput.Write(", Gen1="u8);
        WriteNumber(after.Gen1Collections - before.Gen1Collections);
        StandardOutput.Write(", Gen2="u8);
        WriteNumber(after.Gen2Collections - before.Gen2Collections);
        StandardOutput.Write(", allocated bytes="u8);
        WriteNumber(after.CurrentThreadAllocatedBytes - before.CurrentThreadAllocatedBytes);
        StandardOutput.Write("\n"u8);
    }

    private static void WriteHandlerMeasurements()
    {
        StandardOutput.Write("Handler: segments="u8);
        WriteNumber(Volatile.Read(ref handledSegments));
        StandardOutput.Write(", allocated bytes="u8);
        WriteNumber(Volatile.Read(ref handlerAllocatedBytes));
        StandardOutput.Write("\n"u8);
    }

    private static void WriteNumber(long value)
    {
        Span<byte> buffer = stackalloc byte[32];
        Utf8Formatter.TryFormat(value, buffer, out var bytesWritten);
        StandardOutput.Write(buffer[..bytesWritten]);
    }

    private readonly record struct GcSnapshot(
        int Gen0Collections,
        int Gen1Collections,
        int Gen2Collections,
        long CurrentThreadAllocatedBytes)
    {
        public static GcSnapshot Capture()
        {
            return new GcSnapshot(
                GC.CollectionCount(0),
                GC.CollectionCount(1),
                GC.CollectionCount(2),
                GC.GetAllocatedBytesForCurrentThread());
        }
    }
}
