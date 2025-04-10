// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.AI;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net;

/// <summary>
/// Client for speech-to-text operations using Whisper models.
/// </summary>
public sealed class WhisperSpeechToTextClient : ISpeechToTextClient
{
    private readonly Func<WhisperFactory> _buildFactoryFunc;
    private WhisperFactory? _factory;
    private readonly object _factoryLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperSpeechToTextClient"/> class.
    /// </summary>
    /// <param name="modelFileName">The path to the model file.</param>
    public WhisperSpeechToTextClient(string modelFileName)
        : this(() => WhisperFactory.FromPath(modelFileName))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperSpeechToTextClient"/> class with a factory builder function.
    /// </summary>
    /// <param name="buildFactoryFunc">A function that creates a WhisperFactory instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when the factory builder is null.</exception>
    public WhisperSpeechToTextClient(Func<WhisperFactory> buildFactoryFunc)
    {
        if (buildFactoryFunc is null)
        {
            throw new ArgumentNullException(nameof(buildFactoryFunc));
        }

        _buildFactoryFunc = buildFactoryFunc;
    }

    /// <summary>
    /// Gets the WhisperFactory instance, creating it if it doesn't exist yet.
    /// </summary>
    /// <returns>The WhisperFactory instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the factory builder returns null.</exception>
    private WhisperFactory GetFactory()
    {
        if (_factory is not null)
        {
            return _factory;
        }

        lock (_factoryLock)
        {
            if (_factory is not null)
            {
                return _factory;
            }

            _factory = _buildFactoryFunc();
            
            if (_factory is null)
            {
                throw new ArgumentNullException(nameof(_factory));
            }

            return _factory;
        }
    }

    public void Dispose()
    {
        lock (_factoryLock)
        {
            _factory?.Dispose();
            _factory = null;
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

        using var processor = options.BuildWhisperProcessor(GetFactory());

        var responseId = Guid.NewGuid().ToString();
        await foreach (var segment in processor.ProcessAsync(audioSpeechStream, cancellationToken))
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

        using var processor = options?.BuildWhisperProcessor(GetFactory()) ?? GetFactory().CreateBuilder().Build();

        StringBuilder fullTranscription = new();
        List<SegmentData> segments = [];

        await foreach (var segment in processor.ProcessAsync(audioSpeechStream, cancellationToken))
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
