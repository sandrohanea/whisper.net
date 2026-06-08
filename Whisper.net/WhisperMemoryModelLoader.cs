// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Loads a model from managed memory.
/// </summary>
public sealed class WhisperMemoryModelLoader : IWhisperModelLoader
{
    private readonly ReadOnlyMemory<byte> memory;
    private int offset;

    public WhisperMemoryModelLoader(ReadOnlyMemory<byte> memory)
    {
        this.memory = memory;
    }

    /// <inheritdoc />
    public bool IsEof => offset >= memory.Length;

    /// <inheritdoc />
    public void Reset()
    {
        offset = 0;
    }

    /// <inheritdoc />
    public int CopyTo(Span<byte> destination)
    {
        var bytesToCopy = Math.Min(destination.Length, memory.Length - offset);
        if (bytesToCopy <= 0)
        {
            return 0;
        }

        memory.Span.Slice(offset, bytesToCopy).CopyTo(destination);
        offset += bytesToCopy;
        return bytesToCopy;
    }

    /// <inheritdoc />
    public void Close()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
