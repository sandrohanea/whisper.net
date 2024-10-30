// Licensed under the MIT license: https://opensource.org/licenses/MIT
using FluentAssertions;
using Whisper.net.Logger;
using Xunit;

namespace Whisper.net.Tests;

public class FactoryTests : IClassFixture<TinyModelFixture>
{
    private readonly TinyModelFixture model;

    public FactoryTests(TinyModelFixture model)
    {
        LogProvider.AddConsoleLogging(minLevel: WhisperLogLevel.Debug);
        this.model = model;
    }

    [Fact]
    public void CreateBuilder_WithNoModel_ShouldThrow()
    {
        Action loadingMethod = () =>
        {
            WhisperFactory.FromPath("non-existent-file.bin")
                .CreateBuilder();
        };

        loadingMethod.Should().Throw<WhisperModelLoadException>();
    }

    [Fact]
    public void CreateBuilder_WithCorruptedModel_ShouldThrow()
    {
        Action loadingMethod = () =>
        {
            WhisperFactory.FromPath("kennedy.wav")
                .CreateBuilder();
        };

        loadingMethod.Should().Throw<WhisperModelLoadException>();
    }

    [Fact]
    public void CreateBuilder_WithFileModel_ShouldReturnBuilder()
    {
        using var factory = WhisperFactory.FromPath(model.ModelFile);
        var builder = factory.CreateBuilder();
        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_WithBufferedModel_ShouldReturnBuilder()
    {
        var buffer = File.ReadAllBytes(model.ModelFile);
        using var factory = WhisperFactory.FromBuffer(buffer);
        var builder = factory.CreateBuilder();
        builder.Should().NotBeNull();
    }

    [Fact]
    public void CreateBuilder_WithDisposedFactory_ShouldThrow()
    {
        var factory = WhisperFactory.FromPath(model.ModelFile);
        factory.Dispose();

        Action loadingMethod = () =>
        {
            factory.CreateBuilder();
        };

        loadingMethod.Should().Throw<ObjectDisposedException>();
    }
}
