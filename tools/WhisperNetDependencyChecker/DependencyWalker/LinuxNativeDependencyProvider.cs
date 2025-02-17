// Licensed under the MIT license: https://opensource.org/licenses/MIT

using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using ELFSharp.ELF.Segments;

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

        var dynamicSection = elfFile.Segments
            .FirstOrDefault(seg => seg.Type == SegmentType.Dynamic);

        if (dynamicSection == null)
        {
            yield break;
        }

        foreach (var entry in ((IDynamicSection)dynamicSection).Entries)
        {
            if (entry.Tag == DynamicTag.Needed)
            {
                var libName = entry.ToString();
                if (libName != null)
                {
                    yield return libName;
                }
            }
        }
    }
}
