// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.Native;
using Whisper.net.Wave;

namespace Whisper.net;

/// <summary>
/// Represents a processor that can detect speech segments in audio input.
/// </summary>
public sealed class WhisperVadProcessor : IAsyncDisposable, IDisposable
{
    private readonly INativeWhisper nativeWhisper;
    private readonly WhisperVadProcessorOptions options;
    private readonly SemaphoreSlim processingSemaphore = new(1);
    private IntPtr contextHandle;
    private bool isDisposed;

    internal WhisperVadProcessor(WhisperVadProcessorOptions options, INativeWhisper nativeWhisper)
    {
        this.options = options;
        this.nativeWhisper = nativeWhisper;
        contextHandle = options.ContextHandle;
    }

    /// <summary>
    /// Detects speech segments in a wave stream.
    /// </summary>
    public IReadOnlyList<VadSegmentData> DetectSpeech(Stream waveStream)
    {
        var waveParser = new WaveParser(waveStream);
        return DetectSpeech(waveParser.GetAvgSamples());
    }

    /// <summary>
    /// Detects speech segments in a wave stream without resetting the VAD state.
    /// </summary>
    public IReadOnlyList<VadSegmentData> DetectSpeechNoReset(Stream waveStream)
    {
        var waveParser = new WaveParser(waveStream);
        return DetectSpeechNoReset(waveParser.GetAvgSamples());
    }

    /// <summary>
    /// Detects speech segments in audio samples.
    /// </summary>
    public IReadOnlyList<VadSegmentData> DetectSpeech(float[] samples)
    {
        return DetectSpeech(samples.AsSpan());
    }

    /// <summary>
    /// Detects speech segments in audio samples without resetting the VAD state.
    /// </summary>
    public IReadOnlyList<VadSegmentData> DetectSpeechNoReset(float[] samples)
    {
        return DetectSpeechNoReset(samples.AsSpan());
    }

    /// <summary>
    /// Detects speech segments in audio samples.
    /// </summary>
    public IReadOnlyList<VadSegmentData> DetectSpeech(ReadOnlySpan<float> samples)
    {
        return DetectSpeechCore(samples, resetState: true);
    }

    /// <summary>
    /// Detects speech segments in audio samples without resetting the VAD state.
    /// </summary>
    public IReadOnlyList<VadSegmentData> DetectSpeechNoReset(ReadOnlySpan<float> samples)
    {
        return DetectSpeechCore(samples, resetState: false);
    }

