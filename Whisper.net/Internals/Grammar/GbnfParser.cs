// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Internals.Grammar;

/// <summary>
/// Compiles the GBNF syntax supported by whisper.cpp's examples/grammar-parser.cpp.
/// </summary>
internal sealed class GbnfParser
{
    private readonly string source;
    private readonly Dictionary<string, int> symbols = new(StringComparer.Ordinal);
    private readonly List<GrammarElement[]?> rules = [];
    private int position;
    private int nesting;

    private GbnfParser(string source)
    {
        this.source = source;
    }

    public static ParsedGrammar Parse(string source, string startRule)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (startRule is null)
        {
            throw new ArgumentNullException(nameof(startRule));
        }

        var parser = new GbnfParser(source);
        return parser.ParseRules(startRule);
    }

    private char Current => position < source.Length ? source[position] : '\0';

    private ParsedGrammar ParseRules(string startRule)
    {
        SkipSpace(true);
        while (position < source.Length)
        {
            var name = ReadName();
            var id = GetSymbol(name);
            if (rules[id] is not null)
            {
                throw Error($"Duplicate rule '{name}'");
            }

            SkipSpace(false);
            Expect(':');
            Expect(':');
            Expect('=');
            SkipSpace(true);
            rules[id] = ReadAlternatives(false);
            if (position < source.Length && Current != '\r' && Current != '\n')
            {
                throw Error("Expected a newline or end of grammar");
            }
            SkipSpace(true);
        }

        if (!symbols.TryGetValue(startRule, out var start))
        {
            throw Error($"Start rule '{startRule}' is not defined");
        }

        foreach (var symbol in symbols)
        {
            if (rules[symbol.Value] is null)
            {
                throw Error($"Undefined rule '{symbol.Key}'");
            }
        }
        var compiled = rules.Select(rule => rule!).ToArray();
        ValidateRecursion(compiled);
        return new ParsedGrammar(compiled, start);
    }

    private GrammarElement[] ReadAlternatives(bool nested)
    {
        var result = new List<GrammarElement>();
        while (true)
        {
            ReadSequence(result, nested);
            if (Current != '|')
            {
                break;
            }

            position++;
            result.Add(new(GrammarElementType.Alternative));
            SkipSpace(true);
        }
        result.Add(new(GrammarElementType.End));
        return [.. result];
    }

    private void ReadSequence(List<GrammarElement> result, bool nested)
    {
        while (true)
        {
            var atom = new List<GrammarElement>();
            switch (Current)
            {
                case '"':
                    position++;
                    while (Current != '"')
                    {
                        atom.Add(new(GrammarElementType.Character, ReadCharacter()));
                    }

                    position++;
                    break;
                case '[':
                    ReadCharacterClass(atom);
                    break;
                case '(':
                    if (++nesting > 128)
                    {
                        throw Error("Grammar groups are nested too deeply");
                    }

                    position++;
                    SkipSpace(true);
                    var group = NewRule();
                    rules[group] = ReadAlternatives(true);
                    Expect(')');
                    nesting--;
                    atom.Add(new(GrammarElementType.RuleReference, (uint)group));
                    break;
                default:
                    if (!IsNameCharacter(Current))
                    {
                        return;
                    }

                    atom.Add(new(GrammarElementType.RuleReference, (uint)GetSymbol(ReadName())));
                    break;
            }
            SkipSpace(nested);
            if (Current is '*' or '+' or '?')
            {
                var operation = Current;
                position++;
                var repetition = NewRule();
                var expanded = new List<GrammarElement>(atom);
                if (operation != '?')
                {
                    expanded.Add(new(GrammarElementType.RuleReference, (uint)repetition));
                }

                expanded.Add(new(GrammarElementType.Alternative));
                if (operation == '+')
                {
                    expanded.AddRange(atom);
                }

                expanded.Add(new(GrammarElementType.End));
                rules[repetition] = [.. expanded];
                atom = [new(GrammarElementType.RuleReference, (uint)repetition)];
                SkipSpace(nested);
            }
            result.AddRange(atom);
        }
    }

    private void ReadCharacterClass(List<GrammarElement> elements)
    {
        position++;
        var firstType = GrammarElementType.Character;
        if (Current == '^')
        {
            position++;
            firstType = GrammarElementType.CharacterNot;
        }
        while (Current != ']')
        {
            var lower = ReadCharacter();
            elements.Add(new(elements.Count == 0 ? firstType : GrammarElementType.CharacterAlternative, lower));
            if (Current == '-' && position + 1 < source.Length && source[position + 1] != ']')
            {
                position++;
                var upper = ReadCharacter();
                if (upper < lower)
                {
                    throw Error("Character range is reversed");
                }

                elements.Add(new(GrammarElementType.CharacterRangeUpper, upper));
            }
        }
        if (elements.Count == 0)
        {
            throw Error("Character classes cannot be empty");
        }

        position++;
    }

    private uint ReadCharacter()
    {
        if (position >= source.Length)
        {
            throw Error("Unexpected end of grammar");
        }

        uint value;
        var character = source[position++];
        if (character == '\\')
        {
            if (position >= source.Length)
            {
                throw Error("Incomplete escape sequence");
            }

            var escape = source[position++];
            switch (escape)
            {
                case 't':
                    value = '\t';
                    break;
                case 'r':
                    value = '\r';
                    break;
                case 'n':
                    value = '\n';
                    break;
                case '\\':
                case '"':
                case '[':
                case ']':
                    value = escape;
                    break;
                case 'x':
                case 'u':
                case 'U':
                    var digits = escape == 'x' ? 2 : escape == 'u' ? 4 : 8;
                    value = 0;
                    for (var i = 0; i < digits; i++)
                    {
                        var hex = Current;
                        var digit = hex is >= '0' and <= '9' ? hex - '0'
                            : hex is >= 'a' and <= 'f' ? hex - 'a' + 10
                            : hex is >= 'A' and <= 'F' ? hex - 'A' + 10 : -1;
                        if (digit < 0)
                        {
                            throw Error("Expected hexadecimal escape digits");
                        }

                        value = (value << 4) | (uint)digit;
                        position++;
                    }
                    break;
                default:
                    throw Error($"Unknown escape '\\{escape}'");
            }
        }
        else if (char.IsHighSurrogate(character))
        {
            if (!char.IsLowSurrogate(Current))
            {
                throw Error("Invalid Unicode surrogate pair");
            }

            value = (uint)char.ConvertToUtf32(character, source[position++]);
        }
        else
        {
            value = character;
        }
        if (value == 0 || value > 0x10FFFF || value is >= 0xD800 and <= 0xDFFF)
        {
            throw Error("Expected a nonzero Unicode scalar value");
        }
        return value;
    }

    private int GetSymbol(string name)
    {
        if (symbols.TryGetValue(name, out var id))
        {
            return id;
        }

        id = NewRule();
        symbols.Add(name, id);
        return id;
    }

    private int NewRule()
    {
        rules.Add(null);
        return rules.Count - 1;
    }

    private string ReadName()
    {
        var start = position;
        while (IsNameCharacter(Current))
        {
            position++;
        }
        if (position == start)
        {
            throw Error("Expected a rule name");
        }
        return source.Substring(start, position - start);
    }

    private static bool IsNameCharacter(char c)
    {
        return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-';
    }

    private void Expect(char character)
    {
        if (Current != character)
        {
            throw Error($"Expected '{character}'");
        }

        position++;
    }

    private void SkipSpace(bool newlineAllowed)
    {
        while (position < source.Length)
        {
            if (Current is ' ' or '\t' || (newlineAllowed && Current is '\r' or '\n'))
            {
                position++;
            }
            else if (Current == '#')
            {
                while (position < source.Length && Current is not ('\r' or '\n'))
                {
                    position++;
                }
            }
            else
            {
                break;
            }
        }
    }

    private FormatException Error(string message)
    {
        var line = 1;
        var column = 1;
        for (var i = 0; i < position; i++)
        {
            if (source[i] == '\r' || (source[i] == '\n' && (i == 0 || source[i - 1] != '\r')))
            {
                line++;
                column = 1;
            }
            else if (source[i] != '\n')
            {
                column++;
            }
        }
        return new FormatException($"{message} at line {line}, column {column}.");
    }

    // Native expansion follows rule references recursively before consuming characters.
    // Reject cycles reachable through nullable prefixes, including repetition of an empty rule.
    private void ValidateRecursion(GrammarElement[][] compiled)
    {
        var nullable = new bool[compiled.Length];
        bool changed;
        do
        {
            changed = false;
            for (var i = 0; i < compiled.Length; i++)
            {
                if (nullable[i])
                {
                    continue;
                }

                var empty = true;
                foreach (var element in compiled[i])
                {
                    if (element.Type is GrammarElementType.Alternative or GrammarElementType.End)
                    {
                        if (empty)
                        {
                            nullable[i] = true;
                            changed = true;
                            break;
                        }
                        empty = true;
                    }
                    else if (element.Type == GrammarElementType.RuleReference)
                    {
                        empty &= nullable[(int)element.Value];
                    }
                    else
                    {
                        empty = false;
                    }
                }
            }
        } while (changed);

        var edges = new List<int>[compiled.Length];
        for (var i = 0; i < compiled.Length; i++)
        {
            edges[i] = [];
            var prefix = true;
            foreach (var element in compiled[i])
            {
                if (element.Type is GrammarElementType.Alternative or GrammarElementType.End)
                {
                    prefix = true;
                }
                else if (prefix && element.Type == GrammarElementType.RuleReference)
                {
                    edges[i].Add((int)element.Value);
                    prefix = nullable[(int)element.Value];
                }
                else if (element.Type != GrammarElementType.RuleReference)
                {
                    prefix = false;
                }
            }
        }
        var state = new byte[compiled.Length];
        var stack = new Stack<(int Rule, int Edge)>();
        for (var i = 0; i < compiled.Length; i++)
        {
            if (state[i] != 0)
            {
                continue;
            }

            state[i] = 1;
            stack.Push((i, 0));
            while (stack.Count > 0)
            {
                var (rule, edge) = stack.Pop();
                if (edge == edges[rule].Count)
                {
                    state[rule] = 2;
                    continue;
                }
                stack.Push((rule, edge + 1));
                var target = edges[rule][edge];
                if (state[target] == 1)
                {
                    throw Error("Left recursion or repetition of an empty expression is not supported");
                }

                if (state[target] == 0)
                {
                    state[target] = 1;
                    stack.Push((target, 0));
                }
            }
        }
    }
}
