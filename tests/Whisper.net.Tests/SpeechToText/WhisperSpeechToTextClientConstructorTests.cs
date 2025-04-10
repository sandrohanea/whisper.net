// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Reflection;
using Microsoft.Extensions.AI;
using Xunit;

#pragma warning disable MEAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace Whisper.net.Tests.SpeechToText;

public class WhisperSpeechToTextClientConstructorTests : IClassFixture<TinyModelFixture>
{
    private readonly TinyModelFixture _model;

    public WhisperSpeechToTextClientConstructorTests(TinyModelFixture model)
    {
        _model = model;
    }

    [Fact]
    public void Constructor_WithModelFileName_ShouldNotCreateFactoryImmediately()
    {
        // Arrange & Act
        using var client = new WhisperSpeechToTextClient(_model.ModelFile);

        // Assert
        var factoryField = GetFactoryField(client);
        Assert.Null(factoryField);
    }

    [Fact]
    public void Constructor_WithFactoryBuilder_ShouldNotCreateFactoryImmediately()
    {
        // Arrange
        var factoryBuilderCalled = false;
        WhisperFactory FactoryBuilder()
        {
            factoryBuilderCalled = true;
            return WhisperFactory.FromPath(_model.ModelFile);
        }

        // Act
        using var client = new WhisperSpeechToTextClient(FactoryBuilder);

        // Assert
        var factoryField = GetFactoryField(client);
        Assert.Null(factoryField);
        Assert.False(factoryBuilderCalled, "Factory builder should not be called until needed");
    }

    [Fact]
    public void Constructor_WithNullFactoryBuilder_ShouldThrowArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WhisperSpeechToTextClient((Func<WhisperFactory>)null!));
    }

    [Fact]
    public async Task GetTextAsync_ShouldCreateFactoryLazily()
    {
        // Arrange
        var factoryBuilderCalled = false;
        WhisperFactory FactoryBuilder()
        {
            factoryBuilderCalled = true;
            return WhisperFactory.FromPath(_model.ModelFile);
        }

        using var client = new WhisperSpeechToTextClient(FactoryBuilder);

        // Assert - Before
        Assert.Null(GetFactoryField(client));
        Assert.False(factoryBuilderCalled, "Factory builder should not be called until needed");

        // Act
        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var options = new SpeechToTextOptions().WithLanguage("en");
        await client.GetTextAsync(fileReader, options);

        // Assert - After
        Assert.NotNull(GetFactoryField(client));
        Assert.True(factoryBuilderCalled, "Factory builder should be called when GetTextAsync is invoked");
    }

    [Fact]
    public async Task GetStreamingTextAsync_ShouldCreateFactoryLazily()
    {
        // Arrange
        var factoryBuilderCalled = false;
        WhisperFactory FactoryBuilder()
        {
            factoryBuilderCalled = true;
            return WhisperFactory.FromPath(_model.ModelFile);
        }

        using var client = new WhisperSpeechToTextClient(FactoryBuilder);

        // Assert - Before
        Assert.Null(GetFactoryField(client));
        Assert.False(factoryBuilderCalled, "Factory builder should not be called until needed");

        // Act
        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var options = new SpeechToTextOptions().WithLanguage("en");
        await foreach (var _ in client.GetStreamingTextAsync(fileReader, options))
        {
            // Just consume the stream
        }

        // Assert - After
        Assert.NotNull(GetFactoryField(client));
        Assert.True(factoryBuilderCalled, "Factory builder should be called when GetStreamingTextAsync is invoked");
    }

    [Fact]
    public async Task MultipleRequests_ShouldReuseFactory()
    {
        // Arrange
        var factoryBuilderCallCount = 0;
        WhisperFactory FactoryBuilder()
        {
            factoryBuilderCallCount++;
            return WhisperFactory.FromPath(_model.ModelFile);
        }

        using var client = new WhisperSpeechToTextClient(FactoryBuilder);
        var options = new SpeechToTextOptions().WithLanguage("en");

        // Act - First request
        using (var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav"))
        {
            await client.GetTextAsync(fileReader, options);
        }

        var factoryAfterFirstRequest = GetFactoryField(client);
        Assert.NotNull(factoryAfterFirstRequest);
        Assert.Equal(1, factoryBuilderCallCount);

        // Act - Second request
        using (var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav"))
        {
            await client.GetTextAsync(fileReader, options);
        }

        // Assert
        var factoryAfterSecondRequest = GetFactoryField(client);
        Assert.NotNull(factoryAfterSecondRequest);
        Assert.Equal(1, factoryBuilderCallCount); // Factory builder should only be called once
        Assert.Same(factoryAfterFirstRequest, factoryAfterSecondRequest); // he same factory instance should be reused
    }

    [Fact]
    public async Task Dispose_ShouldDisposeFactory()
    {
        // Arrange
        using var client = new WhisperSpeechToTextClient(_model.ModelFile);
        var options = new SpeechToTextOptions().WithLanguage("en");

        // Act - Create factory by using the client
        using (var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav"))
        {
            await client.GetTextAsync(fileReader, options);
        }

        var factory = GetFactoryField(client);
        Assert.NotNull(factory);

        // Act - Dispose the client
        client.Dispose();

        // Assert
        // After disposal, the factory field should be null
        Assert.Null(GetFactoryField(client));
    }

    [Fact]
    public async Task AfterDispose_GetTextAsync_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var client = new WhisperSpeechToTextClient(_model.ModelFile);
        var options = new SpeechToTextOptions().WithLanguage("en");

        // Initialize the client by using it once
        using (var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav"))
        {
            await client.GetTextAsync(fileReader, options);
        }

        // Act - Dispose the client
        client.Dispose();

        // Assert - Attempting to use the client after disposal should throw ObjectDisposedException
        using var fileReader2 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetTextAsync(fileReader2, options));
    }

    [Fact]
    public async Task AfterDispose_GetStreamingTextAsync_ShouldThrowObjectDisposedException()
    {
        // Arrange
        var client = new WhisperSpeechToTextClient(_model.ModelFile);
        var options = new SpeechToTextOptions().WithLanguage("en");

        // Initialize the client by using it once
        using (var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav"))
        {
            await foreach (var _ in client.GetStreamingTextAsync(fileReader, options))
            {
                // Just consume the stream
                break; // We only need to process one item to initialize the client
            }
        }

        // Act - Dispose the client
        client.Dispose();

        // Assert - Attempting to use the client after disposal should throw ObjectDisposedException
        using var fileReader2 = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            await foreach (var _ in client.GetStreamingTextAsync(fileReader2, options))
            {
                // This should throw before we get here
            }
        });
    }

    [Fact]
    public async Task FactoryBuilder_ReturningNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        using var client = new WhisperSpeechToTextClient(() => null!);
        var options = new SpeechToTextOptions().WithLanguage("en");

        // Act & Assert
        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetTextAsync(fileReader, options));
    }

    private static WhisperFactory? GetFactoryField(WhisperSpeechToTextClient client)
    {
        var fieldInfo = typeof(WhisperSpeechToTextClient).GetField("_factory", BindingFlags.NonPublic | BindingFlags.Instance);
        return fieldInfo?.GetValue(client) as WhisperFactory;
    }
}
