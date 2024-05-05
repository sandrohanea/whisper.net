// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

public interface IWhisperSamplingStrategyBuilder
{
  /// <summary>
  /// Returns the parent <seealso cref="WhisperProcessorBuilder"/>.
  /// </summary>
  WhisperProcessorBuilder ParentBuilder { get; }
}
