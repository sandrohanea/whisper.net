// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Wave;

/// <summary>
/// Thrown by the <seealso cref="WaveParser"/> when the wave stream is corrupted and cannot be parsed.
/// </summary>
/// <param name="message"></param>
public class CorruptedWaveException(string? message) : Exception(message)
{
}

/// <summary>
/// Thrown by the <seealso cref="WaveParser"/> when the wave stream is not currently supported.
/// </summary>
/// <param name="message"></param>
public class NotSupportedWaveException(string? message) : Exception(message)
{
}
