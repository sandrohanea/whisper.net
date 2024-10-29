// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Whisper.net.Internals;
using Whisper.net.Internals.Native;
using Whisper.net.LibraryLoader;
using Whisper.net.Native;
using Whisper.net.SamplingStrategy;
using Whisper.net.Wave;

namespace Whisper.net;

/// <summary>
/// Represents a processor that can transcribe or translate audio input.
/// </summary>
public sealed class WhisperProcessor : IAsyncDisposable, IDisposable
{
    private static readonly ConcurrentDictionary<long, WhisperProcessor> processorInstances = new();
    private static long currentProcessorId;
    private const byte trueByte = 1;
    private const byte falseByte = 0;

    private readonly IntPtr currentWhisperContext;
    private readonly WhisperProcessorOptions options;
    private readonly INativeWhisper nativeWhisper;
    private readonly List<GCHandle> gcHandles = [];
    private readonly SemaphoreSlim processingSemaphore;
    private WhisperFullParams whisperParams;
    private IntPtr? language;
    private IntPtr? initialPromptText;
    private IntPtr? suppressRegex;
    private bool isDisposed;
    private int segmentIndex;
    private CancellationToken? currentCancellationToken;

    // Id is used to identify the current instance when calling the callbacks from C++
    private readonly long myId;

    internal WhisperProcessor(WhisperProcessorOptions options, INativeWhisper nativeWhisper)
    {
        this.options = options;
        this.nativeWhisper = nativeWhisper;
        myId = Interlocked.Increment(ref currentProcessorId);

        processorInstances[myId] = this;

        currentWhisperContext = options.ContextHandle;
        whisperParams = GetWhisperParams();
        processingSemaphore = new(1);
    }

    /// <summary>
    /// Change the language that is used to process the audio input.
    /// </summary>
    /// <param name="newLanguage"></param>
    public void ChangeLanguage(string? newLanguage)
    {
        var oldLanguage = language;

        var newParams = whisperParams;
        if (string.IsNullOrEmpty(newLanguage))
        {
            newParams.Language = IntPtr.Zero;
        }
        else
        {
            language = Marshal.StringToHGlobalAnsi(newLanguage);
            newParams.Language = language.Value;
        }

        if (oldLanguage.HasValue)
        {
            Marshal.FreeHGlobal(oldLanguage.Value);
        }
        whisperParams = newParams;
    }

    /// <summary>
    /// For the given audio input, detects the most probable language.
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    public unsafe string? DetectLanguage(float[] samples)
    {
        var (language, _) = DetectLanguageWithProbability(samples.AsSpan());
        return language;
    }

    /// <summary>
    /// For the given audio input, detects the most probable language and also returns the probability of this language to be correct.
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    public (string? language, float probability) DetectLanguageWithProbability(float[] samples)
    {
        return DetectLanguageWithProbability(samples.AsSpan());
    }

    /// <summary>
    /// For the given audio input, detects the most probable language and also returns the probability of this language to be correct.
    /// </summary>
    /// <param name="samples"></param>
    /// <returns></returns>
    public unsafe (string? language, float probability) DetectLanguageWithProbability(ReadOnlySpan<float> samples)
    {
        var probs = new float[nativeWhisper.Whisper_Lang_Max_Id()];

        fixed (float* pData = probs)
        {
            var state = GetWhisperState();
            try
            {
                fixed (float* pSamples = samples)
                {
                    nativeWhisper.Whisper_PCM_To_Mel_With_State(currentWhisperContext, state, (IntPtr)pSamples, samples.Length, whisperParams.Threads);
                }
                var langId = nativeWhisper.Whisper_Lang_Auto_Detect_With_State(currentWhisperContext, state, 0, whisperParams.Threads, (IntPtr)pData);
                if (langId == -1)
                {
                    return (null, 0f);
                }
                var languagePtr = nativeWhisper.Whisper_Lang_Str(langId);
                var language = Marshal.PtrToStringAnsi(languagePtr);
                return (language, probs[langId]);
            }
            finally
            {
                nativeWhisper.Whisper_Free_State(state);
            }
        }
    }

