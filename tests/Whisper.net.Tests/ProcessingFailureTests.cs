// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals;
using Whisper.net.Internals.Native;
using Whisper.net.Native;
using Xunit;

namespace Whisper.net.Tests;

public class ProcessingFailureTests
{
    // Native inference tests share the CI host and can delay managed test scheduling.
    private static readonly TimeSpan testTimeout = TimeSpan.FromSeconds(30);

    internal sealed class FakeNativeWhisper : INativeWhisper
    {
        private readonly int _errorCode;

        public FakeNativeWhisper(int errorCode, INativeWhisper.whisper_full_with_state? whisperFullWithState = null)
        {
            _errorCode = errorCode;
            Whisper_Full_With_State = whisperFullWithState ?? ((context, state, p, samples, n) => _errorCode);
            Whisper_Init_State = _ => new IntPtr(1);
            Whisper_Free_State = _ => { };
            Whisper_Full_Default_Params_By_Ref = strategy =>
            {
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<WhisperFullParams>());
                var param = new WhisperFullParams { Strategy = strategy };
                Marshal.StructureToPtr(param, ptr, false);
                return ptr;
            };
            Whisper_Free_Params = ptr => Marshal.FreeHGlobal(ptr);
            Whisper_Init_From_File_With_Params_No_State = (_, _) => IntPtr.Zero;
            Whisper_Init_From_Buffer_With_Params_No_State = (_, _, _) => IntPtr.Zero;
            Whisper_Init_With_Params_No_State = (ref WhisperModelLoader loader, WhisperContextParams parameters) => IntPtr.Zero;
            Whisper_Free = _ => { };
            Whisper_Full_N_Segments_From_State = _ => 0;
            Whisper_Full_Get_Segment_T0_From_State = (_, _) => 0;
            Whisper_Full_Get_Segment_T1_From_State = (_, _) => 0;
            Whisper_Full_Get_Segment_Text_From_State = (_, _) => IntPtr.Zero;
            Whisper_Full_N_Tokens_From_State = (_, _) => 0;
            Whisper_Full_Get_Token_P_From_State = (_, _, _) => 0;
            Whisper_Lang_Max_Id = () => 0;
            Whisper_Lang_Auto_Detect_With_State = (_, _, _, _, _) => 0;
            Whisper_PCM_To_Mel_With_State = (_, _, _, _, _) => 0;
            Whisper_Lang_Str = _ => IntPtr.Zero;
            Whisper_Full_Lang_Id_From_State = _ => 0;
            Whisper_Log_Set = (_, _) => { };
            Whisper_Ctx_Init_Openvino_Encoder_With_State = (_, _, _, _, _) => { };
            Whisper_Full_Get_Token_Data_From_State = (_, _, _) => default;
            Whisper_Full_Get_Token_Text_From_State = (_, _, _, _) => IntPtr.Zero;
            WhisperPrintSystemInfo = () => IntPtr.Zero;
            Whisper_Full_Get_Segment_No_Speech_Prob_From_State = (_, _) => 0;
            Whisper_Lang_Id = _ => 0;
            Whisper_Vad_Default_Params = () => default;
            Whisper_Vad_Default_Context_Params = () => default;
            Whisper_Vad_Init_From_File_With_Params = (_, _) => IntPtr.Zero;
            Whisper_Vad_Init_With_Params = (ref WhisperModelLoader loader, WhisperVadContextParams parameters) => IntPtr.Zero;
            Whisper_Vad_Detect_Speech = (_, _, _) => 0;
            Whisper_Vad_Detect_Speech_No_Reset = (_, _, _) => 0;
            Whisper_Vad_Reset_State = _ => { };
            Whisper_Vad_Segments_From_Probs = (_, _) => IntPtr.Zero;
            Whisper_Vad_Segments_N_Segments = _ => 0;
            Whisper_Vad_Segments_Get_Segment_T0 = (_, _) => 0;
            Whisper_Vad_Segments_Get_Segment_T1 = (_, _) => 0;
            Whisper_Vad_Free_Segments = _ => { };
            Whisper_Vad_Free = _ => { };
        }

