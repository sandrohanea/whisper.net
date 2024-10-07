// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals.ModelLoader;
using Whisper.net.LibraryLoader;
using Whisper.net.Logger;
using Whisper.net.Native;

namespace Whisper.net;

/// <summary>
/// A factory for creating <seealso cref="WhisperProcessorBuilder"/> used to initialize the process.
/// </summary>
/// <remarks>
/// The factory is loading the model and it is reusing it across all the processors.
/// </remarks>
public sealed class WhisperFactory : IDisposable
{
    private static readonly List<RuntimeLibrary> defaultRuntimeOrder = [RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.CoreML, RuntimeLibrary.Cpu];
    private readonly IWhisperProcessorModelLoader loader;
    private readonly Lazy<IntPtr> contextLazy;
    private bool wasDisposed;
    private static bool bypassLoading;
    private static string? libraryPath;
    private static bool useGpu = true;
    private static List<RuntimeLibrary> runtimeLibrary = defaultRuntimeOrder;

    private static readonly Lazy<LoadResult> libraryLoaded = new(() =>
    {
        var libraryLoaded = NativeLibraryLoader.LoadNativeLibrary(bypassLoading, runtimeLibrary);
        if (libraryLoaded.IsSuccess)
        {
            LogProvider.InitializeLogging();
        }
        return libraryLoaded;
    }, true);

    /// <summary>
    /// Optional initialization of the whisper library, to bypass the runtime loading or to set a custom path.
    /// </summary>
    /// <param name="libraryPath">The custom path to be used when loading the runtime library.</param>
    /// <param name="bypassLoading">Bypass loading the library. Use this if you've already loaded the runtime library manually or providing it in path.</param>
    /// <param name="useGpu">A value indicating if the model should be loaded on the GPU or CPU (if runtime with GPU support is installed).</param>
    /// <remarks>
    /// Use this initialization on your own risk if you understand how the runtime library is being loaded, otherwise use the default initialization and do not call this method.
    /// </remarks>
    public static void Initialize(string? libraryPath = null, bool bypassLoading = false, bool useGpu = true, List<RuntimeLibrary>? runtimeLibraryOrder = null)
    {
        WhisperFactory.libraryPath = libraryPath;
        WhisperFactory.bypassLoading = bypassLoading;
        WhisperFactory.useGpu = useGpu;
        WhisperFactory.runtimeLibrary = runtimeLibraryOrder ?? defaultRuntimeOrder;
    }

    private WhisperFactory(IWhisperProcessorModelLoader loader, bool delayInit)
    {
        if (!libraryLoaded.Value.IsSuccess)
        {
            throw new Exception($"Failed to load native whisper library. Error: {libraryLoaded.Value.ErrorMessage}");
        }

        this.loader = loader;
        if (!delayInit)
        {
            var nativeContext = loader.LoadNativeContext();
            contextLazy = new Lazy<IntPtr>(() => nativeContext);
        }
        else
        {
            contextLazy = new Lazy<IntPtr>(() => loader.LoadNativeContext(), isThreadSafe: false);
        }
    }

    /// <summary>
    /// Creates a factory that uses the ggml model from a path in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="path">The path to the model.</param>
    /// <param name="delayInitialization">A value indicating if the model should be loaded right away or during the first <see cref="CreateBuilder"/> call.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromPath(string path, bool delayInitialization = false)
    {
        return new WhisperFactory(new WhisperProcessorModelFileLoader(path, useGpu), delayInitialization);
    }

    /// <summary>
    /// Creates a factory that uses the ggml model from a buffer in order to create <seealso cref="WhisperProcessorBuilder"/>.
    /// </summary>
    /// <param name="buffer">The buffer with the model.</param>
    /// <param name="delayInitialization">A value indicating if the model should be loaded right away or during the first <see cref="CreateBuilder"/> call.</param>
    /// <returns>An instance to the same builder.</returns>
    /// <remarks>
    /// If you don't know where to find a ggml model, you can use <seealso cref="Ggml.WhisperGgmlDownloader"/> which is downloading a model from huggingface.co.
    /// </remarks>
    public static WhisperFactory FromBuffer(byte[] buffer, bool delayInitialization = false)
    {
        return new WhisperFactory(new WhisperProcessorModelBufferLoader(buffer, useGpu), delayInitialization);
    }

    /// <summary>
    /// Creates a builder that can be used to initialize the whisper processor.
    /// </summary>
    /// <returns>An instance to a new builder.</returns>
    /// <exception cref="ObjectDisposedException">Throws if the factory was already disposed.</exception>
    /// <exception cref="WhisperModelLoadException">Throws if the model couldn't be loaded.</exception>
    public WhisperProcessorBuilder CreateBuilder()
    {
        if (wasDisposed)
        {
            throw new ObjectDisposedException(nameof(WhisperFactory));
        }

        var context = contextLazy.Value;
        if (context == IntPtr.Zero)
        {
            throw new WhisperModelLoadException("Failed to load the whisper model.");
        }

        return new WhisperProcessorBuilder(contextLazy.Value);
    }

    public void Dispose()
    {
        if (wasDisposed)
        {
            return;
        }
        if (contextLazy.IsValueCreated && contextLazy.Value != IntPtr.Zero)
        {
            NativeMethods.whisper_free(contextLazy.Value);
        }
        loader.Dispose();
        wasDisposed = true;
    }
}
