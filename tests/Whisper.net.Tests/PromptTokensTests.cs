// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.Native;
using Xunit;

namespace Whisper.net.Tests;

public class PromptTokensTests
{
    private static int[] ReadPromptTokens(WhisperFullParams parameters)
    {
        var tokens = new int[parameters.PromptNTokens];
        Marshal.Copy(parameters.PromptTokens, tokens, 0, tokens.Length);
        return tokens;
    }

    [Fact]
    public void Processor_WithPromptTokens_PassesTokensAndClearsTextPrompt()
    {
        var calls = 0;
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            calls++;
            Assert.Equal(3, parameters.PromptNTokens);
            Assert.Equal([40, 669, 16517], ReadPromptTokens(parameters));
            Assert.Equal(IntPtr.Zero, parameters.InitialPrompt);
            return 0;
        });
        using var processor = new WhisperProcessorBuilder(IntPtr.Zero, native)
            .WithPrompt("I am Kennedy")
            .WithPromptTokens([40, 669, 16517])
            .Build();
        processor.Process(new float[160]);
        processor.Process(new float[160]);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void Builder_WithPromptTokens_CopiesTheArray()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            Assert.Equal([1, 2, 3], ReadPromptTokens(parameters));
            return 0;
        });
        var tokens = new[] { 1, 2, 3 };
        var builder = new WhisperProcessorBuilder(IntPtr.Zero, native).WithPromptTokens(tokens);
        tokens[0] = 99;
        using var processor = builder.Build();
        processor.Process(new float[160]);
    }

    [Fact]
    public void Builder_WithEmptyPromptTokens_ClearsTheSettingAndKeepsTextPrompt()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            Assert.Equal(IntPtr.Zero, parameters.PromptTokens);
            Assert.Equal(0, parameters.PromptNTokens);
            Assert.NotEqual(IntPtr.Zero, parameters.InitialPrompt);
            return 0;
        });
        using var processor = new WhisperProcessorBuilder(IntPtr.Zero, native)
            .WithPrompt("I am Kennedy")
            .WithPromptTokens([40, 669, 16517])
            .WithPromptTokens([])
            .Build();
        processor.Process(new float[160]);
    }

    [Fact]
    public void Processor_WithoutPromptTokens_PreservesNativeDefaults()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            Assert.Equal(IntPtr.Zero, parameters.PromptTokens);
            Assert.Equal(0, parameters.PromptNTokens);
            return 0;
        });
        using var processor = new WhisperProcessorBuilder(IntPtr.Zero, native).Build();
        processor.Process(new float[160]);
    }

    [Fact]
    public void Builder_WithNullPromptTokens_Throws()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0);
        var builder = new WhisperProcessorBuilder(IntPtr.Zero, native);
        Assert.Throws<ArgumentNullException>(() => builder.WithPromptTokens(null!));
    }

    [Fact]
    public void Builder_Parakeet_RejectsPromptTokens()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0);
        var builder = new WhisperProcessorBuilder(IntPtr.Zero, native, WhisperModelFamily.Parakeet)
            .WithPromptTokens([40, 669, 16517]);
        var error = Assert.Throws<NotSupportedException>(() => builder.Build());
        Assert.Contains("Initial prompt tokens", error.Message);
    }
}
