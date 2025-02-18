// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace WhisperNetDependencyChecker.DependencyWalker.Windows;
internal class WindowsKnownLibraryPathProvider : IKnownLibraryPathProvider
{
    public IEnumerable<string> GetKnownPaths()
    {
        yield return Environment.SystemDirectory;
    }
}
