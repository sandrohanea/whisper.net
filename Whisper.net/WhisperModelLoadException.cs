// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Exception thrown by the <seealso cref="WhisperProcessor"/> if the provided model couldn't be loaded.
/// </summary>
/// <remarks>
/// Check if the path to the model is correct and if the appropiate version of the model is used.
/// </remarks>
/// <param name="message"></param>
public class WhisperModelLoadException(string message) : Exception(message)
{
}
