// Licensed under the MIT license: https://opensource.org/licenses/MIT
using FluentAssertions;
using Whisper.net.Logger;
using Xunit;
using Xunit.Abstractions;

namespace Whisper.net.Tests;

public sealed class FactoryTests : IClassFixture<TinyModelFixture>, IDisposable
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

        languages.Should().HaveCount(99);
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
}
