// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using System.Text;
using Whisper.net.Internals.Native;
using Whisper.net.Native;
using Xunit;

namespace Whisper.net.Tests;

public class ParakeetProcessorTests
{
    [Fact]
    public void Process_MapsOptionsAndSegmentData()
    {
        using var native = new FakeNativeParakeet();
        SegmentData? segment = null;
        var options = new WhisperProcessorOptions
        {
            ModelFamily = WhisperModelFamily.Parakeet,
            ContextHandle = new IntPtr(1),
            Threads = 3,
            Offset = TimeSpan.FromMilliseconds(120),
            Duration = TimeSpan.FromMilliseconds(340),
            NoContext = true,
            AudioContextSize = 512,
            ComputeProbabilities = true,
            OnSegmentEventHandlers = [data => segment = data]
        };

        using var processor = new WhisperProcessor(options, native);
        processor.Process(new float[160]);

        Assert.Equal(3, native.LastParameters.Threads);
        Assert.Equal(120, native.LastParameters.OffsetMs);
        Assert.Equal(340, native.LastParameters.DurationMs);
        Assert.Equal(1, native.LastParameters.NoContext);
        Assert.Equal(512, native.LastParameters.AudioContextSize);

        Assert.NotNull(segment);
        Assert.Equal("hello", segment.Text);
        Assert.Equal(TimeSpan.FromMilliseconds(100), segment.Start);
        Assert.Equal(TimeSpan.FromMilliseconds(250), segment.End);
        Assert.Equal(string.Empty, segment.Language);
        Assert.True(float.IsNaN(segment.NoSpeechProbability));
        Assert.Equal(0.75f, segment.MinProbability);
        Assert.Equal(0.75f, segment.MaxProbability);
        Assert.Equal(0.75f, segment.Probability);

        var token = Assert.Single(segment.Tokens);
        Assert.Equal(42, token.Id);
        Assert.Equal("hello", token.Text);
        Assert.Equal(0.75f, token.Probability);
        Assert.Equal(-0.25f, token.ProbabilityLog);
        Assert.Equal(10, token.Start);
        Assert.Equal(25, token.End);
        Assert.Equal(0, token.TimestampId);
        Assert.Equal(0, token.TimestampProbability);
        Assert.Equal(0, token.TimestampProbabilitySum);
        Assert.Equal(0, token.DtwTimestamp);
        Assert.Equal(0, token.VoiceLen);
    }

    [Fact]
    public void LanguageOperations_ThrowNotSupportedException()
    {
        using var native = new FakeNativeParakeet();
        var options = new WhisperProcessorOptions
        {
            ModelFamily = WhisperModelFamily.Parakeet,
            ContextHandle = new IntPtr(1)
        };
        using var processor = new WhisperProcessor(options, native);

        Assert.Throws<NotSupportedException>(() => processor.ChangeLanguage("en"));
        Assert.Throws<NotSupportedException>(() => processor.DetectLanguage([0f]));
        Assert.Throws<NotSupportedException>(() => processor.DetectLanguageWithProbability([0f]));
    }

    [Fact]
    public void Process_WhenSegmentHandlerThrows_RethrowsAfterNativeCall()
    {
        using var native = new FakeNativeParakeet();
        var expectedException = new InvalidOperationException("Segment handler failed.");
        var options = new WhisperProcessorOptions
        {
            ModelFamily = WhisperModelFamily.Parakeet,
            ContextHandle = new IntPtr(1),
            OnSegmentEventHandlers = [_ => throw expectedException]
        };
        using var processor = new WhisperProcessor(options, native);

        var actualException = Assert.Throws<InvalidOperationException>(() => processor.Process(new float[160]));

        Assert.Same(expectedException, actualException);
        Assert.True(native.NativeCallCompleted);
    }

    private sealed class FakeNativeParakeet : INativeParakeet
    {
        private readonly IntPtr text = AllocateUtf8("hello");

