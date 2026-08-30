// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.LibraryLoader;

internal sealed class RuntimeLibrarySelection
{
    internal RuntimeLibrarySelection(IReadOnlyList<RuntimeLibrary> runtimeLibraryOrder,
        bool bypassCompatibilityChecks)
    {
        RuntimeLibraryOrder = runtimeLibraryOrder;
        BypassCompatibilityChecks = bypassCompatibilityChecks;
    }

    internal IReadOnlyList<RuntimeLibrary> RuntimeLibraryOrder { get; }

    internal bool BypassCompatibilityChecks { get; }
}

internal static class RuntimeLibrarySelector
{
    internal static RuntimeLibrarySelection Select(RuntimeLibrary? forcedRuntimeLibrary,
        IReadOnlyList<RuntimeLibrary> preferredRuntimeLibraryOrder)
    {
        return forcedRuntimeLibrary.HasValue
            ? new RuntimeLibrarySelection([forcedRuntimeLibrary.Value], bypassCompatibilityChecks: true)
            : new RuntimeLibrarySelection(preferredRuntimeLibraryOrder.ToList(), bypassCompatibilityChecks: false);
    }
}
