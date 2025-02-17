// Licensed under the MIT license: https://opensource.org/licenses/MIT

using ELFSharp.MachO;

namespace WhisperNetDependencyChecker.DependencyWalker;

internal class MacOsNativeDependencyProvider : INativeDependencyProvider
{
    public IEnumerable<string> GetDependencies(string nativeLibPath)
    {
        var macho = MachOReader.Load(nativeLibPath);
        foreach (var lib in macho.GetCommandsOfType<LoadDylib>())
        {
            yield return lib.Name;
        }
    }
}
