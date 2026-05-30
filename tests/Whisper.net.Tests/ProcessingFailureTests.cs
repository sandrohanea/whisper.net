// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;
using Xunit;

namespace Whisper.net.Tests;

public class ProcessingFailureTests
{
    private sealed class FakeNativeWhisper : INativeWhisper
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
        public INativeWhisper.whisper_free Whisper_Free { get; }
        public INativeWhisper.whisper_free_params Whisper_Free_Params { get; }
        public INativeWhisper.whisper_full_default_params_by_ref Whisper_Full_Default_Params_By_Ref { get; }
        public INativeWhisper.whisper_full_with_state Whisper_Full_With_State { get; }
        public INativeWhisper.whisper_full_n_segments_from_state Whisper_Full_N_Segments_From_State { get; }
        public INativeWhisper.whisper_full_get_segment_t0_from_state Whisper_Full_Get_Segment_T0_From_State { get; }
        public INativeWhisper.whisper_full_get_segment_t1_from_state Whisper_Full_Get_Segment_T1_From_State { get; }
        public INativeWhisper.whisper_full_get_segment_text_from_state Whisper_Full_Get_Segment_Text_From_State { get; }
        public INativeWhisper.whisper_full_n_tokens_from_state Whisper_Full_N_Tokens_From_State { get; }
        public INativeWhisper.whisper_full_get_token_p_from_state Whisper_Full_Get_Token_P_From_State { get; }
        public INativeWhisper.whisper_lang_max_id Whisper_Lang_Max_Id { get; }
        public INativeWhisper.whisper_lang_id Whisper_Lang_Id { get; }
        public INativeWhisper.whisper_lang_auto_detect_with_state Whisper_Lang_Auto_Detect_With_State { get; }
        public INativeWhisper.whisper_pcm_to_mel_with_state Whisper_PCM_To_Mel_With_State { get; }
        public INativeWhisper.whisper_lang_str Whisper_Lang_Str { get; }
        public INativeWhisper.whisper_init_state Whisper_Init_State { get; }
        public INativeWhisper.whisper_free_state Whisper_Free_State { get; }
        public INativeWhisper.whisper_full_lang_id_from_state Whisper_Full_Lang_Id_From_State { get; }
        public INativeWhisper.whisper_log_set Whisper_Log_Set { get; }
        public INativeWhisper.whisper_ctx_init_openvino_encoder_with_state Whisper_Ctx_Init_Openvino_Encoder_With_State { get; }
        public INativeWhisper.whisper_full_get_token_data_from_state Whisper_Full_Get_Token_Data_From_State { get; }
        public INativeWhisper.whisper_full_get_token_text_from_state Whisper_Full_Get_Token_Text_From_State { get; }
        public INativeWhisper.whisper_print_system_info WhisperPrintSystemInfo { get; }
        public INativeWhisper.whisper_full_get_segment_no_speech_prob_from_state Whisper_Full_Get_Segment_No_Speech_Prob_From_State { get; }
        public INativeWhisper.whisper_vad_default_params Whisper_Vad_Default_Params { get; }
        public INativeWhisper.whisper_vad_default_context_params Whisper_Vad_Default_Context_Params { get; }
        public INativeWhisper.whisper_vad_init_from_file_with_params Whisper_Vad_Init_From_File_With_Params { get; }
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
                allowNativeReturn.Wait(TimeSpan.FromSeconds(5));
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

            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => WaitForTaskAsync(processingTask, TimeSpan.FromSeconds(1)));
            Assert.False(nativeFinished.Task.IsCompleted);
        }
        finally
        {
            allowNativeReturn.Set();
        }

        await WaitForTaskAsync(nativeFinished.Task);
    }

    private static async Task WaitForTaskAsync(Task task)
    {
        await WaitForTaskAsync(task, TimeSpan.FromSeconds(5));
    }

    private static async Task WaitForTaskAsync(Task task, TimeSpan timeout)
    {
        var completedTask = await Task.WhenAny(task, Task.Delay(timeout));
        Assert.Same(task, completedTask);
        await task;
    }
}
