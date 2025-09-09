// Licensed under the MIT license: https://opensource.org/licenses/MIT
using Whisper.net.Logger;
using Xunit;

namespace Whisper.net.Tests;

public sealed class FactoryTests : IDisposable
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
        Action loadingMethod = () =>
        {
            WhisperFactory.FromPath("non-existent-file.bin")
                .CreateBuilder();
        };

        Assert.Throws<WhisperModelLoadException>(() => loadingMethod());
    }

    [Fact]
    public void CreateBuilder_WithCorruptedModel_ShouldThrow()
    {
        Action loadingMethod = () =>
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
    public void CreateBuilder_WithDisposedFactory_ShouldThrow()
    {
        var factory = WhisperFactory.FromPath(model.ModelFile);
        factory.Dispose();

        Action loadingMethod = () =>
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
}
