// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Whisper.net.Internals.Native;
using Whisper.net.Native;

namespace Whisper.net.Internals.ModelLoader;

internal sealed class ManagedWhisperProcessorModelLoader : IWhisperProcessorModelLoader
{
    private const byte trueByte = 1;
    private const byte falseByte = 0;

    private readonly IWhisperModelLoader modelLoader;
    private readonly WhisperFactoryOptions options;
    private readonly WhisperAheads aHeads;
    private readonly GCHandle? aheadsHandle;

    public ManagedWhisperProcessorModelLoader(IWhisperModelLoader modelLoader, WhisperFactoryOptions options)
    {
        this.modelLoader = modelLoader ?? throw new ArgumentNullException(nameof(modelLoader));
        this.options = options;
        aHeads = ModelLoaderUtils.GetWhisperAlignmentHeads(options.CustomAlignmentHeads, out aheadsHandle);
    }

    public IntPtr LoadNativeContext(INativeWhisper nativeWhisper)
    {
        using var nativeLoader = CreateNativeLoader();
        var context = nativeWhisper.Whisper_Init_With_Params_No_State(ref nativeLoader.Loader,
            ModelLoaderUtils.GetWhisperContextParams(options, aHeads));
        try
        {
            nativeLoader.ThrowIfError();
        }
        catch
        {
            if (context != IntPtr.Zero)
            {
                nativeWhisper.Whisper_Free(context);
            }

            throw;
        }

        return context;
    }

    public IntPtr LoadNativeVadContext(INativeWhisper nativeWhisper, WhisperVadContextParams parameters)
    {
        using var nativeLoader = CreateNativeLoader();
        var context = nativeWhisper.Whisper_Vad_Init_With_Params(ref nativeLoader.Loader, parameters);
        try
        {
            nativeLoader.ThrowIfError();
        }
        catch
        {
            if (context != IntPtr.Zero)
            {
                nativeWhisper.Whisper_Vad_Free(context);
            }

            throw;
        }

        return context;
    }

    public void Dispose()
    {
        modelLoader.Dispose();
        aheadsHandle?.Free();
    }

    private NativeModelLoaderScope CreateNativeLoader()
    {
        try
        {
            modelLoader.Reset();
        }
        catch (Exception ex)
        {
            throw new WhisperModelLoadException("Failed to reset the whisper model loader.", ex);
        }

        return new NativeModelLoaderScope(modelLoader);
    }

    private static CallbackContext GetContext(IntPtr context)
    {
        if (context == IntPtr.Zero)
        {
            throw new InvalidOperationException("The native model loader callback context was not provided.");
        }

        var handle = GCHandle.FromIntPtr(context);
        return handle.Target as CallbackContext
            ?? throw new InvalidOperationException("The native model loader callback context is invalid.");
    }

#if !NETSTANDARD
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    private static unsafe nuint ReadStatic(IntPtr context, IntPtr output, nuint readSize)
    {
        var callbackContext = GetContext(context);
        try
        {
            var destination = new Span<byte>((void*)output, checked((int)readSize));
            var totalCopied = 0;
            while (totalCopied < destination.Length)
            {
                if (callbackContext.ModelLoader.IsEof)
                {
                    if (totalCopied == 0)
                    {
                        return 0;
                    }

                    throw new EndOfStreamException("The model loader reached EOF before the requested native buffer was filled.");
                }

                var copied = callbackContext.ModelLoader.CopyTo(destination.Slice(totalCopied));
                if (copied < 0 || copied > destination.Length - totalCopied)
                {
                    throw new InvalidOperationException("The model loader returned an invalid byte count.");
                }

                if (copied == 0)
                {
                    if (callbackContext.ModelLoader.IsEof)
                    {
                        if (totalCopied == 0)
                        {
                            return 0;
                        }

                        throw new EndOfStreamException("The model loader reached EOF before the requested native buffer was filled.");
                    }

                    throw new InvalidOperationException("The model loader returned no data before reaching EOF.");
                }

                totalCopied += copied;
            }

            return (nuint)totalCopied;
        }
        catch (Exception ex)
        {
            callbackContext.SetError(ex);
            return 0;
        }
    }

#if !NETSTANDARD
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    private static byte EofStatic(IntPtr context)
    {
        var callbackContext = GetContext(context);
        try
        {
            return callbackContext.Error != null || callbackContext.ModelLoader.IsEof ? trueByte : falseByte;
        }
        catch (Exception ex)
        {
            callbackContext.SetError(ex);
            return trueByte;
        }
    }

#if !NETSTANDARD
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
    private static void CloseStatic(IntPtr context)
    {
        _ = context;
    }

    private sealed class CallbackContext
    {
        public CallbackContext(IWhisperModelLoader modelLoader)
        {
            ModelLoader = modelLoader;
        }

        public IWhisperModelLoader ModelLoader { get; }

        public Exception? Error { get; private set; }

        public void SetError(Exception exception)
        {
            Error ??= exception;
        }
    }

    private sealed class NativeModelLoaderScope : IDisposable
    {
        private readonly GCHandle contextHandle;
        private readonly CallbackContext callbackContext;
#if NETSTANDARD
        private readonly WhisperModelLoaderRead readCallback = ReadStatic;
        private readonly WhisperModelLoaderEof eofCallback = EofStatic;
        private readonly WhisperModelLoaderClose closeCallback = CloseStatic;
#endif

        public NativeModelLoaderScope(IWhisperModelLoader modelLoader)
        {
            callbackContext = new CallbackContext(modelLoader);
            contextHandle = GCHandle.Alloc(callbackContext);

            Loader = new WhisperModelLoader
            {
                Context = GCHandle.ToIntPtr(contextHandle)
            };

#if NETSTANDARD
            Loader.Read = Marshal.GetFunctionPointerForDelegate(readCallback);
            Loader.Eof = Marshal.GetFunctionPointerForDelegate(eofCallback);
            Loader.Close = Marshal.GetFunctionPointerForDelegate(closeCallback);
#else
            unsafe
            {
                delegate* unmanaged[Cdecl]<IntPtr, IntPtr, nuint, nuint> readCallbackPtr = &ReadStatic;
                delegate* unmanaged[Cdecl]<IntPtr, byte> eofCallbackPtr = &EofStatic;
                delegate* unmanaged[Cdecl]<IntPtr, void> closeCallbackPtr = &CloseStatic;
                Loader.Read = (IntPtr)readCallbackPtr;
                Loader.Eof = (IntPtr)eofCallbackPtr;
                Loader.Close = (IntPtr)closeCallbackPtr;
            }
#endif
        }

        public WhisperModelLoader Loader;

        public void ThrowIfError()
        {
            if (callbackContext.Error != null)
            {
                throw new WhisperModelLoadException("Failed to load the whisper model from the managed model loader.", callbackContext.Error);
            }
        }

        public void Dispose()
        {
            if (contextHandle.IsAllocated)
            {
                contextHandle.Free();
            }

#if NETSTANDARD
            GC.KeepAlive(readCallback);
            GC.KeepAlive(eofCallback);
            GC.KeepAlive(closeCallback);
#endif
        }
    }
}
