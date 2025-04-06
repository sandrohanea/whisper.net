// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Microsoft.Extensions.AI;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net;

public static class SpeechToTextOptionsExtensions
{
    internal const string BeamSearchSamplingStrategyKey = "BeamSearchSamplingStrategy";
    internal const string AudioContextSizeKey = "AudioContextSize";

    public static SpeechToTextOptions WithLanguage(this SpeechToTextOptions options, string language)
    {
        options.SpeechLanguage = language;
        return options;
    }

    public static SpeechToTextOptions WithBeamSearchSamplingStrategy(this SpeechToTextOptions options)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[BeamSearchSamplingStrategyKey] = true;

        return options;
    }

    public static SpeechToTextOptions WithAudioContextSize(this SpeechToTextOptions options, int audioContextSize)
    {
        options.AdditionalProperties ??= [];
        options.AdditionalProperties[AudioContextSizeKey] = audioContextSize;
        return options;
    }
}
