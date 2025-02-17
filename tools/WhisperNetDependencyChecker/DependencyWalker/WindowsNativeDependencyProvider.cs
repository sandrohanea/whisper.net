// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Reflection.PortableExecutable;

namespace WhisperNetDependencyChecker.DependencyWalker;
internal class WindowsNativeDependencyProvider : INativeDependencyProvider
{
    public IEnumerable<string> GetDependencies(string nativeLibPath)
    {
        using var fs = new FileStream(nativeLibPath, FileMode.Open, FileAccess.Read);
        using var peReader = new PEReader(fs);

        var peHeaders = peReader.PEHeaders;
        var importDir = peHeaders.PEHeader?.ImportTableDirectory;

        if (importDir == null || importDir.Value.Size == 0)
        {
            yield break;
        }

        var reader = peReader.GetEntireImage();
        var readerContent = reader.GetContent().AsMemory();

        var importTableRVA = importDir.Value.RelativeVirtualAddress;
        var importTableOffset = RvaToOffset(peHeaders, importTableRVA);
        var pos = importTableOffset;

        while (true)
        {
            var nameRVA = BitConverter.ToInt32(readerContent.Span.Slice(pos + 12, 4));

            if (nameRVA == 0)
            {
                break; // End of import table
            }

            var nameOffset = RvaToOffset(peHeaders, nameRVA);
            var libName = ReadNullTerminatedString(readerContent, nameOffset);
            yield return libName;

            pos += 20; // Move to next import descriptor
        }
    }

    static int RvaToOffset(PEHeaders peHeaders, int rva)
    {
        foreach (var section in peHeaders.SectionHeaders)
        {
            var sectionStartRVA = section.VirtualAddress;
            var sectionEndRVA = sectionStartRVA + section.VirtualSize;

            if (rva >= sectionStartRVA && rva < sectionEndRVA)
            {
                return (rva - sectionStartRVA) + section.PointerToRawData;
            }
        }

        throw new InvalidOperationException("RVA not found in any section.");
    }

    static string ReadNullTerminatedString(ReadOnlyMemory<byte> data, int offset)
    {
        var end = offset;
        while (end < data.Length && data.Span[end] != 0)
        {
            end++;
        }

        return System.Text.Encoding.ASCII.GetString(data.Span[offset..end]);
    }
}