    /// <summary>
    /// Starts the synchronous processing.
    /// </summary>
    /// <param name="waveStream"></param>
    public void Process(Stream waveStream)
    {
        var waveParser = new WaveParser(waveStream);

        var samples = waveParser.GetAvgSamples();

        Process(samples);
    }

    /// <summary>
    /// Starts the synchronous processing.
    /// </summary>
    /// <param name="samples"></param>
    public void Process(float[] samples)
    {
        Process(samples.AsSpan());
    }

    /// <summary>
    /// Starts the synchronous processing.
    /// </summary>
    /// <param name="samples"></param>
    /// <exception cref="ObjectDisposedException"></exception>
    public unsafe void Process(ReadOnlySpan<float> samples)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException("This processor has already been disposed.");
        }

        fixed (float* pData = samples)
        {

            var state = GetWhisperState();
            try
            {
                processingSemaphore.Wait();
                segmentIndex = 0;

                nativeWhisper.Whisper_Full_With_State(currentWhisperContext, state, whisperParams, (IntPtr)pData, samples.Length);
            }
            finally
            {
                nativeWhisper.Whisper_Free_State(state);
                processingSemaphore.Release();
            }
        }
    }

    /// <summary>
    /// Starts the asynchronous processing.
    /// </summary>
    /// <param name="waveStream"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<SegmentData> ProcessAsync(Stream waveStream, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var waveParser = new WaveParser(waveStream);
        var samples = await waveParser.GetAvgSamplesAsync(cancellationToken);
        await foreach (var segmentData in ProcessAsync(samples, cancellationToken))
        {
            yield return segmentData;
        }
    }

    /// <summary>
    /// Starts the asynchronous processing.
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async IAsyncEnumerable<SegmentData> ProcessAsync(ReadOnlyMemory<float> samples, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var resetEvent = new AsyncAutoResetEvent();
        var buffer = new ConcurrentQueue<SegmentData>();

        void OnSegmentHandler(SegmentData segmentData)
        {
            buffer!.Enqueue(segmentData);
            resetEvent!.Set();
        }

        bool OnWhisperAbortHandler()
        {
            if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
            {
                return true;
            }
            return false;
        }

        try
        {
            options.OnSegmentEventHandlers.Add(OnSegmentHandler);
            options.WhisperAbortEventHandler = OnWhisperAbortHandler;

            currentCancellationToken = cancellationToken;
            var whisperTask = ProcessInternalAsync(samples, cancellationToken)
                .ContinueWith(_ => resetEvent.Set(), cancellationToken, TaskContinuationOptions.None, TaskScheduler.Default);

            while (!whisperTask.IsCompleted || !buffer.IsEmpty)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (buffer.IsEmpty)
                {
                    await Task.WhenAny(whisperTask, resetEvent.WaitAsync())
                        .ConfigureAwait(false);
                }

                while (!buffer.IsEmpty && buffer.TryDequeue(out var segmentData))
                {
                    yield return segmentData;
                }
            }

            await whisperTask.ConfigureAwait(false);

            while (buffer.TryDequeue(out var segmentData))
            {
                yield return segmentData;
            }
        }
        finally
        {
            options.OnSegmentEventHandlers.Remove(OnSegmentHandler);
        }
    }

    /// <summary>
    /// Starts the asynchronous processing.
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public IAsyncEnumerable<SegmentData> ProcessAsync(float[] samples, CancellationToken cancellationToken = default)
    {
        return ProcessAsync(samples.AsMemory(), cancellationToken);
    }

    public void Dispose()
    {
        if (processingSemaphore.CurrentCount == 0)
        {
            throw new Exception("Cannot dispose while processing, please use DisposeAsync instead.");
        }

        processorInstances.TryRemove(myId, out _);
        if (language.HasValue)
        {
            Marshal.FreeHGlobal(language.Value);
            language = null;
        }

        if (initialPromptText.HasValue)
        {
            Marshal.FreeHGlobal(initialPromptText.Value);
            initialPromptText = null;
        }

        if (suppressRegex.HasValue)
        {
            Marshal.FreeHGlobal(suppressRegex.Value);
            suppressRegex = null;
        }

        foreach (var gcHandle in gcHandles)
        {
            gcHandle.Free();
        }
        gcHandles.Clear();
        isDisposed = true;
    }

    private unsafe Task ProcessInternalAsync(ReadOnlyMemory<float> samples, CancellationToken cancellationToken)
    {
        if (isDisposed)
        {
            throw new ObjectDisposedException("This processor has already been disposed.");
        }
        return Task.Factory.StartNew(() =>
        {
            fixed (float* pData = samples.Span)
            {
                processingSemaphore.Wait();
                segmentIndex = 0;

                var state = GetWhisperState();

                try
                {
                    nativeWhisper.Whisper_Full_With_State(currentWhisperContext, state, whisperParams, (IntPtr)pData, samples.Length);
                }
                finally
                {
                    nativeWhisper.Whisper_Free_State(state);
                    processingSemaphore.Release();
                }
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    private IntPtr GetWhisperState()
    {
        var state = nativeWhisper.Whisper_Init_State(currentWhisperContext);
        if (RuntimeOptions.Instance.LoadedLibrary == RuntimeLibrary.OpenVino)
        {
            var modelPath = Marshal.StringToHGlobalAnsi(options.OpenVinoModelPath);
            var device = Marshal.StringToHGlobalAnsi(options.OpenVinoDevice);
            var cachePath = Marshal.StringToHGlobalAnsi(options.OpenVinoCacheDir);
            nativeWhisper.Whisper_Ctx_Init_Openvino_Encoder_With_State(
                options.ContextHandle,
                state,
                modelPath,
                device,
                cachePath);
        }
        return state;
    }

    private WhisperFullParams GetWhisperParams()
    {
        var strategy = options.SamplingStrategy.GetNativeStrategy();
        var whisperParamsRef = nativeWhisper.Whisper_Full_Default_Params_By_Ref(strategy);
        var whisperParams = Marshal.PtrToStructure<WhisperFullParams>(whisperParamsRef);
        nativeWhisper.Whisper_Free_Params(whisperParamsRef);
        whisperParams.Strategy = strategy;

        if (options.Threads.HasValue)
        {
            whisperParams.Threads = options.Threads.Value;
        }

        if (options.MaxLastTextTokens.HasValue)
        {
            whisperParams.MaxLastTextTokens = options.MaxLastTextTokens.Value;
        }

        if (options.Offset.HasValue)
        {
            whisperParams.OffsetMs = (int)options.Offset.Value.TotalMilliseconds;
        }

        if (options.Duration.HasValue)
        {
            whisperParams.DurationMs = (int)options.Duration.Value.TotalMilliseconds;
        }

        if (options.Translate.HasValue)
        {
            whisperParams.Translate = options.Translate.Value ? trueByte : falseByte;
        }

        if (options.NoContext.HasValue)
        {
            whisperParams.NoContext = options.NoContext.Value ? trueByte : falseByte;
        }

        if (options.SingleSegment.HasValue)
        {
            whisperParams.SingleSegment = options.SingleSegment.Value ? trueByte : falseByte;
        }

        if (options.PrintSpecialTokens.HasValue)
        {
            whisperParams.PrintSpecialTokens = options.PrintSpecialTokens.Value ? trueByte : falseByte;
        }

        if (options.PrintProgress.HasValue)
        {
            whisperParams.PrintProgress = options.PrintProgress.Value ? trueByte : falseByte;
        }

        if (options.PrintResults.HasValue)
        {
            whisperParams.PrintResults = options.PrintResults.Value ? trueByte : falseByte;
        }

        if (options.UseTokenTimestamps.HasValue)
        {
            whisperParams.UseTokenTimestamps = options.UseTokenTimestamps.Value ? trueByte : falseByte;
        }

        if (options.TokenTimestampsThreshold.HasValue)
        {
            whisperParams.TokenTimestampsThreshold = options.TokenTimestampsThreshold.Value;
        }

        if (options.TokenTimestampsSumThreshold.HasValue)
        {
            whisperParams.TokenTimestampsSumThreshold = options.TokenTimestampsSumThreshold.Value;
        }

        if (options.MaxSegmentLength.HasValue)
        {
            whisperParams.MaxSegmentLength = options.MaxSegmentLength.Value;
        }

        if (options.SplitOnWord.HasValue)
        {
            whisperParams.SplitOnWord = options.SplitOnWord.Value ? trueByte : falseByte;
        }

        if (options.MaxTokensPerSegment.HasValue)
        {
            whisperParams.MaxTokensPerSegment = options.MaxTokensPerSegment.Value;
        }

        if (options.AudioContextSize.HasValue)
        {
            whisperParams.AudioContextSize = options.AudioContextSize.Value;
        }

        if (!string.IsNullOrEmpty(options.SuppressRegex))
        {
            suppressRegex = Marshal.StringToHGlobalAnsi(options.SuppressRegex);
            whisperParams.SuppressRegex = suppressRegex.Value;
        }

        if (!string.IsNullOrEmpty(options.Prompt))
        {
            var tokenMaxLength = options.Prompt!.Length + 1;

            initialPromptText = Marshal.StringToHGlobalAnsi(options.Prompt);

            whisperParams.InitialPrompt = initialPromptText.Value;
        }

        if (options.Language != null)
        {
            language = Marshal.StringToHGlobalAnsi(options.Language);
            whisperParams.Language = language.Value;
        }

        if (options.SuppressBlank.HasValue)
        {
            whisperParams.SuppressBlank = options.SuppressBlank.Value ? trueByte : falseByte;
        }

        if (options.Temperature.HasValue)
        {
            whisperParams.Temperature = options.Temperature.Value;
        }

        if (options.MaxInitialTs.HasValue)
        {
            whisperParams.MaxInitialTs = options.MaxInitialTs.Value;
        }

        if (options.LengthPenalty.HasValue)
        {
            whisperParams.LengthPenalty = options.LengthPenalty.Value;
        }

        if (options.TemperatureInc.HasValue)
        {
            whisperParams.TemperatureInc = options.TemperatureInc.Value;
        }

        if (options.EntropyThreshold.HasValue)
        {
            whisperParams.EntropyThreshold = options.EntropyThreshold.Value;
        }

        if (options.LogProbThreshold.HasValue)
        {
            whisperParams.LogProbThreshold = options.LogProbThreshold.Value;
        }

        if (options.NoSpeechThreshold.HasValue)
        {
            whisperParams.NoSpeechThreshold = options.NoSpeechThreshold.Value;
        }

        if (options.SamplingStrategy is GreedySamplingStrategy greedySamplingStrategy)
        {
            if (greedySamplingStrategy.BestOf.HasValue)
            {
                whisperParams.WhisperParamGreedy.BestOf = greedySamplingStrategy.BestOf.Value;
            }
        }
        if (options.SamplingStrategy is BeamSearchSamplingStrategy beamSamplingStrategy)
        {
            if (beamSamplingStrategy.BeamSize.HasValue)
            {
                whisperParams.WhisperParamBeamSearch.BeamSize = beamSamplingStrategy.BeamSize.Value;
            }

            if (beamSamplingStrategy.Patience.HasValue)
            {
                whisperParams.WhisperParamBeamSearch.Patience = beamSamplingStrategy.Patience.Value;
            }
        }

        var myIntPtrId = new IntPtr(myId);
        whisperParams.OnNewSegmentUserData = myIntPtrId;
        whisperParams.OnEncoderBeginUserData = myIntPtrId;
        whisperParams.OnAbortUserData = myIntPtrId;

#if NETSTANDARD
        // For netframework, we don't have `UnmanagedCallersOnlyAttribute` so we need to use a delegate wrapped with a GC handle
        var onNewSegmentDelegate = new WhisperNewSegmentCallback(OnNewSegmentStatic);
        var gcHandle = GCHandle.Alloc(onNewSegmentDelegate);
        gcHandles.Add(gcHandle);
        whisperParams.OnNewSegment = Marshal.GetFunctionPointerForDelegate(onNewSegmentDelegate);

        var onEncoderBeginDelegate = new WhisperEncoderBeginCallback(OnEncoderBeginStatic);
        gcHandle = GCHandle.Alloc(onEncoderBeginDelegate);
        gcHandles.Add(gcHandle);
        whisperParams.OnEncoderBegin = Marshal.GetFunctionPointerForDelegate(onEncoderBeginDelegate);

        var onWhisperAbortDelegate = new WhisperAbortCallback(OnWhisperAbortStatic);
        gcHandle = GCHandle.Alloc(onWhisperAbortDelegate);
        gcHandles.Add(gcHandle);
        whisperParams.OnAbort = Marshal.GetFunctionPointerForDelegate(onWhisperAbortDelegate);

        if (options.OnProgressHandlers.Count > 0)
        {
            var onProgressDelegate = new WhisperProgressCallback(OnProgressStatic);
            gcHandle = GCHandle.Alloc(onProgressDelegate);
            gcHandles.Add(gcHandle);
            whisperParams.OnProgressCallback = Marshal.GetFunctionPointerForDelegate(onProgressDelegate);
            whisperParams.OnProgressCallbackUserData = myIntPtrId;
        }
#else
        unsafe
        {
            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr, void> onNewSegmentDelegate = &OnNewSegmentStatic;
            whisperParams.OnNewSegment = (IntPtr)onNewSegmentDelegate;

            delegate* unmanaged[Cdecl]<IntPtr, IntPtr, IntPtr, byte> onEncoderBeginDelegate = &OnEncoderBeginStatic;
            whisperParams.OnEncoderBegin = (IntPtr)onEncoderBeginDelegate;

            delegate* unmanaged[Cdecl]<IntPtr, byte> onWhisperAbortDelegate = &OnWhisperAbortStatic;
            whisperParams.OnAbort = (IntPtr)onWhisperAbortDelegate;

            if (options.OnProgressHandlers.Count > 0)
            {
                delegate* unmanaged[Cdecl]<IntPtr, IntPtr, int, IntPtr, void> onProgressDelegate = &OnProgressStatic;
                whisperParams.OnProgressCallback = (IntPtr)onProgressDelegate;
                whisperParams.OnProgressCallbackUserData = myIntPtrId;
            }
        }
#endif

        return whisperParams;
    }

#if !NETSTANDARD
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    private static byte OnWhisperAbortStatic(IntPtr userData)
    {
        if (!processorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }

        var shouldCancel = processor.options.WhisperAbortEventHandler?.Invoke() ?? false;
        return shouldCancel ? trueByte : falseByte;
    }

#if !NETSTANDARD
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    private static void OnNewSegmentStatic(IntPtr ctx, IntPtr state, int nNew, IntPtr userData)
    {
        if (!processorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }
        processor.OnNewSegment(state);
    }

#if !NETSTANDARD
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    private static byte OnEncoderBeginStatic(IntPtr ctx, IntPtr state, IntPtr userData)
    {
        if (!processorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }
        return processor.OnEncoderBegin() ? trueByte : falseByte;
    }

#if !NETSTANDARD
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    private static void OnProgressStatic(IntPtr ctx, IntPtr state, int progress, IntPtr userData)
    {
        if (!processorInstances.TryGetValue(userData.ToInt64(), out var processor))
        {
            throw new Exception("Couldn't find processor instance for user data");
        }
        processor.OnProgress(progress);
    }

    private void OnProgress(int progress)
    {
        if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
        {
            return;
        }

        foreach (var handler in options.OnProgressHandlers)
        {
            handler?.Invoke(progress);
            if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
            {
                return;
            }
        }
    }

    private bool OnEncoderBegin()
    {
        if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
        {
            return false;
        }

        var encoderBeginArgs = new EncoderBeginData();
        foreach (var handler in options.OnEncoderBeginEventHandlers)
        {
            var shouldContinue = handler.Invoke(encoderBeginArgs);
            if (!shouldContinue)
            {
                return false;
            }
        }
        return true;
    }

    private void OnNewSegment(IntPtr state)
    {
        if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
        {
            return;
        }

        var segments = nativeWhisper.Whisper_Full_N_Segments_From_State(state);

        while (segmentIndex < segments)
        {
            var t1 = TimeSpan.FromMilliseconds(nativeWhisper.Whisper_Full_Get_Segment_T1_From_State(state, segmentIndex) * 10);
            var t0 = TimeSpan.FromMilliseconds(nativeWhisper.Whisper_Full_Get_Segment_T0_From_State(state, segmentIndex) * 10);
            var textAnsi = StringFromNativeUtf8(nativeWhisper.Whisper_Full_Get_Segment_Text_From_State(state, segmentIndex));

            float minimumProbability = 0;
            float maximumProbability = 0;
            double sumProbability = 0;
            var numberOfTokens = nativeWhisper.Whisper_Full_N_Tokens_From_State(state, segmentIndex);
            var languageId = nativeWhisper.Whisper_Full_Lang_Id_From_State(state);
            var language = Marshal.PtrToStringAnsi(nativeWhisper.Whisper_Lang_Str(languageId));

            var tokens = new WhisperToken[numberOfTokens];

            for (var tokenIndex = 0; tokenIndex < numberOfTokens; tokenIndex++)
            {
                var tokenData = nativeWhisper.Whisper_Full_Get_Token_Data_From_State(state, segmentIndex, tokenIndex);
                var text = StringFromNativeUtf8(nativeWhisper.Whisper_Full_Get_Token_Text_From_State(currentWhisperContext, state, segmentIndex, tokenIndex));

                tokens[tokenIndex] = new()
                {
                    Id = tokenData.id,
                    Tid = tokenData.tid,
                    T_Dtw = tokenData.t_dtw,
                    Vlen = tokenData.vlen,
                    P = tokenData.p,
                    Plog = tokenData.plog,
                    Pt = tokenData.pt,
                    PtSum = tokenData.ptsum,
                    Text = text,
                    T0 = tokenData.t0,
                    T1 = tokenData.t1
                };

                if (options.ComputeProbabilities)
                {
                    var tokenProbability = nativeWhisper.Whisper_Full_Get_Token_P_From_State(state, segmentIndex, tokenIndex);
                    sumProbability += tokenProbability;
                    if (tokenIndex == 0)
                    {
                        minimumProbability = tokenProbability;
                        maximumProbability = tokenProbability;
                        continue;
                    }
                    if (tokenProbability < minimumProbability)
                    {
                        minimumProbability = tokenProbability;
                    }

                    if (tokenProbability > maximumProbability)
                    {
                        maximumProbability = tokenProbability;
                    }
                }
            }

            if (!string.IsNullOrEmpty(textAnsi))
            {
                var eventHandlerArgs = new SegmentData(textAnsi!, t0, t1, minimumProbability, maximumProbability, (float)(sumProbability / numberOfTokens), language!, tokens);

                foreach (var handler in options.OnSegmentEventHandlers)
                {
                    handler?.Invoke(eventHandlerArgs);
                    if (currentCancellationToken.HasValue && currentCancellationToken.Value.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }

            segmentIndex++;
        }
    }

    private static string? StringFromNativeUtf8(IntPtr nativeUtf8)
    {

#if NETSTANDARD
        var len = 0;

        while (Marshal.ReadByte(nativeUtf8, len) != 0)
        {
            len++;
        }

        var buffer = new byte[len];
        Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
        return System.Text.Encoding.UTF8.GetString(buffer);
#else
        return Marshal.PtrToStringUTF8(nativeUtf8);
#endif
    }

    /// <summary>
    /// Releases the resources used by this processor.
    /// </summary>
    /// <returns></returns>
    public async ValueTask DisposeAsync()
    {
        // If a processing is still running, wait for it to finish
        await processingSemaphore.WaitAsync();
        processingSemaphore.Release();
        Dispose();
    }
}
