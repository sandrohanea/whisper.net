// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Native;

namespace Whisper.net.SamplingStrategy;

internal interface IWhisperSamplingStrategy {
  public WhisperSamplingStrategy GetNativeStrategy();
}
