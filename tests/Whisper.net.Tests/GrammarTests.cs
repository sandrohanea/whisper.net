// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Reflection;
using System.Runtime.InteropServices;
using Whisper.net.Internals.Grammar;
using Whisper.net.Native;
using Xunit;

namespace Whisper.net.Tests;

public class GrammarTests
{
    [Fact]
    public void Parse_LiteralsAndUnicode_UsesCodePoints()
    {
        var parsed = GbnfParser.Parse("root ::= \"ă😀\\u0219\\U0001F600\\x41\\t\\n\\r\\\\\\\"\\[\\]\"", "root");
        Assert.Equal(0, parsed.StartRule);
        AssertElements(Assert.Single(parsed.Rules),
            (3, 0x103), (3, 0x1F600), (3, 0x219), (3, 0x1F600), (3, 65),
            (3, 9), (3, 10), (3, 13), (3, 92), (3, 34), (3, 91), (3, 93), (0, 0));
    }

    [Fact]
    public void Parse_CharacterClasses_UsesNativeRangeAndAlternativeElements()
    {
        var parsed = GbnfParser.Parse("root ::= [a-zA-Zâ-ț] [^0-9!?]", "root");
        AssertElements(Assert.Single(parsed.Rules),
            (3, 97), (5, 122), (6, 65), (5, 90), (6, 0xE2), (5, 0x21B),
            (4, 48), (5, 57), (6, 33), (6, 63), (0, 0));
    }

    [Fact]
    public void Parse_ForwardReferencesCommentsAndCustomRoot()
    {
        var parsed = GbnfParser.Parse("# comment\r\nother ::= word # comment\r\nstart ::= other |\n \"stop\"\nword ::= \"go\"", "start");
        Assert.Equal(2, parsed.StartRule);
        AssertElements(parsed.Rules[0], (2, 1), (0, 0));
        AssertElements(parsed.Rules[2], (2, 0), (1, 0), (3, 115), (3, 116), (3, 111), (3, 112), (0, 0));
    }

    [Theory]
    [InlineData('*')]
    [InlineData('+')]
    [InlineData('?')]
    public void Parse_Repetition_CompilesEntirePrecedingLiteral(char operation)
    {
        var parsed = GbnfParser.Parse($"root ::= \"ab\"{operation}", "root");
        AssertElements(parsed.Rules[0], (2, 1), (0, 0));
        var expected = new List<(int, uint)> { (3, 97), (3, 98) };
        if (operation != '?')
        {
            expected.Add((2, 1));
        }

        expected.Add((1, 0));
        if (operation == '+')
        {
            expected.AddRange([(3, 97), (3, 98)]);
        }

        expected.Add((0, 0));
        AssertElements(parsed.Rules[1], [.. expected]);
    }

    [Fact]
    public void Parse_GroupsAndRightRecursion_AreSupported()
    {
        var parsed = GbnfParser.Parse("root ::= (\n\"a\" | \"b\"\n)+ tail?\ntail ::= \"c\" tail | \"\"", "root");
        Assert.Equal(5, parsed.Rules.Length);
        Assert.All(parsed.Rules, rule => Assert.Equal(GrammarElementType.End, rule[rule.Length - 1].Type));
    }

    [Theory]
    [InlineData("")]
    [InlineData("root := \"a\"")]
    [InlineData("root ::= \"unterminated")]
    [InlineData("root ::= [abc")]
    [InlineData("root ::= []")]
    [InlineData("root ::= [^]")]
    [InlineData("root ::= [z-a]")]
    [InlineData("root ::= \"\\uD800\"")]
    [InlineData("root ::= \"\\U00110000\"")]
    [InlineData("root ::= \"\\x00\"")]
    [InlineData("root ::= \"\\q\"")]
    [InlineData("root ::= \"\\u12\"")]
    [InlineData("root ::= missing")]
    [InlineData("root ::= \"a\"\nroot ::= \"b\"")]
    [InlineData("root ::= (\"a\"")]
    [InlineData("root ::= *")]
    [InlineData("root ::= \"a\"**")]
    [InlineData("root ::= \"a\"{2}")]
    [InlineData("root ::= root \"a\" | \"b\"")]
    [InlineData("root ::= other\nother ::= root")]
    [InlineData("root ::= empty root | \"a\"\nempty ::= \"\"")]
    [InlineData("root ::= (\"\")*")]
    [InlineData("root ::= (\"a\"?)+")]
    public void Parse_InvalidGrammar_ThrowsWithLocation(string gbnf)
    {
        var error = Assert.Throws<FormatException>(() => GbnfParser.Parse(gbnf, "root"));
        Assert.Contains("line ", error.Message);
        Assert.Contains("column ", error.Message);
    }

