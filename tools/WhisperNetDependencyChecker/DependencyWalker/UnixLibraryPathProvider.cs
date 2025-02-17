// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace WhisperNetDependencyChecker.DependencyWalker;

internal class UnixLibraryPathProvider : IKnownLibraryPathProvider
{
    public IEnumerable<string> GetKnownPaths()
    {
        yield return "/usr/lib";
        yield return "/usr/local/lib";
    }
}
