// Licensed under the MIT license: https://opensource.org/licenses/MIT
using Whisper.net.Logger;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssemblyFixture;

namespace Whisper.net.Tests;

public sealed class FactoryTests : IAssemblyFixture<TinyModelFixture>, IDisposable
{
    private readonly TinyModelFixture model;
    private readonly ITestOutputHelper output;

    private readonly List<IDisposable> loggers = [];

    public FactoryTests(TinyModelFixture model, ITestOutputHelper output)
    {
        loggers.Add(LogProvider.AddConsoleLogging(minLevel: WhisperLogLevel.Debug));
        loggers.Add(LogProvider.AddLogger(OnLog));
        this.model = model;
        this.output = output;
    }

    public void Dispose()
    {
        foreach (var logger in loggers)
        {
            logger.Dispose();
        }
    }

    [Fact]
    public void GetSupportedLanguages_ShouldReturnAll()
    {
        var languages = WhisperFactory.GetSupportedLanguages().ToList();

        Assert.Equal(99, languages.Count);
    }

    [Fact]
    public void CreateBuilder_WithNoModel_ShouldThrow()
    {
        var loadingMethod = () =>
        {
            WhisperFactory.FromPath("non-existent-file.bin")
                .CreateBuilder();
        };

        Assert.Throws<WhisperModelLoadException>(() => loadingMethod());
    }

    [Fact]
    public void CreateBuilder_WithCorruptedModel_ShouldThrow()
    {
        var loadingMethod = () =>
        {
            WhisperFactory.FromPath("kennedy.wav")
                .CreateBuilder();
        };

        Assert.Throws<WhisperModelLoadException>(loadingMethod);
    }

    [Fact]
    public void CreateBuilder_WithFileModel_ShouldReturnBuilder()
    {
        using var factory = WhisperFactory.FromPath(model.ModelFile);
        var builder = factory.CreateBuilder();
        Assert.NotNull(builder);
    }

    [Fact]
    public void CreateBuilder_WithMemoryModel_ShouldReturnBuilder()
    {
        var memoryBuffer = File.ReadAllBytes(model.ModelFile);
        using var factory = WhisperFactory.FromBuffer(memoryBuffer);
        var builder = factory.CreateBuilder();
        Assert.NotNull(builder);
    }

    [Fact]
    public void CreateBuilder_WithStreamModelReturningPartialReads_ShouldReturnBuilder()
    {
        using var modelStream = File.OpenRead(model.ModelFile);
        using var partialReadStream = new PartialReadStream(modelStream, maxBytesPerRead: 3);
        using var factory = WhisperFactory.FromStream(partialReadStream);
        var builder = factory.CreateBuilder();
        Assert.NotNull(builder);
    }

    [Fact]
    public void CreateBuilder_WithNonSeekableStreamModelReturningPartialReads_ShouldReturnBuilder()
    {
        using var modelStream = File.OpenRead(model.ModelFile);
        using var partialReadStream = new PartialReadStream(modelStream, maxBytesPerRead: 3, canSeek: false);
        using var factory = WhisperFactory.FromStream(partialReadStream);
        var builder = factory.CreateBuilder();
        Assert.NotNull(builder);
    }

    [Fact]
    public void CreateBuilder_WithThrowingModelLoader_ShouldThrowModelLoadExceptionWithInnerException()
    {
        using var factory = WhisperFactory.FromModelLoader(new ThrowingModelLoader(),
            new WhisperFactoryOptions { DelayInitialization = true });

        var exception = Assert.Throws<WhisperModelLoadException>(() => factory.CreateBuilder());
        Assert.IsType<InvalidOperationException>(exception.InnerException);
    }

    [Fact]
    public void CreateBuilder_WithModelLoader_ShouldCloseLoaderAfterNativeLoad()
    {
        var loader = new TrackingModelLoader(File.ReadAllBytes(model.ModelFile));
        using var factory = WhisperFactory.FromModelLoader(loader);

        _ = factory.CreateBuilder();

        Assert.Equal(1, loader.CloseCount);
    }

    [Fact]
    public void CreateBuilder_WithDisposedFactory_ShouldThrow()
    {
        var factory = WhisperFactory.FromPath(model.ModelFile);
        factory.Dispose();

        var loadingMethod = () =>
        {
            factory.CreateBuilder();
        };

        Assert.Throws<ObjectDisposedException>(loadingMethod);
    }

    private void OnLog(WhisperLogLevel logLevel, string? message)
    {
        try
        {
            output.WriteLine($"[Whisper.net] {logLevel}: {message}");
        }
        catch
        {
            // Might be that some tests were not disposed yet and will still receive log events resulting in errors
        }
    }

    private sealed class ThrowingModelLoader : IWhisperModelLoader
    {
        public bool IsEof => false;

        public void Reset()
        {
        }

        public int CopyTo(Span<byte> destination)
        {
            throw new InvalidOperationException("Loader failure.");
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }

    private sealed class TrackingModelLoader : IWhisperModelLoader
    {
        private readonly WhisperMemoryModelLoader inner;

        public TrackingModelLoader(ReadOnlyMemory<byte> memory)
        {
            inner = new WhisperMemoryModelLoader(memory);
        }

        public int CloseCount { get; private set; }

        public bool IsEof => inner.IsEof;

        public void Reset()
        {
            inner.Reset();
        }

        public int CopyTo(Span<byte> destination)
        {
            return inner.CopyTo(destination);
        }

        public void Close()
        {
            CloseCount++;
            inner.Close();
        }

        public void Dispose()
        {
            inner.Dispose();
        }
    }

    private sealed class PartialReadStream : Stream
    {
        private readonly Stream inner;
        private readonly int maxBytesPerRead;
        private readonly bool canSeek;

        public PartialReadStream(Stream inner, int maxBytesPerRead, bool canSeek = true)
        {
            this.inner = inner;
            this.maxBytesPerRead = maxBytesPerRead;
            this.canSeek = canSeek;
        }

        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => canSeek && inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => CanSeek ? inner.Length : throw new NotSupportedException();

        public override long Position
        {
            get => CanSeek ? inner.Position : throw new NotSupportedException();
            set
            {
                if (!CanSeek)
                {
                    throw new NotSupportedException();
                }

                inner.Position = value;
            }
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return inner.Read(buffer, offset, Math.Min(count, maxBytesPerRead));
        }

#if !NET48
        public override int Read(Span<byte> buffer)
        {
            return inner.Read(buffer[..Math.Min(buffer.Length, maxBytesPerRead)]);
        }
#endif

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
            {
                throw new NotSupportedException();
            }

            return inner.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
