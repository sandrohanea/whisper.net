using System.Runtime.InteropServices;
using Whisper.net.Native;
using Whisper.net.SamplingStrategy;
using Whisper.net.Wave;

namespace Whisper.net
{
    public sealed class WhisperProcessor : IDisposable
    {
        private const byte trueByte = 1;
        private const byte falseByte = 0;

        private static readonly Lazy<bool> libraryLoaded = new(() =>
        {
            return NativeLibraryLoader.LoadNativeLibrary();
        }, true);

        private IntPtr currentWhisperContext;
        private readonly WhisperProcessorOptions options;
        private WhisperFullParams whisperParams;
        private IntPtr? language;
        private IntPtr? initialPrompt;
        private WhisperNewSegmentCallback? newSegmentCallback;
        private WhisperEncoderBeginCallback? whisperEncoderBeginCallback;
        private int segmentIndex;

        internal WhisperProcessor(WhisperProcessorOptions options)
        {
            if (!libraryLoaded.Value)
            {
                throw new Exception("Failed to load native whisper library.");
            }

            this.options = options;
        }

        internal void Initialize()
        {
            currentWhisperContext = options.ModelLoader!.LoadNativeContext();
            whisperParams = GetWhisperParams();
        }

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
        public unsafe string? DetectLanguage(float[] samples, bool speedUp = false)
        {
            var (language, _) = DetectLanguageWithProbability(samples, speedUp);
            return language;
        }
        
        public unsafe (string? language, float probability) DetectLanguageWithProbability(float[] samples, bool speedUp = false)
        {
            var probs = new float[NativeMethods.whisper_lang_max_id()];

            fixed (float* pData = probs)
            {
                fixed (float* pSamples = samples)
                {
                    if (speedUp)
                    {
                        // whisper_pcm_to_mel_phase_vocoder is not yet exported from whisper.cpp
                        NativeMethods.whisper_pcm_to_mel_phase_vocoder(currentWhisperContext, (IntPtr)pSamples, samples.Length, whisperParams.Threads);
                    }
                    else
                    {
                        NativeMethods.whisper_pcm_to_mel(currentWhisperContext, (IntPtr)pSamples, samples.Length, whisperParams.Threads);
                    }
                }
                var langId = NativeMethods.whisper_lang_auto_detect(currentWhisperContext, 0, whisperParams.Threads, (IntPtr)pData);
                if (langId == -1)
                {
                    return (null, 0f);
                }
                var languagePtr = NativeMethods.whisper_lang_str(langId);
                var language = Marshal.PtrToStringAnsi(languagePtr);
                return (language, probs[langId]);
            }
        }
        
        public unsafe void Process(float[] samples)
        {
            segmentIndex = 0;
            fixed (float* pData = samples)
            {
                NativeMethods.whisper_full(currentWhisperContext, whisperParams, (IntPtr)pData, samples.Length);
            }
        }


        /// <summary>
        /// Returns the auto-detected language from the last call to Process
        /// </summary>
        /// <returns></returns>
        public string? GetAutodetectedLanguage()
        {
            var detectedLanguageId = NativeMethods.whisper_full_lang_id(currentWhisperContext);
            if (detectedLanguageId == -1)
            {
                return null;
            }
            
            var languagePtr = NativeMethods.whisper_lang_str(detectedLanguageId);
            var language = Marshal.PtrToStringAnsi(languagePtr);
            return language;
        }

        public void Process(Stream waveStream)
        {
            var waveParser = new WaveParser(waveStream);

            var samples = waveParser.GetAvgSamples();

            Process(samples);
        }

        private WhisperFullParams GetWhisperParams()
        {
            var strategy = options.SamplingStrategy.GetNativeStrategy();
            var whisperParams = NativeMethods.whisper_full_default_params(strategy);

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

            if (options.SpeedUp2x.HasValue)
            {
                whisperParams.SpeedUp2x = options.SpeedUp2x.Value ? trueByte : falseByte;
            }

            if (options.AudioContextSize.HasValue)
            {
                whisperParams.AudioContextSize = options.AudioContextSize.Value;
            }

            if (!string.IsNullOrEmpty(options.Prompt))
            {
                var tokenMaxLength = options.Prompt!.Length + 1;

                var promptTextPtr = Marshal.StringToHGlobalAnsi(options.Prompt);

                initialPrompt = Marshal.AllocHGlobal(tokenMaxLength);

                var tokens = NativeMethods.whisper_tokenize(currentWhisperContext, promptTextPtr, initialPrompt.Value, tokenMaxLength);

                Marshal.FreeHGlobal(promptTextPtr);

                if (tokens == -1)
                {
                    throw new ApplicationException("Cannot tokenize prompt text.");
                }

                whisperParams.PromptTokens = initialPrompt.Value;
                whisperParams.PromptNTokens = tokens;
            }
            //TODO: Fix prompt

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

            // Store the delegates so they won't be GC before the processor.
            newSegmentCallback = new WhisperNewSegmentCallback(OnNewSegment);
            whisperEncoderBeginCallback = new WhisperEncoderBeginCallback(OnEncoderBegin);

            whisperParams.OnNewSegment = Marshal.GetFunctionPointerForDelegate(newSegmentCallback);
            whisperParams.OnEncoderBegin = Marshal.GetFunctionPointerForDelegate(whisperEncoderBeginCallback);

            return whisperParams;
        }

        private bool OnEncoderBegin(IntPtr ctx, IntPtr user_data)
        {
            var encoderBeginArgs = new OnEncoderBeginEventArgs();
            for (var handlerIndex = 0; handlerIndex < options.OnEncoderBeginEventHandlers.Count; handlerIndex++)
            {
                options.OnEncoderBeginEventHandlers[handlerIndex]?.Invoke(this, encoderBeginArgs);
            }
            return true;
        }

        private void OnNewSegment(IntPtr ctx, int n_new, IntPtr user_data)
        {
            var segments = NativeMethods.whisper_full_n_segments(ctx);

            while (segmentIndex < segments)
            {
                var t1 = TimeSpan.FromMilliseconds(NativeMethods.whisper_full_get_segment_t1(ctx, segmentIndex) * 10);
                var t0 = TimeSpan.FromMilliseconds(NativeMethods.whisper_full_get_segment_t0(ctx, segmentIndex) * 10);
                var textAnsi = Marshal.PtrToStringAnsi(NativeMethods.whisper_full_get_segment_text(ctx, segmentIndex));
                var eventHandlerArgs = new OnSegmentEventArgs(textAnsi, t0, t1);

                for (var handlerIndex = 0; handlerIndex < options.OnSegmentEventHandlers.Count; handlerIndex++)
                {
                    options.OnSegmentEventHandlers[handlerIndex]?.Invoke(this, eventHandlerArgs);
                }
                segmentIndex++;
            }
        }
        
        public void Dispose()
        {
            NativeMethods.whisper_free(currentWhisperContext);
            if (language.HasValue)
            {
                Marshal.FreeHGlobal(language.Value);
            }

            if (initialPrompt.HasValue)
            {
                Marshal.FreeHGlobal(initialPrompt.Value);
            }

            options.ModelLoader!.Dispose();
        }
    }
}
