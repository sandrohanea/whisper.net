// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Internals.ModelLoader;
using Whisper.net.LibraryLoader;
using Whisper.net.Logger;

namespace Whisper.net;

/// <summary>
/// A factory for creating <seealso cref="WhisperProcessorBuilder"/> used to initialize the process.
/// </summary>
/// <remarks>
/// The factory is loading the model and it is reusing it across all the processors.
/// </remarks>
public sealed class WhisperFactory : IDisposable
{
    private readonly IWhisperProcessorModelLoader loader;
    private readonly Lazy<IntPtr> contextLazy;
    private readonly bool isEagerlyInitialized;
    private bool wasDisposed;

    private static readonly Lazy<LoadResult> libraryLoaded = new(() =>
    {
        var libraryLoaded = NativeLibraryLoader.LoadNativeLibrary();
        if (libraryLoaded.IsSuccess)
        {
            LogProvider.InitializeLogging(libraryLoaded.NativeWhisper!);
        }
        return libraryLoaded;
    }, true);

    private WhisperFactory(IWhisperProcessorModelLoader loader, bool delayInit)
    {
        CheckLibraryLoaded();

        this.loader = loader;
        if (!delayInit)
        {
            var nativeContext = loader.LoadNativeContext(libraryLoaded.Value.NativeWhisper!);
            isEagerlyInitialized = true;

#if NET8_0_OR_GREATER
            contextLazy = new Lazy<IntPtr>(nativeContext);
#else
            contextLazy = new Lazy<IntPtr>(() => nativeContext);
#endif
        }
        else
        {
            contextLazy = new Lazy<IntPtr>(() => loader.LoadNativeContext(libraryLoaded.Value.NativeWhisper!), isThreadSafe: false);
        }
    }

    /// <summary>
    /// Returns the information about the loaded native runtime.
    /// </summary>
    /// <remarks>
    /// This information includes support of the features like AVX, AVX2, AVX512, CUDA, etc.
    /// </remarks>
    /// <exception cref="Exception"></exception>
    public static string? GetRuntimeInfo()
    {
        CheckLibraryLoaded();

        var systemInfoPtr = libraryLoaded.Value.NativeWhisper!.WhisperPrintSystemInfo();
        var systemInfoStr = Marshal.PtrToStringAnsi(systemInfoPtr);
        Marshal.FreeHGlobal(systemInfoPtr);
        return systemInfoStr;
    }

    /// <summary>
    /// Returns an enumerable of the supported languages.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetSupportedLanguages()
    {
        CheckLibraryLoaded();

        for (var i = 0; i < libraryLoaded.Value.NativeWhisper!.Whisper_Lang_Max_Id(); i++)
        {
            var languagePtr = libraryLoaded.Value.NativeWhisper!.Whisper_Lang_Str(i);
            var language = Marshal.PtrToStringAnsi(languagePtr);
            if (!string.IsNullOrEmpty(language))
            {
                yield return language;
            }
        }
    }

    /// <summary>
    /// Creates a factory that uses the ggml model from a buffer in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="path">The path to the model.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromPath(string path)
    {
        return FromPath(path, WhisperFactoryOptions.Default);
    }

    /// <summary>
    /// Creates a factory that uses the ggml model from a path in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="path">The path to the model.</param>
    /// <param name="options">The options for the factory and the loading of the model.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromPath(string path, WhisperFactoryOptions options)
    {
        return new WhisperFactory(new WhisperProcessorModelFileLoader(path, options), options.DelayInitialization);
    }

    /// <summary>
    /// Creates a factory that uses the ggml model from a buffer in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="buffer">The buffer with the model.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromBuffer(byte[] buffer)
    {
        return FromBuffer(buffer, WhisperFactoryOptions.Default);
    }

    /// <summary>
    /// Creates a factory that uses the ggml model from a buffer in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="buffer">The buffer with the model.</param>
    /// <param name="options">Thhe options for the factory and the loading of the model.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromBuffer(byte[] buffer, WhisperFactoryOptions options)
    {
        return new WhisperFactory(new WhisperProcessorModelBufferLoader(buffer, options), options.DelayInitialization);
    }

    /// <summary>
    /// Creates a builder that can be used to initialize the whisper processor.
    /// </summary>
    /// <returns>An instance to a new builder.</returns>
    /// <exception cref="ObjectDisposedException">Throws if the factory was already disposed.</exception>
    /// <exception cref="WhisperModelLoadException">Throws if the model couldn't be loaded.</exception>
    public WhisperProcessorBuilder CreateBuilder()
    {
#if NET8_0_OR_GREATER
        ObjectDisposedException.ThrowIf(wasDisposed, this);
#else
        if (wasDisposed)
        {
            throw new ObjectDisposedException(nameof(WhisperFactory));
        }
#endif

        var context = contextLazy.Value;
        if (context == IntPtr.Zero)
        {
            throw new WhisperModelLoadException("Failed to load the whisper model.");
        }

        return new WhisperProcessorBuilder(contextLazy.Value, libraryLoaded.Value.NativeWhisper!);
    }

    public void Dispose()
    {
        if (wasDisposed)
        {
            return;
        }

        // Even if the Lazy value was not created, we still need to free the context if it was eagerly initialized.
        if ((contextLazy.IsValueCreated || isEagerlyInitialized) && contextLazy.Value != IntPtr.Zero)
        {
            libraryLoaded.Value.NativeWhisper!.Whisper_Free(contextLazy.Value);
        }
        loader.Dispose();
        wasDisposed = true;
    }

    private static void CheckLibraryLoaded()
    {
        if (!libraryLoaded.Value.IsSuccess)
        {
            throw new Exception($"Failed to load native whisper library. Error: {libraryLoaded.Value.ErrorMessage}");
        }
    }
}
