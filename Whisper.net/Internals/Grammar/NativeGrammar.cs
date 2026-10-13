// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Whisper.net.Native;

namespace Whisper.net.Internals.Grammar;

// One allocation owns both the pointer table and the END-terminated element arrays.
internal sealed class NativeGrammar : SafeHandleZeroOrMinusOneIsInvalid
{
    public NativeGrammar(ParsedGrammar grammar) : base(true)
    {
        var elementSize = Marshal.SizeOf<GrammarElement>();
        var tableSize = checked(grammar.Rules.Length * IntPtr.Size);
        var size = tableSize;
        foreach (var rule in grammar.Rules)
        {
            size = checked(size + rule.Length * elementSize);
        }

        SetHandle(Marshal.AllocHGlobal(size));
        try
        {
            var offset = tableSize;
            for (var i = 0; i < grammar.Rules.Length; i++)
            {
                Marshal.WriteIntPtr(handle, i * IntPtr.Size, IntPtr.Add(handle, offset));
                foreach (var element in grammar.Rules[i])
                {
                    Marshal.StructureToPtr(element, IntPtr.Add(handle, offset), false);
                    offset += elementSize;
                }
            }
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Apply(ref WhisperFullParams parameters, ParsedGrammar grammar, float penalty)
    {
        parameters.WhisperGrammarElement = DangerousGetHandle();
        parameters.NGrammarRules = (UIntPtr)(uint)grammar.Rules.Length;
        parameters.StartGrammarRule = (UIntPtr)(uint)grammar.StartRule;
        parameters.GrammarPenalty = penalty;
    }

    protected override bool ReleaseHandle()
    {
        Marshal.FreeHGlobal(handle);
        return true;
    }
}
