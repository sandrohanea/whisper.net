using System.Runtime.InteropServices;
using System.Text;
using Whisper.net.Native;
using Whisper.net.SamplingStrategy;
using Whisper.net.Wave;
using static System.Net.Mime.MediaTypeNames;

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

        private readonly IntPtr currentWhisperContext;
        private readonly WhisperProcessorOptions options;

        internal WhisperProcessor(WhisperProcessorOptions options)
        {
            if (!libraryLoaded.Value)
            {
                throw new Exception("Failed to load native whisper library.");
            }

            currentWhisperContext = options.ModelLoader!.LoadNativeContext();
            this.options = options;
        }

        public void Process(Stream waveStream)
        {
            var waveParser = new WaveParser(waveStream);

            var samples = waveParser.GetAvgSamples();

            var (whisperParams, unmanagedHGlobals) = GetWhisperFullParams();

            try
            {
                unsafe
                {
                    fixed (float* pData = samples)
                    {
                        NativeMethods.whisper_full(currentWhisperContext, whisperParams, (IntPtr)pData, samples.Length);
                    }
                }
            }
            finally
            {
                foreach (var unmanagedHGlobal in unmanagedHGlobals)
                {
                    Marshal.FreeHGlobal(unmanagedHGlobal);
                }
            }
        }

        private (WhisperFullParams, List<IntPtr>) GetWhisperFullParams()
        {
            var unmanagedHGlobals = new List<IntPtr>();

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
                unmanagedHGlobals.Add(promptTextPtr);

                var promptTokens = Marshal.AllocHGlobal(tokenMaxLength);
                unmanagedHGlobals.Add(promptTokens);

                var tokens = NativeMethods.whisper_tokenize(currentWhisperContext, promptTextPtr, promptTokens, tokenMaxLength);

                if (tokens == -1)
                {
                    throw new ApplicationException("Cannot tokenize prompt text.");
                }

                whisperParams.PromptTokens = promptTokens;
                whisperParams.PromptNTokens = tokens;
            }
            //TODO: Fix prompt

            if (options.Language != null)
            {
                var languagePtr = Marshal.StringToHGlobalAnsi(options.Language);
                unmanagedHGlobals.Add(languagePtr);
                whisperParams.Language = languagePtr;
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

            if (options.EntropyThreshhold.HasValue)
            {
                whisperParams.EntropyThreshhold = options.EntropyThreshhold.Value;
            }

            if (options.LogProbThreshhold.HasValue)
            {
                whisperParams.LogProbThreshhold = options.LogProbThreshhold.Value;
            }

            if (options.NoSpeechThreshhold.HasValue)
            {
                whisperParams.NoSpeechThreshhold = options.NoSpeechThreshhold.Value;
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

            whisperParams.OnNewSegment = Marshal.GetFunctionPointerForDelegate(new WhisperNewSegmentCallback(OnNewSegment));
            whisperParams.OnEncoderBegin = Marshal.GetFunctionPointerForDelegate(new WhisperEncoderBeginCallback(OnEncoderBegin));

            return (whisperParams, unmanagedHGlobals);
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

            for (var i = 0; i < segments; i++)
            {
                var t1 = TimeSpan.FromMilliseconds(NativeMethods.whisper_full_get_segment_t1(ctx, i) * 10);
                var t0 = TimeSpan.FromMilliseconds(NativeMethods.whisper_full_get_segment_t0(ctx, i) * 10);
                var textAnsi = Marshal.PtrToStringAnsi(NativeMethods.whisper_full_get_segment_text(ctx, i));
                var eventHandlerArgs = new OnSegmentEventArgs(textAnsi, t0, t1);

                for (var handlerIndex = 0; handlerIndex < options.OnSegmentEventHandlers.Count; handlerIndex++)
                {
                    options.OnSegmentEventHandlers[handlerIndex]?.Invoke(this, eventHandlerArgs);
                }
            }
        }

        public void Dispose()
        {
            NativeMethods.whisper_free(currentWhisperContext);
            options.ModelLoader!.Dispose();
        }
    }
}
