// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals;
using Whisper.net.Internals.Native;
using Whisper.net.Native;
using Xunit;

namespace Whisper.net.Tests;

public class LanguageDetectionTests
{
    [Fact]
    public void DetectLanguageWithProbability_WhenCandidatesProvided_SelectsBestCandidate()
    {
        using var native = CreateNative(votedLanguageId: 2, probabilities: [0.1f, 0.9f, 0.6f], languages: ["en", "fr", "ro"]);
        using var processor = CreateProcessor(native);

        var (language, probability) = processor.DetectLanguageWithProbability(new float[16], new[] { "fr", "en" });

        Assert.Equal("fr", language);
        Assert.Equal(0.9f, probability);
    }

    [Fact]
    public void DetectLanguageWithProbability_WhenCandidatesProvidedAsSpan_SelectsBestCandidate()
    {
        using var native = CreateNative(votedLanguageId: 2, probabilities: [0.1f, 0.9f, 0.6f], languages: ["en", "fr", "ro"]);
        using var processor = CreateProcessor(native);
        var candidates = new[] { "fr", "en" };

        var (language, probability) = processor.DetectLanguageWithProbability(new float[16], candidates.AsSpan());

        Assert.Equal("fr", language);
        Assert.Equal(0.9f, probability);
    }

    [Fact]
    public void DetectLanguageWithProbability_WhenNoCandidatesProvided_ReturnsNativeResult()
    {
        using var native = CreateNative(votedLanguageId: 2, probabilities: [0.1f, 0.9f, 0.6f], languages: ["en", "fr", "ro"]);
        using var processor = CreateProcessor(native);

        var (language, probability) = processor.DetectLanguageWithProbability(new float[16]);

        Assert.Equal("ro", language);
        Assert.Equal(0.6f, probability);
    }

    [Fact]
    public void DetectLanguageWithProbability_WhenCandidatesDontMatch_FallsBackToNativeResult()
    {
        using var native = CreateNative(votedLanguageId: 2, probabilities: [0.1f, 0.9f, 0.6f], languages: ["en", "fr", "ro"]);
        using var processor = CreateProcessor(native);

        var (language, probability) = processor.DetectLanguageWithProbability(new float[16], new[] { "de" });

        Assert.Equal("ro", language);
        Assert.Equal(0.6f, probability);
    }

    private static WhisperProcessor CreateProcessor(INativeWhisper nativeWhisper)
    {
        var options = new WhisperProcessorOptions { ContextHandle = IntPtr.Zero };
        return new WhisperProcessor(options, nativeWhisper);
    }

    private static LanguageDetectionNativeWhisper CreateNative(int votedLanguageId, float[] probabilities, string[] languages) =>
        new(votedLanguageId, probabilities, languages);

    private sealed class LanguageDetectionNativeWhisper : INativeWhisper
    {
        private readonly float[] probabilities;
        private readonly string[] languages;
        private readonly int votedLanguageId;
        private readonly Dictionary<string, int> languageIds;
        private readonly IntPtr[] languagePtrs;

        public LanguageDetectionNativeWhisper(int votedLanguageId, float[] probabilities, string[] languages)
        {
            this.votedLanguageId = votedLanguageId;
            this.probabilities = probabilities;
            this.languages = languages;
            languageIds = languages
                .Select((name, index) => (name, index))
                .ToDictionary(k => k.name, k => k.index, StringComparer.OrdinalIgnoreCase);
            languagePtrs = languages.Select(MarshalUtils.GetStringHGlobalPtr).ToArray();

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
            Whisper_Lang_Max_Id = () => probabilities.Length - 1;
            Whisper_Lang_Id = lang =>
            {
                var name = Marshal.PtrToStringUTF8(lang);
                return name is not null && languageIds.TryGetValue(name, out var id) ? id : -1;
            };
            Whisper_Lang_Auto_Detect_With_State = (_, _, _, _, langProbs) =>
            {
                Marshal.Copy(probabilities, 0, langProbs, probabilities.Length);
                return votedLanguageId;
            };
            Whisper_PCM_To_Mel_With_State = (_, _, _, _, _) => 0;
            Whisper_Lang_Str = langId =>
            {
                if (langId < 0 || langId >= languagePtrs.Length)
                {
                    return IntPtr.Zero;
                }

                return languagePtrs[langId];
            };
            Whisper_Init_State = _ => new IntPtr(1);
            Whisper_Free_State = _ => { };
            Whisper_Full_Lang_Id_From_State = _ => votedLanguageId;
            Whisper_Log_Set = (_, _) => { };
            Whisper_Ctx_Init_Openvino_Encoder_With_State = (_, _, _, _, _) => { };
            Whisper_Full_Get_Token_Data_From_State = (_, _, _) => default;
            Whisper_Full_Get_Token_Text_From_State = (_, _, _, _) => IntPtr.Zero;
            WhisperPrintSystemInfo = () => IntPtr.Zero;
            Whisper_Full_Get_Segment_No_Speech_Prob_From_State = (_, _) => 0;
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

        public void Dispose()
        {
            foreach (var ptr in languagePtrs)
            {
                MarshalUtils.TryReleaseStringHGlobal(ptr);
            }
        }
    }
}
