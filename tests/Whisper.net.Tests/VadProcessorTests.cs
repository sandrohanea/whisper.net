// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.ModelLoader;
using Whisper.net.Internals.Native;
using Whisper.net.Native;
using Xunit;

namespace Whisper.net.Tests;

public class VadProcessorTests
{
    [Fact]
    public void Build_ShouldUseVadContextParams()
    {
        var loader = new FakeModelLoader();
        var nativeWhisper = new FakeNativeWhisper();

        using var processor = new WhisperVadProcessorBuilder(loader, nativeWhisper)
            .WithThreads(4)
            .WithUseGpu(false)
            .WithGpuDevice(2)
            .Build();

        Assert.Equal(4, loader.VadContextParams.NThreads);
        Assert.Equal(0, loader.VadContextParams.UseGpu);
        Assert.Equal(2, loader.VadContextParams.GpuDevice);
    }

    [Fact]
    public void DetectSpeech_ShouldReturnSegments()
    {
        var nativeWhisper = new FakeNativeWhisper();
        var options = new WhisperVadProcessorOptions
        {
            ContextHandle = new IntPtr(10),
            VadParams = new WhisperVadParams { Threshold = 0.75f }
        };

        using var processor = new WhisperVadProcessor(options, nativeWhisper);

        var segments = processor.DetectSpeech([0.1f, 0.2f]);

        Assert.Equal(2, nativeWhisper.LastSamplesCount);
        Assert.Equal(0.75f, nativeWhisper.LastVadParams.Threshold);
        Assert.True(nativeWhisper.WasSegmentsHandleFreed);
        Assert.Collection(segments,
            segment =>
            {
                Assert.Equal(TimeSpan.FromSeconds(1.25), segment.Start);
                Assert.Equal(TimeSpan.FromSeconds(2.5), segment.End);
            },
            segment =>
            {
                Assert.Equal(TimeSpan.FromSeconds(3), segment.Start);
                Assert.Equal(TimeSpan.FromSeconds(4), segment.End);
            });
    }

    [Fact]
    public void DetectSpeech_WhenNativeFails_ShouldThrow()
    {
        var nativeWhisper = new FakeNativeWhisper { DetectSpeechResult = 0 };
        var options = new WhisperVadProcessorOptions
        {
            ContextHandle = new IntPtr(10),
            VadParams = new WhisperVadParams()
        };

        using var processor = new WhisperVadProcessor(options, nativeWhisper);

        Assert.Throws<WhisperProcessingException>(() => processor.DetectSpeech([0.1f]));
    }

    [Fact]
    public void DetectSpeechNoReset_ShouldUseNoResetNativeCall()
    {
        var nativeWhisper = new FakeNativeWhisper();
        var options = new WhisperVadProcessorOptions
        {
            ContextHandle = new IntPtr(10),
            VadParams = new WhisperVadParams()
        };

        using var processor = new WhisperVadProcessor(options, nativeWhisper);

        processor.DetectSpeechNoReset([0.1f, 0.2f, 0.3f]);

        Assert.True(nativeWhisper.WasNoResetDetectionUsed);
        Assert.Equal(3, nativeWhisper.LastSamplesCount);
    }

    [Fact]
    public void ResetState_ShouldResetNativeVadState()
    {
        var nativeWhisper = new FakeNativeWhisper();
        var options = new WhisperVadProcessorOptions
        {
            ContextHandle = new IntPtr(10),
            VadParams = new WhisperVadParams()
        };

        using var processor = new WhisperVadProcessor(options, nativeWhisper);

        processor.ResetState();

        Assert.True(nativeWhisper.WasStateReset);
    }

    private sealed class FakeModelLoader : IWhisperProcessorModelLoader
    {
        public WhisperVadContextParams VadContextParams { get; private set; }

        public IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
        {
            return IntPtr.Zero;
        }

        public IntPtr LoadNativeVadContext(INativeWhisper nativeWhisper, WhisperVadContextParams parameters)
        {
            VadContextParams = parameters;
            return new IntPtr(10);
        }

        public void Dispose()
        {
        }
    }

    private sealed class FakeNativeWhisper : INativeWhisper
    {
        private readonly IntPtr segmentsHandle = new(20);

