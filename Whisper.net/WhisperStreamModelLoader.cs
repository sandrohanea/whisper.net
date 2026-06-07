// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Buffers;

namespace Whisper.net;

/// <summary>
/// Loads a model from a managed stream.
/// </summary>
public sealed class WhisperStreamModelLoader : IWhisperModelLoader
{
    private readonly Stream stream;
    private readonly bool leaveOpen;
    private readonly long initialPosition;
    private bool hasRead;
    private bool isDisposed;
    private bool reachedEnd;

    public WhisperStreamModelLoader(Stream stream, bool leaveOpen = false)
    {
        this.stream = stream ?? throw new ArgumentNullException(nameof(stream));
        if (!stream.CanRead)
        {
            throw new ArgumentException("The model stream must be readable.", nameof(stream));
        }

        this.leaveOpen = leaveOpen;
        initialPosition = stream.CanSeek ? stream.Position : 0;
    }

    /// <inheritdoc />
    public bool IsEof => reachedEnd || (stream.CanSeek && stream.Position >= stream.Length);

    /// <inheritdoc />
    public void Reset()
    {
        ThrowIfDisposed();

        if (stream.CanSeek)
        {
            stream.Position = initialPosition;
            reachedEnd = false;
            return;
        }

        if (hasRead)
        {
            throw new InvalidOperationException("Cannot reset a non-seekable model stream after it was read.");
        }

        reachedEnd = false;
    }

    /// <inheritdoc />
    public int CopyTo(Span<byte> destination)
    {
        ThrowIfDisposed();

        if (destination.Length == 0)
        {
            return 0;
        }

        hasRead = true;
#if NETSTANDARD
        var buffer = ArrayPool<byte>.Shared.Rent(destination.Length);
        try
        {
            var bytesRead = stream.Read(buffer, 0, destination.Length);
            buffer.AsSpan(0, bytesRead).CopyTo(destination);
            reachedEnd = bytesRead == 0;
            return bytesRead;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
#else
        var read = stream.Read(destination);
        reachedEnd = read == 0;
        return read;
#endif
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        if (!leaveOpen)
        {
            stream.Dispose();
        }

        isDisposed = true;
    }

    private void ThrowIfDisposed()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(isDisposed, this);
#else
        if (isDisposed)
        {
            throw new ObjectDisposedException(nameof(WhisperStreamModelLoader));
        }
#endif
    }
}
