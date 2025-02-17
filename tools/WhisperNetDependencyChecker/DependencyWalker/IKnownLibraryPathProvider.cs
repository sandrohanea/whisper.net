// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace WhisperNetDependencyChecker.DependencyWalker;
internal interface IKnownLibraryPathProvider
{
    IEnumerable<string> GetKnownPaths();
}
