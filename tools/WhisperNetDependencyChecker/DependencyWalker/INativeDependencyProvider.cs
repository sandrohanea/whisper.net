// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace WhisperNetDependencyChecker.DependencyWalker;

internal interface INativeDependencyProvider
{
    public IEnumerable<string> GetDependencies(string nativeLibPath);
}
