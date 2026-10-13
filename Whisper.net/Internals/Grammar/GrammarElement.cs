// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace Whisper.net.Internals.Grammar;

// Values and layout must match whisper_gretype and whisper_grammar_element in whisper.h.
internal enum GrammarElementType
{
    End = 0,
    Alternative = 1,
    RuleReference = 2,
    Character = 3,
    CharacterNot = 4,
    CharacterRangeUpper = 5,
    CharacterAlternative = 6
}

[StructLayout(LayoutKind.Sequential)]
internal readonly struct GrammarElement(GrammarElementType type, uint value = 0)
{
    public readonly GrammarElementType Type = type;
    public readonly uint Value = value;
}

internal sealed class ParsedGrammar(GrammarElement[][] rules, int startRule)
{
    public GrammarElement[][] Rules { get; } = rules;
    public int StartRule { get; } = startRule;
}
