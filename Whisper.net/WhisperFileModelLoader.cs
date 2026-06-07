// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net;

/// <summary>
/// Loads a model from a file path.
/// </summary>
public sealed class WhisperFileModelLoader : IWhisperModelLoader
{
    private readonly string path;
    private FileStream? stream;
    private bool isDisposed;
    private bool reachedEnd;

    public WhisperFileModelLoader(string path)
    {
        this.path = path ?? throw new ArgumentNullException(nameof(path));
    }

    /// <inheritdoc />
    public bool IsEof => reachedEnd || (stream != null && stream.Position >= stream.Length);

    /// <inheritdoc />
    public void Reset()
    {
        ThrowIfDisposed();
        stream?.Dispose();
        stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        reachedEnd = false;
    }

    /// <inheritdoc />
    public int CopyTo(Span<byte> destination)
    {
        ThrowIfDisposed();

        if (stream == null)
        {
            throw new InvalidOperationException("The model file loader was not reset before reading.");
        }

        if (destination.Length == 0)
        {
            return 0;
        }

#if NETSTANDARD
        var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(destination.Length);
        try
        {
            var bytesRead = stream.Read(buffer, 0, destination.Length);
            buffer.AsSpan(0, bytesRead).CopyTo(destination);
            reachedEnd = bytesRead == 0;
            return bytesRead;
        }
        finally
        {
            System.Buffers.ArrayPool<byte>.Shared.Return(buffer);
        }
#else
        var read = stream.Read(destination);
        reachedEnd = read == 0;
        return read;
#endif
    }

    /// <inheritdoc />
    public void Close()
    {
        stream?.Dispose();
        stream = null;
        reachedEnd = false;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        Close();
        isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(isDisposed, this);
#else
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(WhisperFileModelLoader));
        }
#endif
    }
}
