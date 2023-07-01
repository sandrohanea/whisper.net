// Licensed under the MIT license: https://opensource.org/licenses/MIT
using FluentAssertions;
using NUnit.Framework;

namespace Whisper.net.Tests;

public class FactoryTests
{
    [Test]
    public void CreateBuilder_WithNoModel_ShouldThrow()
    {
        Action loadingMethod = () =>
        {
            WhisperFactory.FromPath("non-existent-file.bin")
                .CreateBuilder();
        };

        loadingMethod.Should().Throw<WhisperModelLoadException>();
    }

    [Test]
    public void CreateBuilder_WithCorruptedModel_ShouldThrow()
    {
        Action loadingMethod = () =>
        {
            WhisperFactory.FromPath("kennedy.wav")
                .CreateBuilder();
        };

        loadingMethod.Should().Throw<WhisperModelLoadException>();
    }

    [Test]
    public void CreateBuilder_WithFileModel_ShouldReturnBuilder()
    {
        using var factory = WhisperFactory.FromPath(TestModelProvider.GgmlModelTiny);
        var builder = factory.CreateBuilder();
        builder.Should().NotBeNull();
    }

    [Test]
    public void CreateBuilder_WithBufferedModel_ShouldReturnBuilder()
    {
        var buffer = File.ReadAllBytes(TestModelProvider.GgmlModelTiny);
        using var factory = WhisperFactory.FromBuffer(buffer);
        var builder = factory.CreateBuilder();
        builder.Should().NotBeNull();
    }

    [Test]
    public void CreateBuilder_WithDisposedFactory_ShouldThrow()
    {
        var factory = WhisperFactory.FromPath(TestModelProvider.GgmlModelTiny);
        factory.Dispose();

        Action loadingMethod = () =>
        {
            factory.CreateBuilder();
        };

        loadingMethod.Should().Throw<ObjectDisposedException>();
    }
}
