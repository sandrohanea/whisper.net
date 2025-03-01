// Licensed under the MIT license: https://opensource.org/licenses/MIT

using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace WhisperNetDependencyChecker.DependencyWalker.Unix.Linux;

internal class LinuxNativeDependencyProvider : INativeDependencyProvider
{
    public IEnumerable<string> GetDependencies(string nativeLibPath)
    {
        var elf = ELFReader.Load(nativeLibPath);
        if (elf is not IELF elfFile)
        {
            yield break;
        }

        // Try to get the dynamic section (usually named ".dynamic")
        var dynamicSection = elfFile.Sections
            .FirstOrDefault(sec => sec.Name == ".dynamic") as IDynamicSection;
        var stringTable = elfFile.Sections
            .FirstOrDefault(sec => sec.Name == ".dynstr") as IStringTable;

        if (dynamicSection == null)
        {
            yield break;
        }

        foreach (var entry in dynamicSection.Entries)
        {
            if (entry.Tag == DynamicTag.Needed)
            {
                // Cast the entry to a generic dynamic entry to get the dependency name.
                if (entry is DynamicEntry<string> stringEntry)
                {
                    yield return stringEntry.Value;
                    continue;
                }

                if (entry is DynamicEntry<ulong> ulongEntry && stringTable != null)
                {
                    var longIndex = (long)ulongEntry.Value;
                    yield return stringTable[longIndex];
                    continue;
                }

                if (entry is DynamicEntry<uint> uintEntry && stringTable != null)
                {
                    var uintIndex = (long)uintEntry.Value;
                    yield return stringTable[uintIndex];
                }
            }
        }
    }
}
