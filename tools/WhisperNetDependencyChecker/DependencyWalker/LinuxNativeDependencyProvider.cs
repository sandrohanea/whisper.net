// Licensed under the MIT license: https://opensource.org/licenses/MIT

using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace WhisperNetDependencyChecker.DependencyWalker;

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
                }
            }
        }
    }
}
