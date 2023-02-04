using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whisper.net;

public interface IWhisperSamplingStrategyBuilder
{
    /// <summary>
    /// Returns the parent <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    WhisperProcessorBuilder ParentBuilder { get; }
}