    /// <summary>
    /// Detects speech segments in a wave stream.
    /// </summary>
    public async Task<IReadOnlyList<VadSegmentData>> DetectSpeechAsync(Stream waveStream, CancellationToken cancellationToken = default)
    {
        var waveParser = new WaveParser(waveStream);
        var samples = await waveParser.GetAvgSamplesAsync(cancellationToken).ConfigureAwait(false);
        return await DetectSpeechAsync(samples, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Detects speech segments in a wave stream without resetting the VAD state.
    /// </summary>
    public async Task<IReadOnlyList<VadSegmentData>> DetectSpeechNoResetAsync(Stream waveStream, CancellationToken cancellationToken = default)
    {
        var waveParser = new WaveParser(waveStream);
        var samples = await waveParser.GetAvgSamplesAsync(cancellationToken).ConfigureAwait(false);
        return await DetectSpeechNoResetAsync(samples, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Detects speech segments in audio samples.
    /// </summary>
    public Task<IReadOnlyList<VadSegmentData>> DetectSpeechAsync(float[] samples, CancellationToken cancellationToken = default)
    {
        return DetectSpeechAsync(samples.AsMemory(), cancellationToken);
    }

    /// <summary>
    /// Detects speech segments in audio samples without resetting the VAD state.
    /// </summary>
    public Task<IReadOnlyList<VadSegmentData>> DetectSpeechNoResetAsync(float[] samples, CancellationToken cancellationToken = default)
    {
        return DetectSpeechNoResetAsync(samples.AsMemory(), cancellationToken);
    }

    /// <summary>
    /// Detects speech segments in audio samples.
    /// </summary>
    public Task<IReadOnlyList<VadSegmentData>> DetectSpeechAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default)
    {
        return DetectSpeechAsync(samples, resetState: true, cancellationToken);
    }

    /// <summary>
    /// Detects speech segments in audio samples without resetting the VAD state.
    /// </summary>
    public Task<IReadOnlyList<VadSegmentData>> DetectSpeechNoResetAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken = default)
    {
        return DetectSpeechAsync(samples, resetState: false, cancellationToken);
    }

    /// <summary>
    /// Resets the VAD state.
    /// </summary>
    public void ResetState()
    {
        ThrowIfDisposed();

        processingSemaphore.Wait();
        try
        {
            ThrowIfDisposed();
            nativeWhisper.Whisper_Vad_Reset_State(contextHandle);
        }
        finally
        {
            processingSemaphore.Release();
        }
    }

    private Task<IReadOnlyList<VadSegmentData>> DetectSpeechAsync(ReadOnlyMemory<float> samples, bool resetState, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();

        return Task.Factory.StartNew(() =>
            DetectSpeechCore(samples.Span, resetState, cancellationToken),
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        if (processingSemaphore.CurrentCount == 0)
        {
            throw new Exception("Cannot dispose while processing, please use DisposeAsync instead.");
        }

        DisposeContext();
        isDisposed = true;
        processingSemaphore.Dispose();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (isDisposed)
        {
            return;
        }

        await processingSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            DisposeContext();
            isDisposed = true;
        }
        finally
        {
            processingSemaphore.Dispose();
        }
    }

    private unsafe IReadOnlyList<VadSegmentData> DetectSpeechCore(ReadOnlySpan<float> samples, bool resetState, CancellationToken? cancellationToken = null)
    {
        ThrowIfDisposed();

        if (cancellationToken.HasValue)
        {
            processingSemaphore.Wait(cancellationToken.Value);
        }
        else
        {
            processingSemaphore.Wait();
        }

        try
        {
            ThrowIfDisposed();

            fixed (float* pData = samples)
            {
                var result = resetState
                    ? nativeWhisper.Whisper_Vad_Detect_Speech(contextHandle, (IntPtr)pData, samples.Length)
                    : nativeWhisper.Whisper_Vad_Detect_Speech_No_Reset(contextHandle, (IntPtr)pData, samples.Length);
                if (result == 0)
                {
                    throw new WhisperProcessingException(-1);
                }

                return GetSegments();
            }
        }
        finally
        {
            processingSemaphore.Release();
        }
    }

    private IReadOnlyList<VadSegmentData> GetSegments()
    {
        var segmentsHandle = nativeWhisper.Whisper_Vad_Segments_From_Probs(contextHandle, options.VadParams);
        if (segmentsHandle == IntPtr.Zero)
        {
            throw new WhisperProcessingException(-1);
        }

        try
        {
            var segmentsCount = nativeWhisper.Whisper_Vad_Segments_N_Segments(segmentsHandle);
            var segments = new VadSegmentData[segmentsCount];
            for (var i = 0; i < segmentsCount; i++)
            {
                var start = nativeWhisper.Whisper_Vad_Segments_Get_Segment_T0(segmentsHandle, i);
                var end = nativeWhisper.Whisper_Vad_Segments_Get_Segment_T1(segmentsHandle, i);
                segments[i] = new VadSegmentData(TimeSpan.FromSeconds(start / 100.0), TimeSpan.FromSeconds(end / 100.0));
            }

            return segments;
        }
        finally
        {
            nativeWhisper.Whisper_Vad_Free_Segments(segmentsHandle);
        }
    }

    private void DisposeContext()
    {
        if (contextHandle == IntPtr.Zero)
        {
            return;
        }

        nativeWhisper.Whisper_Vad_Free(contextHandle);
        contextHandle = IntPtr.Zero;
    }

    private void ThrowIfDisposed()
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException("This VAD processor has already been disposed.");
        }
    }
}
