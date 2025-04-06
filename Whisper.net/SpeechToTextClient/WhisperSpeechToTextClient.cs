// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.AI;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net;

public sealed class WhisperSpeechToTextClient(string modelFileName) : ISpeechToTextClient
{
    private readonly WhisperFactory _factory = WhisperFactory.FromPath(modelFileName);
    private WhisperProcessor? _processor;

    public void Dispose()
    {
        _processor?.Dispose();
        _factory?.Dispose();
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

        this._processor ??= options.BuildWhisperProcessor(_factory);

        var responseId = Guid.NewGuid().ToString();
        await foreach (var segment in _processor!.ProcessAsync(audioSpeechStream, cancellationToken))
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
        if (audioSpeechStream is null)
        {
            throw new ArgumentNullException(nameof(audioSpeechStream));
        }

        SpeechToTextResponse response = new();

        this._processor ??= options.BuildWhisperProcessor(_factory);

        StringBuilder fullTranscription = new();
        List<SegmentData> segments = [];

        await foreach (var segment in _processor!.ProcessAsync(audioSpeechStream, cancellationToken))
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
}