    [Fact]
    public void Parse_InvalidSurrogate_Throws()
    {
        Assert.Throws<FormatException>(() => GbnfParser.Parse("root ::= \"\uD800\"", "root"));
        Assert.Throws<FormatException>(() => GbnfParser.Parse("root ::= \"\uDC00\"", "root"));
    }

    [Fact]
    public void Parse_MissingStartRule_Throws()
    {
        Assert.Throws<FormatException>(() => GbnfParser.Parse("other ::= \"a\"", "root"));
    }

    [Fact]
    public void Parse_ReportsCorrectLineAndColumn()
    {
        var error = Assert.Throws<FormatException>(() => GbnfParser.Parse("root ::= \"a\"\r\nbad := \"b\"", "root"));
        Assert.Contains("line 2, column 6", error.Message);
    }

    [Fact]
    public void Parse_ExcessiveNesting_Throws()
    {
        Assert.Throws<FormatException>(() => GbnfParser.Parse("root ::= " + new string('(', 129) + "\"a\"" + new string(')', 129), "root"));
    }

    [Fact]
    public void Builder_ValidatesArguments()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0);
        var builder = new WhisperProcessorBuilder(IntPtr.Zero, native);
        Assert.Throws<ArgumentNullException>(() => builder.WithGrammar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.WithGrammar("root ::= \"a\"", null!));
        foreach (var penalty in new[] { -1f, float.NaN, float.PositiveInfinity, float.NegativeInfinity })
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithGrammar("root ::= \"a\"", penalty: penalty));
        }
    }

    [Fact]
    public void NativeGrammar_LayoutAndDisposal()
    {
        Assert.Equal(8, Marshal.SizeOf<GrammarElement>());
        var parsed = GbnfParser.Parse("root ::= word\nword ::= \"ș\"", "word");
        var grammar = new NativeGrammar(parsed);
        try
        {
            var parameters = new WhisperFullParams();
            grammar.Apply(ref parameters, parsed, 75f);
            Assert.Equal((UIntPtr)2u, parameters.NGrammarRules);
            Assert.Equal((UIntPtr)1u, parameters.StartGrammarRule);
            Assert.Equal(75f, parameters.GrammarPenalty);
            for (var i = 0; i < parsed.Rules.Length; i++)
            {
                var pointer = Marshal.ReadIntPtr(parameters.WhisperGrammarElement, i * IntPtr.Size);
                for (var j = 0; j < parsed.Rules[i].Length; j++)
                {
                    var element = Marshal.PtrToStructure<GrammarElement>(IntPtr.Add(pointer, j * 8));
                    Assert.Equal(parsed.Rules[i][j].Type, element.Type);
                    Assert.Equal(parsed.Rules[i][j].Value, element.Value);
                }
            }
        }
        finally
        {
            grammar.Dispose();
        }
        grammar.Dispose();
        Assert.True(grammar.IsClosed);
    }

    [Fact]
    public void Builder_PassesGrammarToNativeAcrossRepeatedRunsAndOwnsMemoryPerProcessor()
    {
        var calls = 0;
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            calls++;
            Assert.Equal((UIntPtr)2u, parameters.NGrammarRules);
            Assert.Equal((UIntPtr)1u, parameters.StartGrammarRule);
            Assert.Equal(50f, parameters.GrammarPenalty);
            Assert.Equal((byte)0, parameters.Translate);
            var rule = Marshal.ReadIntPtr(parameters.WhisperGrammarElement, IntPtr.Size);
            Assert.Equal((uint)'b', Marshal.PtrToStructure<GrammarElement>(rule).Value);
            return 0;
        });
        var builder = new WhisperProcessorBuilder(IntPtr.Zero, native);
        Assert.Same(builder, builder.WithGrammar("root ::= \"a\"\ncommand ::= \"b\"", "command", 50f));
        using var first = builder.Build();
        using var second = builder.Build();
        var field = typeof(WhisperProcessor).GetField("grammar", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var firstMemory = Assert.IsType<NativeGrammar>(field.GetValue(first));
        var secondMemory = Assert.IsType<NativeGrammar>(field.GetValue(second));
        Assert.NotEqual(firstMemory.DangerousGetHandle(), secondMemory.DangerousGetHandle());
        first.Process(new float[160]);
        first.Process(new float[160]);
        first.Dispose();
        Assert.True(firstMemory.IsClosed);
        Assert.False(secondMemory.IsClosed);
        second.Process(new float[160]);
        Assert.Equal(3, calls);
    }

    [Fact]
    public void Builder_DefaultPenaltyAndReplacement()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            Assert.Equal(100f, parameters.GrammarPenalty);
            Assert.Equal((UIntPtr)1u, parameters.NGrammarRules);
            var rule = Marshal.ReadIntPtr(parameters.WhisperGrammarElement);
            Assert.Equal((uint)'b', Marshal.PtrToStructure<GrammarElement>(rule).Value);
            return 0;
        });
        using var processor = new WhisperProcessorBuilder(IntPtr.Zero, native)
            .WithGrammar("root ::= \"a\"", penalty: 5f)
            .WithGrammar("root ::= \"b\"")
            .Build();
        processor.Process(new float[160]);
    }

    [Fact]
    public void Processor_WithoutGrammar_PreservesNativeDefaults()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            Assert.Equal(IntPtr.Zero, parameters.WhisperGrammarElement);
            Assert.Equal(UIntPtr.Zero, parameters.NGrammarRules);
            return 0;
        });
        using var processor = new WhisperProcessorBuilder(IntPtr.Zero, native).Build();
        processor.Process(new float[160]);
    }

    [Fact]
    public void Builder_Parakeet_RejectsGrammar()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0);
        var builder = new WhisperProcessorBuilder(IntPtr.Zero, native, WhisperModelFamily.Parakeet)
            .WithGrammar("root ::= \"a\"");
        var error = Assert.Throws<NotSupportedException>(() => builder.Build());
        Assert.Contains("Grammar", error.Message);
    }

    [Fact]
    public async Task Processor_DisposeAsync_ReleasesGrammarAfterNativeFailure()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(7);
        var processor = new WhisperProcessorBuilder(IntPtr.Zero, native)
            .WithGrammar("root ::= \"a\"")
            .Build();
        var field = typeof(WhisperProcessor).GetField("grammar", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var memory = Assert.IsType<NativeGrammar>(field.GetValue(processor));
        try
        {
            Assert.Throws<WhisperProcessingException>(() => processor.Process(new float[160]));
            Assert.False(memory.IsClosed);
        }
        finally
        {
            await processor.DisposeAsync();
        }
        Assert.True(memory.IsClosed);
    }

    [Fact]
    public void Builder_InvalidReplacement_PreservesPreviousGrammar()
    {
        using var native = new ProcessingFailureTests.FakeNativeWhisper(0, (_, _, parameters, _, _) =>
        {
            Assert.Equal(0f, parameters.GrammarPenalty);
            var rule = Marshal.ReadIntPtr(parameters.WhisperGrammarElement);
            Assert.Equal((uint)'a', Marshal.PtrToStructure<GrammarElement>(rule).Value);
            return 0;
        });
        var builder = new WhisperProcessorBuilder(IntPtr.Zero, native)
            .WithGrammar("root ::= \"a\"", penalty: 0f);
        Assert.Throws<FormatException>(() => builder.WithGrammar("root ::= missing"));
        using var processor = builder.Build();
        processor.Process(new float[160]);
    }

    private static void AssertElements(GrammarElement[] actual, params (int Type, uint Value)[] expected)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal((GrammarElementType)expected[i].Type, actual[i].Type);
            Assert.Equal(expected[i].Value, actual[i].Value);
        }
    }
}