        public INativeWhisper.whisper_init_from_file_with_params_no_state Whisper_Init_From_File_With_Params_No_State { get; }
        public INativeWhisper.whisper_init_from_buffer_with_params_no_state Whisper_Init_From_Buffer_With_Params_No_State { get; }
        public INativeWhisper.whisper_init_with_params_no_state Whisper_Init_With_Params_No_State { get; }
        public INativeWhisper.whisper_free Whisper_Free { get; }
        public INativeWhisper.whisper_free_params Whisper_Free_Params { get; }
        public INativeWhisper.whisper_full_default_params_by_ref Whisper_Full_Default_Params_By_Ref { get; }
        public INativeWhisper.whisper_full_with_state Whisper_Full_With_State { get; }
        public INativeWhisper.whisper_full_n_segments_from_state Whisper_Full_N_Segments_From_State { get; set; }
        public INativeWhisper.whisper_full_get_segment_t0_from_state Whisper_Full_Get_Segment_T0_From_State { get; }
        public INativeWhisper.whisper_full_get_segment_t1_from_state Whisper_Full_Get_Segment_T1_From_State { get; }
        public INativeWhisper.whisper_full_get_segment_text_from_state Whisper_Full_Get_Segment_Text_From_State { get; set; }
        public INativeWhisper.whisper_full_n_tokens_from_state Whisper_Full_N_Tokens_From_State { get; set; }
        public INativeWhisper.whisper_full_get_token_p_from_state Whisper_Full_Get_Token_P_From_State { get; }
        public INativeWhisper.whisper_lang_max_id Whisper_Lang_Max_Id { get; }
        public INativeWhisper.whisper_lang_id Whisper_Lang_Id { get; }
        public INativeWhisper.whisper_lang_auto_detect_with_state Whisper_Lang_Auto_Detect_With_State { get; }
        public INativeWhisper.whisper_pcm_to_mel_with_state Whisper_PCM_To_Mel_With_State { get; }
        public INativeWhisper.whisper_lang_str Whisper_Lang_Str { get; set; }
        public INativeWhisper.whisper_init_state Whisper_Init_State { get; }
        public INativeWhisper.whisper_free_state Whisper_Free_State { get; }
        public INativeWhisper.whisper_full_lang_id_from_state Whisper_Full_Lang_Id_From_State { get; }
        public INativeWhisper.whisper_log_set Whisper_Log_Set { get; }
        public INativeWhisper.whisper_ctx_init_openvino_encoder_with_state Whisper_Ctx_Init_Openvino_Encoder_With_State { get; }
        public INativeWhisper.whisper_full_get_token_data_from_state Whisper_Full_Get_Token_Data_From_State { get; }
        public INativeWhisper.whisper_full_get_token_text_from_state Whisper_Full_Get_Token_Text_From_State { get; set; }
        public INativeWhisper.whisper_print_system_info WhisperPrintSystemInfo { get; }
        public INativeWhisper.whisper_full_get_segment_no_speech_prob_from_state Whisper_Full_Get_Segment_No_Speech_Prob_From_State { get; }
        public INativeWhisper.whisper_vad_default_params Whisper_Vad_Default_Params { get; }
        public INativeWhisper.whisper_vad_default_context_params Whisper_Vad_Default_Context_Params { get; }
        public INativeWhisper.whisper_vad_init_from_file_with_params Whisper_Vad_Init_From_File_With_Params { get; }
        public INativeWhisper.whisper_vad_init_with_params Whisper_Vad_Init_With_Params { get; }
        public INativeWhisper.whisper_vad_detect_speech Whisper_Vad_Detect_Speech { get; }
        public INativeWhisper.whisper_vad_detect_speech_no_reset Whisper_Vad_Detect_Speech_No_Reset { get; }
        public INativeWhisper.whisper_vad_reset_state Whisper_Vad_Reset_State { get; }
        public INativeWhisper.whisper_vad_segments_from_probs Whisper_Vad_Segments_From_Probs { get; }
        public INativeWhisper.whisper_vad_segments_n_segments Whisper_Vad_Segments_N_Segments { get; }
        public INativeWhisper.whisper_vad_segments_get_segment_t0 Whisper_Vad_Segments_Get_Segment_T0 { get; }
        public INativeWhisper.whisper_vad_segments_get_segment_t1 Whisper_Vad_Segments_Get_Segment_T1 { get; }
        public INativeWhisper.whisper_vad_free_segments Whisper_Vad_Free_Segments { get; }
        public INativeWhisper.whisper_vad_free Whisper_Vad_Free { get; }