        public FakeNativeWhisper()
        {
            Whisper_Init_From_File_With_Params_No_State = (_, _) => IntPtr.Zero;
            Whisper_Init_From_Buffer_With_Params_No_State = (_, _, _) => IntPtr.Zero;
            Whisper_Free = _ => { };
            Whisper_Free_Params = ptr => Marshal.FreeHGlobal(ptr);
            Whisper_Full_Default_Params_By_Ref = strategy =>
            {
                var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<WhisperFullParams>());
                var param = new WhisperFullParams { Strategy = strategy };
                Marshal.StructureToPtr(param, ptr, false);
                return ptr;
            };
            Whisper_Full_With_State = (_, _, _, _, _) => 0;
            Whisper_Full_N_Segments_From_State = _ => 0;
            Whisper_Full_Get_Segment_T0_From_State = (_, _) => 0;
            Whisper_Full_Get_Segment_T1_From_State = (_, _) => 0;
            Whisper_Full_Get_Segment_Text_From_State = (_, _) => IntPtr.Zero;
            Whisper_Full_N_Tokens_From_State = (_, _) => 0;
            Whisper_Full_Get_Token_P_From_State = (_, _, _) => 0;
            Whisper_Lang_Max_Id = () => 0;
            Whisper_Lang_Id = _ => 0;
            Whisper_Lang_Auto_Detect_With_State = (_, _, _, _, _) => 0;
            Whisper_PCM_To_Mel_With_State = (_, _, _, _, _) => 0;
            Whisper_Lang_Str = _ => IntPtr.Zero;
            Whisper_Init_State = _ => IntPtr.Zero;
            Whisper_Free_State = _ => { };
            Whisper_Full_Lang_Id_From_State = _ => 0;
            Whisper_Log_Set = (_, _) => { };
            Whisper_Ctx_Init_Openvino_Encoder_With_State = (_, _, _, _, _) => { };
            Whisper_Full_Get_Token_Data_From_State = (_, _, _) => default;
            Whisper_Full_Get_Token_Text_From_State = (_, _, _, _) => IntPtr.Zero;
            WhisperPrintSystemInfo = () => IntPtr.Zero;
            Whisper_Full_Get_Segment_No_Speech_Prob_From_State = (_, _) => 0;
            Whisper_Vad_Default_Params = () => new WhisperVadParams { Threshold = 0.5f };
            Whisper_Vad_Default_Context_Params = () => new WhisperVadContextParams { NThreads = 1, UseGpu = 1, GpuDevice = 0 };
            Whisper_Vad_Init_From_File_With_Params = (_, _) => new IntPtr(10);
            Whisper_Vad_Detect_Speech = (_, _, nSamples) =>
            {
                LastSamplesCount = nSamples;
                return DetectSpeechResult;
            };
            Whisper_Vad_Detect_Speech_No_Reset = (_, _, nSamples) =>
            {
                WasNoResetDetectionUsed = true;
                LastSamplesCount = nSamples;
                return DetectSpeechResult;
            };
            Whisper_Vad_Reset_State = _ => WasStateReset = true;
            Whisper_Vad_Segments_From_Probs = (_, parameters) =>
            {
                LastVadParams = parameters;
                return segmentsHandle;
            };
            Whisper_Vad_Segments_N_Segments = _ => 2;
            Whisper_Vad_Segments_Get_Segment_T0 = (_, index) => index == 0 ? 1.25f : 3;
            Whisper_Vad_Segments_Get_Segment_T1 = (_, index) => index == 0 ? 2.5f : 4;
            Whisper_Vad_Free_Segments = handle => WasSegmentsHandleFreed = handle == segmentsHandle;
            Whisper_Vad_Free = _ => { };
        }

        public byte DetectSpeechResult { get; set; } = 1;
        public int LastSamplesCount { get; private set; }
        public WhisperVadParams LastVadParams { get; private set; }
        public bool WasSegmentsHandleFreed { get; private set; }
        public bool WasNoResetDetectionUsed { get; private set; }
        public bool WasStateReset { get; private set; }

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

        public void Dispose()
        {
        }
    }
}
