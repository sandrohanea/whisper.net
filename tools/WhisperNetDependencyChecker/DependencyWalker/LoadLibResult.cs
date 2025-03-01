// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace WhisperNetDependencyChecker.DependencyWalker;

internal class LoadLibResult
{
    public string LibraryName { get; set; } = string.Empty;

    public string? LibraryPath { get; set; }

    public bool WasLoaded { get; set; }

    public string? LoadError { get; set; }
}
