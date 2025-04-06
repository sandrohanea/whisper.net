// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.AI;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net;

public sealed class WhisperSpeechToTextClient : ISpeechToTextClient
{
    private readonly WhisperFactory _factory;
    private WhisperProcessor? _processor;

    public WhisperSpeechToTextClient(string modelFileName)
    {
        this._factory = WhisperFactory.FromPath(modelFileName);
    }

    public void Dispose()
    {
        if (this._processor != null)
        {
            this._processor.Dispose();
        }

        if (this._factory != null)
        {
            this._factory.Dispose();
        }
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        throw new NotImplementedException();
    }

    public async IAsyncEnumerable<SpeechToTextResponseUpdate> GetStreamingTextAsync(Stream audioSpeechStream, SpeechToTextOptions? options = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (audioSpeechStream is null)
        {
            throw new ArgumentNullException(nameof(audioSpeechStream));
        }

        PrepareProcessor(options);

        var responseId = Guid.NewGuid().ToString();
        await foreach (var segment in this._processor!.ProcessAsync(audioSpeechStream, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            yield return new SpeechToTextResponseUpdate(segment.Text)
            {
                ResponseId = responseId,
                Kind = SpeechToTextResponseUpdateKind.TextUpdating,
                RawRepresentation = segment,
                StartTime = segment.Start,
                EndTime = segment.End
            };
        }
    }

    public async Task<SpeechToTextResponse> GetTextAsync(Stream audioSpeechStream, SpeechToTextOptions? options = null, CancellationToken cancellationToken = default)
    {
        SpeechToTextResponse response = new();
        PrepareProcessor(options);

        StringBuilder fullTranscription = new();
        List<SegmentData> segments = [];

        await foreach (var segment in this._processor!.ProcessAsync(audioSpeechStream, cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            response.StartTime ??= segment.Start;
            response.EndTime = segment.End;

            segments.Add(segment);
            fullTranscription.Append(segment.Text);
        }

        response.ResponseId = Guid.NewGuid().ToString();
        response.RawRepresentation = segments;
        response.Contents = [new TextContent(fullTranscription.ToString())];

        return response;
    }

    private void PrepareProcessor(SpeechToTextOptions? options)
    {
        if (this._processor is not null)
        {
            return;
        }
        
        var processorBuilder = this._factory.CreateBuilder();
        if (options is not null)
        {
            if (!string.IsNullOrWhiteSpace(options?.SpeechLanguage))
            {
                processorBuilder.WithLanguage(options!.SpeechLanguage!);
            }

            if (GetAdditionalProperty<int>(SpeechToTextOptionsExtensions.AudioContextSizeKey, options!, out var audioContextSize))
            {
                processorBuilder.WithAudioContextSize(audioContextSize);
            }

            if (GetAdditionalProperty<bool>(SpeechToTextOptionsExtensions.BeamSearchSamplingStrategyKey, options!, out var beamSearchSamplingStrategy) && beamSearchSamplingStrategy)
            {
                processorBuilder.WithBeamSearchSamplingStrategy();
            }

            /*
            processorBuilder.WithDuration(options?.Duration ?? TimeSpan.MinValue);
            processorBuilder.WithEncoderBeginHandler(options?.EncoderBeginHandler);
            processorBuilder.WithEntropyThreshold(options?.EntropyThreshold ?? 0.0f);

            if (GetAdditionalProperty<bool>("GreedySamplingStrategy", options!, out var greedySamplingStrategy) && greedySamplingStrategy)
            {
                processorBuilder.WithGreedySamplingStrategy();
            }

            processorBuilder.WithLanguage(options?.Language ?? string.Empty);
            processorBuilder.WithLanguageDetection(options?.LanguageDetection ?? false);
            processorBuilder.WithLengthPenalty(options?.LengthPenalty ?? 0.0f);
            processorBuilder.WithLogProbThreshold(options?.LogProbThreshold ?? 0.0f);
            processorBuilder.WithMaxInitialTs(options?.MaxInitialTs ?? 0);
            processorBuilder.WithMaxSegmentLength(options?.MaxSegmentLength ?? 0);
            processorBuilder.WithMaxLastTextTokens(options?.MaxLastTextTokens ?? 0);
            processorBuilder.WithMaxTokensPerSegment(options?.MaxTokensPerSegment ?? 0);
            processorBuilder.WithNoContext(options?.NoContext ?? false);
            processorBuilder.WithNoSpeechThreshold(options?.NoSpeechThreshold ?? 0.0f);
            processorBuilder.WithOffset(options?.Offset ?? 0);
            processorBuilder.WithOpenVinoEncoder(options?.OpenVinoEncoderPath, options?.OpenVinoDevice, options?.OpenVinoCachePath);
            processorBuilder.WithoutSuppressBlank();
            processorBuilder.WithoutStringPool();
            processorBuilder.WithPrintProgress(options?.PrintProgress ?? false);
            processorBuilder.WithPrintTimestamps(options?.PrintTimestamps ?? false);
            processorBuilder.WithPrintSpecialTokens(options?.PrintSpecialTokens ?? false);
            processorBuilder.WithPrintResults(options?.PrintResults ?? false);
            processorBuilder.WithProbabilities(options?.Probabilities ?? false);
            processorBuilder.WithProgressHandler(options?.ProgressHandler);
            processorBuilder.WithSegmentEventHandler(options?.SegmentEventHandler);
            processorBuilder.WithStringPool(options?.StringPool ?? string.Empty);
            processorBuilder.WithTemperature(options?.Temperature ?? 0.0f);
            processorBuilder.WithTemperatureInc(options?.TemperatureInc ?? 0.0f);
            processorBuilder.WithThreads(options?.Threads ?? 0);
            processorBuilder.WithTokenTimestamps();
            processorBuilder.WithTokenTimestampsSumThreshold(options?.TokenTimestampsSumThreshold ?? 0.0f);
            processorBuilder.WithTokenTimestampsThreshold(options?.TokenTimestampsThreshold ?? 0.0f);
            */
        }

        this._processor = processorBuilder.Build();
    }

    private static bool GetAdditionalProperty<T>(string propertyName, SpeechToTextOptions options, out T? value)
    {
        if (options.AdditionalProperties?.TryGetValue(propertyName, out value) ?? false)
        {
            return true;
        }

        value = default;
        return false;
    }
}
