// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals;
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
    private readonly StringPool stringPool = new();
    private bool wasDisposed;

    private static readonly Lazy<LoadResult> LibraryLoaded = new(() =>
    {
        var localLibraryLoaded = NativeLibraryLoader.LoadNativeLibrary();
        if (localLibraryLoaded.IsSuccess)
        {
            LogProvider.InitializeLogging(localLibraryLoaded.NativeWhisper!);
        }
        return localLibraryLoaded;
    }, true);

    private WhisperFactory(IWhisperProcessorModelLoader loader, bool delayInit)
    {
        CheckLibraryLoaded();

        this.loader = loader;
        if (!delayInit)
        {
            var nativeContext = loader.LoadNativeContext(LibraryLoaded.Value.NativeWhisper!);
            isEagerlyInitialized = true;

#if NET8_0_OR_GREATER
            contextLazy = new Lazy<IntPtr>(nativeContext);
#else
            contextLazy = new Lazy<IntPtr>(() => nativeContext);
#endif
        }
        else
        {
            contextLazy = new Lazy<IntPtr>(() => loader.LoadNativeContext(LibraryLoaded.Value.NativeWhisper!), isThreadSafe: false);
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

        var systemInfoPtr = LibraryLoaded.Value.NativeWhisper!.WhisperPrintSystemInfo();
        var systemInfoStr = MarshalUtils.GetString(systemInfoPtr);
        // The pointer returned by WhisperPrintSystemInfo points to a static
        // buffer owned by the native library. Do not free it here.
        return systemInfoStr;
    }

    /// <summary>
    /// Returns an enumerable of the supported languages.
    /// </summary>
    /// <returns></returns>
    public static IEnumerable<string> GetSupportedLanguages()
    {
        CheckLibraryLoaded();

        for (var i = 0; i < LibraryLoaded.Value.NativeWhisper!.Whisper_Lang_Max_Id(); i++)
        {
            var languagePtr = LibraryLoaded.Value.NativeWhisper!.Whisper_Lang_Str(i);
            var language = MarshalUtils.GetString(languagePtr);
            if (!string.IsNullOrEmpty(language))
            {
                // Null suppression is redundant, but some CS8603 false positives are reported by the analyzer when using the yield return + string from MarshalUtils.GetString.
                yield return language!;
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
    /// Creates a factory that uses the ggml model from a buffer in memory in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="memory">The memory buffer with the model.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromBuffer(Memory<byte> memory)
    {
        return FromBuffer(memory, WhisperFactoryOptions.Default);
    }

    /// <summary>
    /// Creates a factory that uses the ggml model from a buffer in memory in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="memory">The memory buffer with the model.</param>
    /// <param name="options">Thhe options for the factory and the loading of the model.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromBuffer(Memory<byte> memory, WhisperFactoryOptions options)
    {
        return new WhisperFactory(new WhisperProcessorModelMemoryLoader(memory, options), options.DelayInitialization);
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

        return new WhisperProcessorBuilder(contextLazy.Value, LibraryLoaded.Value.NativeWhisper!, stringPool);
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
            LibraryLoaded.Value.NativeWhisper!.Whisper_Free(contextLazy.Value);
        }
        loader.Dispose();
        wasDisposed = true;
    }

    private static void CheckLibraryLoaded()
    {
        if (!LibraryLoaded.Value.IsSuccess)
        {
            throw new Exception($"Failed to load native whisper library. Error: {LibraryLoaded.Value.ErrorMessage}");
        }
    }
}
