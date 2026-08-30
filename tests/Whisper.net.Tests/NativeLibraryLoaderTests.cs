// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.LibraryLoader;
using Xunit;

namespace Whisper.net.Tests;

public class NativeLibraryLoaderTests
{
    [Theory]
    [InlineData(RuntimeLibrary.Cpu)]
    [InlineData(RuntimeLibrary.Cuda)]
    [InlineData(RuntimeLibrary.Cuda12)]
    [InlineData(RuntimeLibrary.Vulkan)]
    [InlineData(RuntimeLibrary.CoreML)]
    [InlineData(RuntimeLibrary.OpenVino)]
    [InlineData(RuntimeLibrary.CpuNoAvx)]
    public void IsRuntimeSupported_WhenRuntimeIsForced_ShouldBypassCompatibilityChecks(RuntimeLibrary runtime)
    {
        var isSupported = NativeLibraryLoader.IsRuntimeSupported(
            runtime,
            platform: "win",
            architecture: "x64",
            runtimeLibraries: [],
            forcedRuntimeLibrary: runtime);

        Assert.True(isSupported);
    }
}