        public FakeNativeParakeet()
        {
            Parakeet_Init_With_Params_No_State =
                (ref WhisperModelLoader loader, ParakeetContextParams parameters) => new IntPtr(1);
            Parakeet_Free = _ => { };
            Parakeet_Init_State = _ => new IntPtr(2);
            Parakeet_Free_State = _ => { };
            Parakeet_Full_Default_Params_By_Ref = strategy =>
            {
                var pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ParakeetFullParams>());
                Marshal.StructureToPtr(
                    new ParakeetFullParams { Strategy = strategy, Threads = 1 },
                    pointer,
                    false);
                return pointer;
            };
            Parakeet_Free_Params = Marshal.FreeHGlobal;
            Parakeet_Full_With_State = (_, state, parameters, _, _) =>
            {
                LastParameters = parameters;
                var callback = Marshal.GetDelegateForFunctionPointer<ParakeetNewSegmentCallback>(
                    parameters.OnNewSegment);
                callback(new IntPtr(1), state, 1, parameters.OnNewSegmentUserData);
                NativeCallCompleted = true;
                return 0;
            };
            Parakeet_Full_N_Segments_From_State = _ => 1;
            Parakeet_Full_Get_Segment_T0_From_State = (_, _) => 10;
            Parakeet_Full_Get_Segment_T1_From_State = (_, _) => 25;
            Parakeet_Full_Get_Segment_Text_From_State = (_, _) => text;
            Parakeet_Full_N_Tokens_From_State = (_, _) => 1;
            Parakeet_Full_Get_Token_Data_From_State = (_, _, _) => new ParakeetTokenData
            {
                Id = 42,
                Probability = 0.75f,
                ProbabilityLog = -0.25f,
                Start = 10,
                End = 25,
                IsWordStart = 1
            };
            Parakeet_Full_Get_Token_Text_From_State = (_, _, _, _) => text;
            Parakeet_Full_Get_Token_P_From_State = (_, _, _) => 0.75f;
            Parakeet_Print_System_Info = () => text;
        }

        public ParakeetFullParams LastParameters { get; private set; }

        public bool NativeCallCompleted { get; private set; }

        public INativeParakeet.parakeet_init_with_params_no_state Parakeet_Init_With_Params_No_State { get; }
        public INativeParakeet.parakeet_free Parakeet_Free { get; }
        public INativeParakeet.parakeet_init_state Parakeet_Init_State { get; }
        public INativeParakeet.parakeet_free_state Parakeet_Free_State { get; }
        public INativeParakeet.parakeet_full_default_params_by_ref Parakeet_Full_Default_Params_By_Ref { get; }
        public INativeParakeet.parakeet_free_params Parakeet_Free_Params { get; }
        public INativeParakeet.parakeet_full_with_state Parakeet_Full_With_State { get; }
        public INativeParakeet.parakeet_full_n_segments_from_state Parakeet_Full_N_Segments_From_State { get; }
        public INativeParakeet.parakeet_full_get_segment_t0_from_state Parakeet_Full_Get_Segment_T0_From_State { get; }
        public INativeParakeet.parakeet_full_get_segment_t1_from_state Parakeet_Full_Get_Segment_T1_From_State { get; }
        public INativeParakeet.parakeet_full_get_segment_text_from_state Parakeet_Full_Get_Segment_Text_From_State { get; }
        public INativeParakeet.parakeet_full_n_tokens_from_state Parakeet_Full_N_Tokens_From_State { get; }
        public INativeParakeet.parakeet_full_get_token_data_from_state Parakeet_Full_Get_Token_Data_From_State { get; }
        public INativeParakeet.parakeet_full_get_token_text_from_state Parakeet_Full_Get_Token_Text_From_State { get; }
        public INativeParakeet.parakeet_full_get_token_p_from_state Parakeet_Full_Get_Token_P_From_State { get; }
        public INativeParakeet.parakeet_print_system_info Parakeet_Print_System_Info { get; }

        public void Dispose()
        {
            Marshal.FreeHGlobal(text);
        }

        private static IntPtr AllocateUtf8(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value + "\0");
            var pointer = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, pointer, bytes.Length);
            return pointer;
        }
    }
}
