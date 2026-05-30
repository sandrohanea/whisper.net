// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.ModelLoader;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net;

/// <summary>
/// A builder for creating a <see cref="WhisperVadProcessor"/>.
/// </summary>
public sealed class WhisperVadProcessorBuilder
{
    private readonly IWhisperProcessorModelLoader loader;
    private readonly INativeWhisper nativeWhisper;
    private WhisperVadContextParams contextParams;
    private WhisperVadParams vadParams;

    internal WhisperVadProcessorBuilder(IWhisperProcessorModelLoader loader, INativeWhisper nativeWhisper)
    {
        this.loader = loader;
        this.nativeWhisper = nativeWhisper;
        contextParams = nativeWhisper.Whisper_Vad_Default_Context_Params();
        vadParams = nativeWhisper.Whisper_Vad_Default_Params();
    }

    /// <summary>
    /// Sets the number of threads used by the VAD processor.
    /// </summary>
    public WhisperVadProcessorBuilder WithThreads(int threads)
    {
        contextParams.NThreads = threads;
        return this;
    }

    /// <summary>
    /// Sets whether GPU should be used by the VAD processor.
    /// </summary>
    public WhisperVadProcessorBuilder WithUseGpu(bool useGpu)
    {
        contextParams.UseGpu = useGpu.AsByte();
        return this;
    }

    /// <summary>
    /// Sets the GPU device used by the VAD processor.
    /// </summary>
    public WhisperVadProcessorBuilder WithGpuDevice(int gpuDevice)
    {
        contextParams.GpuDevice = gpuDevice;
        return this;
    }

    /// <summary>
    /// Sets the probability threshold to consider audio as speech.
    /// </summary>
    public WhisperVadProcessorBuilder WithThreshold(float threshold)
    {
        vadParams.Threshold = threshold;
        return this;
    }

    /// <summary>
    /// Sets the minimum duration for a valid speech segment.
    /// </summary>
    public WhisperVadProcessorBuilder WithMinSpeechDuration(TimeSpan duration)
    {
        vadParams.MinSpeechDurationMs = (int)duration.TotalMilliseconds;
        return this;
    }

    /// <summary>
    /// Sets the minimum silence duration needed to consider speech as ended.
    /// </summary>
    public WhisperVadProcessorBuilder WithMinSilenceDuration(TimeSpan duration)
    {
        vadParams.MinSilenceDurationMs = (int)duration.TotalMilliseconds;
        return this;
    }

    /// <summary>
    /// Sets the maximum duration of a speech segment before forcing a new segment.
    /// </summary>
    public WhisperVadProcessorBuilder WithMaxSpeechDuration(TimeSpan duration)
    {
        vadParams.MaxSpeechDurationS = (float)duration.TotalSeconds;
        return this;
    }

    /// <summary>
    /// Sets the padding added before and after speech segments.
    /// </summary>
    public WhisperVadProcessorBuilder WithSpeechPadding(TimeSpan padding)
    {
        vadParams.SpeechPaddingMs = (int)padding.TotalMilliseconds;
        return this;
    }

    /// <summary>
    /// Sets the overlap used when copying audio samples from speech segments.
    /// </summary>
    public WhisperVadProcessorBuilder WithSamplesOverlap(TimeSpan overlap)
    {
        vadParams.SampleOverlapS = (float)overlap.TotalSeconds;
        return this;
    }

    /// <summary>
    /// Builds a processor used to run voice activity detection.
    /// </summary>
    public WhisperVadProcessor Build()
    {
        var context = loader.LoadNativeVadContext(nativeWhisper, contextParams);
        if (context == IntPtr.Zero)
        {
            throw new WhisperModelLoadException("Failed to load the whisper VAD model.");
        }

        return new WhisperVadProcessor(new WhisperVadProcessorOptions
        {
            ContextHandle = context,
            VadParams = vadParams
        }, nativeWhisper);
    }
}
