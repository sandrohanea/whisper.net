// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Xunit;
using Xunit.Extensions.AssemblyFixture;

namespace Whisper.net.Tests;

public class VadFunctionalTests(SileroVadModelFixture model) : IAssemblyFixture<SileroVadModelFixture>
{
    [Fact]
    public async Task DetectSpeechAsync_WhenUsingSileroVadModel_DetectsExpectedSegments()
    {
        using var factory = WhisperVadFactory.FromPath(model.ModelFile);
        await using var processor = factory.CreateBuilder()
            .WithThreshold(0.5f)
            .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var segments = await processor.DetectSpeechAsync(fileReader);

        AssertSegmentsDetected(segments);
    }

    [Fact]
    public async Task DetectSpeech_WhenUsingSileroVadModel_DetectsExpectedSegments()
    {
        using var factory = WhisperVadFactory.FromPath(model.ModelFile);
        using var processor = factory.CreateBuilder()
            .WithThreshold(0.5f)
            .Build();

        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var segments = processor.DetectSpeech(fileReader);

        AssertSegmentsDetected(segments);
    }

    [Fact]
    public void Build_WhenUsingBufferModel_ShouldReturnProcessorsForMultipleBuilds()
    {
        var modelBuffer = File.ReadAllBytes(model.ModelFile);
        using var factory = WhisperVadFactory.FromBuffer(modelBuffer);

        using var firstProcessor = factory.CreateBuilder().Build();
        using var secondProcessor = factory.CreateBuilder().Build();

        Assert.NotNull(firstProcessor);
        Assert.NotNull(secondProcessor);
    }

    [Fact]
    public void Build_WhenUsingStreamModelReturningPartialReads_ShouldReturnProcessor()
    {
        using var modelStream = File.OpenRead(model.ModelFile);
        using var partialReadStream = new PartialReadStream(modelStream, maxBytesPerRead: 4096);
        using var factory = WhisperVadFactory.FromStream(partialReadStream);

        using var processor = factory.CreateBuilder().Build();

        Assert.NotNull(processor);
    }

    [Fact]
    public void Build_WhenUsingModelLoader_ShouldCloseLoaderAfterNativeLoad()
    {
        var loader = new TrackingModelLoader(File.ReadAllBytes(model.ModelFile));
        using var factory = WhisperVadFactory.FromModelLoader(loader);

        using var processor = factory.CreateBuilder().Build();

        Assert.NotNull(processor);
        Assert.Equal(1, loader.CloseCount);
    }

    private static void AssertSegmentsDetected(IReadOnlyList<VadSegmentData> segments)
    {
        Assert.Equal(7, segments.Count);

        Assert.InRange(segments[0].Start, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(300));
        Assert.InRange(segments[0].End, TimeSpan.FromMilliseconds(2700), TimeSpan.FromMilliseconds(2800));

        Assert.InRange(segments[6].Start, TimeSpan.FromMilliseconds(17500), TimeSpan.FromMilliseconds(17600));
        Assert.InRange(segments[6].End, TimeSpan.FromMilliseconds(20900), TimeSpan.FromMilliseconds(21100));
    }

    private sealed class PartialReadStream : Stream
    {
        private readonly Stream inner;
        private readonly int maxBytesPerRead;

        public PartialReadStream(Stream inner, int maxBytesPerRead)
        {
            this.inner = inner;
            this.maxBytesPerRead = maxBytesPerRead;
        }

        public override bool CanRead => inner.CanRead;
        public override bool CanSeek => inner.CanSeek;
        public override bool CanWrite => false;
        public override long Length => inner.Length;

        public override long Position
        {
            get => inner.Position;
            set => inner.Position = value;
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
}
