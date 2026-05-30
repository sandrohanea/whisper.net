// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals;
using Whisper.net.Internals.ModelLoader;

namespace Whisper.net;

/// <summary>
/// A factory for creating <seealso cref="WhisperVadProcessorBuilder"/> used to initialize VAD processing.
/// </summary>
public sealed class WhisperVadFactory : IDisposable
{
    private readonly IWhisperProcessorModelLoader loader;
    private bool wasDisposed;

    private WhisperVadFactory(IWhisperProcessorModelLoader loader)
    {
        _ = WhisperLibrary.NativeWhisper;
        this.loader = loader;
    }

    /// <summary>
    /// Creates a factory that uses the ggml VAD model from a path in order to create <seealso cref="WhisperVadProcessorBuilder"/>.
    /// </summary>
    /// <param name="path">The path to the VAD model.</param>
    /// <returns>An instance to the same builder.</returns>
    public static WhisperVadFactory FromPath(string path)
    {
        return FromPath(path, WhisperFactoryOptions.Default);
    }

    /// <summary>
    /// Creates a factory that uses the ggml VAD model from a path in order to create <seealso cref="WhisperVadProcessorBuilder"/>.
    /// </summary>
    /// <param name="path">The path to the VAD model.</param>
    /// <param name="options">The options for the factory and the loading of the model.</param>
    /// <returns>An instance to the same builder.</returns>
    public static WhisperVadFactory FromPath(string path, WhisperFactoryOptions options)
    {
        return new WhisperVadFactory(new WhisperProcessorModelFileLoader(path, options));
    }

    /// <summary>
    /// Creates a builder that can be used to initialize the whisper VAD processor.
    /// </summary>
    /// <returns>An instance to a new builder.</returns>
    /// <exception cref="ObjectDisposedException">Throws if the factory was already disposed.</exception>
    public WhisperVadProcessorBuilder CreateBuilder()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(wasDisposed, this);
#else
        if (wasDisposed)
        {
            throw new ObjectDisposedException(nameof(WhisperVadFactory));
        }
#endif

        return new WhisperVadProcessorBuilder(loader, WhisperLibrary.NativeWhisper);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (wasDisposed)
        {
            return;
        }

        loader.Dispose();
        wasDisposed = true;
    }
}