        public void Dispose() { }
    }

    [Fact]
    public void Process_WhenNativeFails_ShouldThrow()
    {
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        using var processor = new WhisperProcessor(options, new FakeNativeWhisper(5));
        Assert.Throws<WhisperProcessingException>(() => processor.Process(new float[1]));
    }

    [Theory]
    [InlineData("segment", false)]
    [InlineData("segment", true)]
    [InlineData("progress", false)]
    [InlineData("progress", true)]
    [InlineData("encoder", false)]
    [InlineData("encoder", true)]
    [InlineData("abort", false)]
    [InlineData("abort", true)]
    [InlineData("utf8", false)]
    [InlineData("utf8", true)]
    public async Task ChangeLanguage_FromNativeCallback_AppliesToNextRun(string callbackKind, bool asynchronous)
    {
        var text = Utf8TestUtils.AllocateNullTerminated(" segment");
        var language = Utf8TestUtils.AllocateNullTerminated("en");
        WhisperProcessor? processor = null;
        var callbackCalled = false;
        var observedLanguages = new List<string?>();

        void ChangeLanguage()
        {
            processor!.ChangeLanguage("fr");
            processor.ChangeLanguage(null);
            processor.ChangeLanguage("ro");
            callbackCalled = true;
        }

        try
        {
            using var native = new FakeNativeWhisper(0, (_, state, parameters, _, _) =>
            {
                observedLanguages.Add(MarshalUtils.GetString(parameters.Language));
                if (observedLanguages.Count == 1)
                {
                    switch (callbackKind)
                    {
                        case "segment":
                        case "utf8":
                            Marshal.GetDelegateForFunctionPointer<WhisperNewSegmentCallback>(parameters.OnNewSegment)(
                                IntPtr.Zero, state, 1, parameters.OnNewSegmentUserData);
                            break;
                        case "progress":
                            Marshal.GetDelegateForFunctionPointer<WhisperProgressCallback>(parameters.OnProgressCallback)(
                                IntPtr.Zero, state, 50, parameters.OnProgressCallbackUserData);
                            break;
                        case "encoder":
                            Marshal.GetDelegateForFunctionPointer<WhisperEncoderBeginCallback>(parameters.OnEncoderBegin)(
                                IntPtr.Zero, state, parameters.OnEncoderBeginUserData);
                            break;
                        case "abort":
                            Marshal.GetDelegateForFunctionPointer<WhisperAbortCallback>(parameters.OnAbort)(
                                parameters.OnAbortUserData);
                            break;
                    }

                    Assert.Equal("en", MarshalUtils.GetString(parameters.Language));
                }

                return 0;
            })
            {
                Whisper_Full_N_Segments_From_State = _ => 1,
                Whisper_Full_Get_Segment_Text_From_State = (_, _) => text,
                Whisper_Lang_Str = _ => language
            };
            var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero, Language = "en" };
            options.OnSegmentEventHandlers.Add(_ => ChangeLanguage());
            options.OnProgressHandlers.Add(_ => ChangeLanguage());
            options.OnEncoderBeginEventHandlers.Add(_ =>
            {
                ChangeLanguage();
                return true;
            });
            options.WhisperAbortEventHandler = () =>
            {
                ChangeLanguage();
                return false;
            };
            using var ownedProcessor = new WhisperProcessor(options, native);
            processor = ownedProcessor;

            // Synchronous processing blocks its caller; keep it off the shared thread pool.
            var processing = Task.Factory.StartNew(async () =>
            {
                if (callbackKind == "utf8")
                {
                    if (asynchronous)
                    {
                        await processor.ProcessWithUtf8HandlerAsync(new float[1], _ => ChangeLanguage());
                    }
                    else
                    {
                        processor.ProcessWithUtf8Handler(new float[1], _ => ChangeLanguage());
                    }
                }
                else if (asynchronous)
                {
                    await foreach (var _ in processor.ProcessAsync(new float[1]))
                    {
                    }
                }
                else
                {
                    processor.Process(new float[1]);
                }
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();

            await WaitForTaskAsync(processing);
            Assert.True(callbackCalled);
            processor.Process(new float[1]);
            Assert.Equal(new[] { "en", "ro" }, observedLanguages);
        }
        finally
        {
            Marshal.FreeCoTaskMem(text);
            Marshal.FreeCoTaskMem(language);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("ro")]
    public async Task ChangeLanguage_DuringGatedNativeCall_KeepsActiveLanguageAlive(string? newLanguage)
    {
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var release = new ManualResetEventSlim();
        var observedLanguages = new List<string?>();
        using var native = new FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            observedLanguages.Add(MarshalUtils.GetString(parameters.Language));
            if (observedLanguages.Count == 1)
            {
                entered.SetResult(true);
                Assert.True(release.Wait(testTimeout));
                Assert.Equal("en", MarshalUtils.GetString(parameters.Language));
            }

            return 0;
        });
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero, Language = "en" };
        using var processor = new WhisperProcessor(options, native);
        var processing = processor.ProcessWithUtf8HandlerAsync(new float[1], _ => { });
        try
        {
            await WaitForTaskAsync(entered.Task);
            await WaitForTaskAsync(Task.Run(() => processor.ChangeLanguage(newLanguage)));
        }
        finally
        {
            release.Set();
            await WaitForTaskAsync(processing);
        }

        processor.Process(new float[1]);
        Assert.Equal(new[] { "en", string.IsNullOrEmpty(newLanguage) ? null : newLanguage }, observedLanguages);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangeLanguage_WhenNativeFails_AppliesDeferredValue(bool asynchronous)
    {
        WhisperProcessor? processor = null;
        var calls = 0;
        using var native = new FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            if (++calls == 1)
            {
                processor!.ChangeLanguage(null);
                Assert.Equal("en", MarshalUtils.GetString(parameters.Language));
                return 5;
            }

            Assert.Equal(IntPtr.Zero, parameters.Language);
            return 0;
        });
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero, Language = "en" };
        using var ownedProcessor = new WhisperProcessor(options, native);
        processor = ownedProcessor;

        if (asynchronous)
        {
            await Assert.ThrowsAsync<WhisperProcessingException>(() =>
                processor.ProcessWithUtf8HandlerAsync(new float[1], _ => { }));
        }
        else
        {
            Assert.Throws<WhisperProcessingException>(() => processor.Process(new float[1]));
        }

        processor.Process(new float[1]);
    }

    [Fact]
    public async Task DisposeAsync_WithDeferredLanguage_WaitsForNativeCallAndRejectsFurtherChanges()
    {
        var entered = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var release = new ManualResetEventSlim();
        using var native = new FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            entered.SetResult(true);
            Assert.True(release.Wait(testTimeout));
            Assert.Equal("en", MarshalUtils.GetString(parameters.Language));
            return 0;
        });
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero, Language = "en" };
        using var processor = new WhisperProcessor(options, native);
        var processing = processor.ProcessWithUtf8HandlerAsync(new float[1], _ => { });
        Task? disposal = null;
        try
        {
            await WaitForTaskAsync(entered.Task);
            processor.ChangeLanguage("ro");
            processor.ChangeLanguage(null);
            disposal = processor.DisposeAsync().AsTask();
            Assert.False(disposal.IsCompleted);
        }
        finally
        {
            release.Set();
            await WaitForTaskAsync(processing);
            if (disposal is not null)
            {
                await WaitForTaskAsync(disposal);
            }
        }

        Assert.Throws<ObjectDisposedException>(() => processor.ChangeLanguage("ro"));
        Assert.Throws<ObjectDisposedException>(() => processor.ChangeLanguage(null));
    }

    [Fact]
    public void Process_WhenSamplesAreEmpty_ShouldNotCallNative()
    {
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        using var processor = new WhisperProcessor(options, new FakeNativeWhisper(0, (_, _, _, _, _) =>
        {
            Assert.Fail("The native processor should not be called for empty samples.");
            return 0;
        }));

        processor.Process([]);
    }

    [Fact]
    public async Task ProcessAsync_WhenNativeFails_ShouldThrow()
    {
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        await using var processor = new WhisperProcessor(options, new FakeNativeWhisper(7));
        await Assert.ThrowsAsync<WhisperProcessingException>(async () =>
        {
            await foreach (var _ in processor.ProcessAsync(new float[1]))
            {
            }
        });
    }

    [Fact]
    public async Task ProcessAsync_WhenSamplesAreEmpty_ShouldNotCallNative()
    {
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        await using var processor = new WhisperProcessor(options, new FakeNativeWhisper(0, (_, _, _, _, _) =>
        {
            Assert.Fail("The native processor should not be called for empty samples.");
            return 0;
        }));

        var segments = new List<SegmentData>();
        await foreach (var segment in processor.ProcessAsync(ReadOnlyMemory<float>.Empty))
        {
            segments.Add(segment);
        }

        Assert.Empty(segments);
    }

    [Fact]
    public async Task ProcessAsync_WhenCancelledBeforeSegment_ShouldWakeEnumeratorAndSignalNativeAbort()
    {
        var nativeStarted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var nativeAbortObserved = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var nativeFinished = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var allowNativeReturn = new ManualResetEventSlim();

        using var native = new FakeNativeWhisper(-6, (_, _, parameters, _, _) =>
        {
            try
            {
                nativeStarted.SetResult(null);
                var abortCallback = Marshal.GetDelegateForFunctionPointer<WhisperAbortCallback>(parameters.OnAbort);

                while (abortCallback(parameters.OnAbortUserData) == 0)
                {
                    Thread.Sleep(10);
                }

                nativeAbortObserved.SetResult(null);
                Assert.True(
                    allowNativeReturn.Wait(TimeSpan.FromSeconds(30)),
                    "Native processing was not released within the safety timeout.");
                return -6;
            }
            finally
            {
                nativeFinished.SetResult(null);
            }
        });

        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        await using var processor = new WhisperProcessor(options, native);
        using var cts = new CancellationTokenSource();

        var processingTask = Task.Run(async () =>
        {
            await foreach (var _ in processor.ProcessAsync(new float[1], cts.Token))
            {
            }
        });

        try
        {
            await WaitForTaskAsync(nativeStarted.Task);
            cts.Cancel();
            await WaitForTaskAsync(nativeAbortObserved.Task);

            await Assert.ThrowsAsync<TaskCanceledException>(() => WaitForTaskAsync(processingTask, TimeSpan.FromSeconds(10)));
            Assert.False(nativeFinished.Task.IsCompleted);
        }
        finally
        {
            allowNativeReturn.Set();
        }

        await WaitForTaskAsync(nativeFinished.Task);
    }

    [Fact]
    public async Task ProcessAsync_WhenCancelledBeforeNativeFailure_ShouldThrowTaskCanceledException()
    {
        using var cts = new CancellationTokenSource();
        using var native = new FakeNativeWhisper(-6, (_, _, _, _, _) =>
        {
            cts.Cancel();
            return -6;
        });

        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        await using var processor = new WhisperProcessor(options, native);

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await foreach (var _ in processor.ProcessAsync(new float[1], cts.Token))
            {
            }
        });
    }

    [Fact]
    public async Task ProcessWithUtf8HandlerAsync_InvokesHandlerWithoutAllocatingSegmentData()
    {
        var segmentText = Utf8TestUtils.AllocateNullTerminated(" Segment text");
        var tokenText = Utf8TestUtils.AllocateNullTerminated(" token");
        var language = Utf8TestUtils.AllocateNullTerminated("en");
        var allocatingHandlerCalled = false;

        try
        {
            using var native = CreateUtf8HandlerNative(segmentText, tokenText, language);
            var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
            options.OnSegmentEventHandlers.Add(_ => allocatingHandlerCalled = true);
            await using var processor = new WhisperProcessor(options, native);

            string? actualSegmentText = null;
            string? actualTokenText = null;
            string? actualLanguage = null;

            await processor.ProcessWithUtf8HandlerAsync(new float[1], segment =>
            {
                actualSegmentText = Utf8TestUtils.Decode(segment.TextUtf8);
                actualLanguage = Utf8TestUtils.Decode(segment.LanguageUtf8);
                actualTokenText = Utf8TestUtils.Decode(segment.GetToken(0).TextUtf8);
            });

            Assert.False(allocatingHandlerCalled);
            Assert.Equal(" Segment text", actualSegmentText);
            Assert.Equal(" token", actualTokenText);
            Assert.Equal("en", actualLanguage);
        }
        finally
        {
            Marshal.FreeCoTaskMem(segmentText);
            Marshal.FreeCoTaskMem(tokenText);
            Marshal.FreeCoTaskMem(language);
        }
    }

    [Fact]
    public async Task ProcessWithUtf8HandlerAsync_WhenHandlerThrows_RethrowsFromTask()
    {
        var segmentText = Utf8TestUtils.AllocateNullTerminated(" Segment text");
        var tokenText = Utf8TestUtils.AllocateNullTerminated(" token");
        var language = Utf8TestUtils.AllocateNullTerminated("en");

        try
        {
            using var native = CreateUtf8HandlerNative(segmentText, tokenText, language);
            var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
            await using var processor = new WhisperProcessor(options, native);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                processor.ProcessWithUtf8HandlerAsync(
                    new float[1],
                    _ => throw new InvalidOperationException("Handler failed.")));

            Assert.Equal("Handler failed.", exception.Message);
        }
        finally
        {
            Marshal.FreeCoTaskMem(segmentText);
            Marshal.FreeCoTaskMem(tokenText);
            Marshal.FreeCoTaskMem(language);
        }
    }

    [Fact]
    public void ProcessWithUtf8HandlerAsync_WhenHandlerIsNull_ThrowsArgumentNullException()
    {
        using var native = new FakeNativeWhisper(0);
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        using var processor = new WhisperProcessor(options, native);

        Assert.Throws<ArgumentNullException>(() =>
        {
            _ = processor.ProcessWithUtf8HandlerAsync(new float[1], null!);
        });
    }

    [Fact]
    public void ProcessWithUtf8Handler_InvokesHandlerSynchronously()
    {
        var segmentText = Utf8TestUtils.AllocateNullTerminated(" Segment text");
        var tokenText = Utf8TestUtils.AllocateNullTerminated(" token");
        var language = Utf8TestUtils.AllocateNullTerminated("en");

        try
        {
            using var native = CreateUtf8HandlerNative(segmentText, tokenText, language);
            var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
            using var processor = new WhisperProcessor(options, native);
            var handlerCalled = false;

            processor.ProcessWithUtf8Handler(new float[1], segment =>
            {
                handlerCalled = true;
                Assert.True(segment.TextUtf8.SequenceEqual(" Segment text"u8));
            });

            Assert.True(handlerCalled);
        }
        finally
        {
            Marshal.FreeCoTaskMem(segmentText);
            Marshal.FreeCoTaskMem(tokenText);
            Marshal.FreeCoTaskMem(language);
        }
    }

    [Fact]
    public void ProcessWithUtf8Handler_WhenHandlerThrows_RethrowsFromCall()
    {
        var segmentText = Utf8TestUtils.AllocateNullTerminated(" Segment text");
        var tokenText = Utf8TestUtils.AllocateNullTerminated(" token");
        var language = Utf8TestUtils.AllocateNullTerminated("en");

        try
        {
            using var native = CreateUtf8HandlerNative(segmentText, tokenText, language);
            var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
            using var processor = new WhisperProcessor(options, native);

            var exception = Assert.Throws<InvalidOperationException>(() =>
                processor.ProcessWithUtf8Handler(
                    new float[1],
                    _ => throw new InvalidOperationException("Handler failed.")));

            Assert.Equal("Handler failed.", exception.Message);
        }
        finally
        {
            Marshal.FreeCoTaskMem(segmentText);
            Marshal.FreeCoTaskMem(tokenText);
            Marshal.FreeCoTaskMem(language);
        }
    }

    private static FakeNativeWhisper CreateUtf8HandlerNative(
        IntPtr segmentText,
        IntPtr tokenText,
        IntPtr language)
    {
        var native = new FakeNativeWhisper(0, (_, state, parameters, _, _) =>
        {
            var callback = Marshal.GetDelegateForFunctionPointer<WhisperNewSegmentCallback>(parameters.OnNewSegment);
            callback(IntPtr.Zero, state, 1, parameters.OnNewSegmentUserData);
            return 0;
        })
        {
            Whisper_Full_N_Segments_From_State = _ => 1,
            Whisper_Full_Get_Segment_Text_From_State = (_, _) => segmentText,
            Whisper_Full_N_Tokens_From_State = (_, _) => 1,
            Whisper_Full_Get_Token_Text_From_State = (_, _, _, _) => tokenText,
            Whisper_Lang_Str = _ => language
        };

        return native;
    }

    private static async Task WaitForTaskAsync(Task task)
    {
        await WaitForTaskAsync(task, testTimeout);
    }

    private static async Task WaitForTaskAsync(Task task, TimeSpan timeout)
    {
        using var timeoutCancellation = new CancellationTokenSource();
        var timeoutTask = Task.Delay(timeout, timeoutCancellation.Token);
        var completedTask = await Task.WhenAny(task, timeoutTask);

        if (completedTask != task && !task.IsCompleted)
        {
            Assert.Fail($"Task did not complete within {timeout}.");
        }

        timeoutCancellation.Cancel();
        await task;
    }
}
