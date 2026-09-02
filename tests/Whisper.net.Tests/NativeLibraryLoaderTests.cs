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
    public void Select_WhenRuntimeIsForced_ShouldSelectOnlyThatRuntimeAndBypassCompatibilityChecks(
        RuntimeLibrary runtime)
    {
        var selection = RuntimeLibrarySelector.Select(
            forcedRuntimeLibrary: runtime,
            preferredRuntimeLibraryOrder: [RuntimeLibrary.Cuda, RuntimeLibrary.Cpu]);

        Assert.Equal([runtime], selection.RuntimeLibraryOrder);
        Assert.True(selection.BypassCompatibilityChecks);
    }

    [Fact]
    public void Select_WhenRuntimeIsNotForced_ShouldPreservePreferredOrderAndCompatibilityChecks()
    {
        RuntimeLibrary[] preferredOrder = [RuntimeLibrary.Cuda, RuntimeLibrary.Cpu];

        var selection = RuntimeLibrarySelector.Select(
            forcedRuntimeLibrary: null,
            preferredRuntimeLibraryOrder: preferredOrder);

        Assert.Equal(preferredOrder, selection.RuntimeLibraryOrder);
        Assert.False(selection.BypassCompatibilityChecks);
    }

    [Theory]
    [InlineData(RuntimeLibrary.Cpu, "runtimes/parakeet/win-x64")]
    [InlineData(RuntimeLibrary.CpuNoAvx, "runtimes/parakeet/noavx/win-x64")]
    [InlineData(RuntimeLibrary.Cuda, "runtimes/parakeet/cuda/win-x64")]
    [InlineData(RuntimeLibrary.Cuda12, "runtimes/parakeet/cuda12/win-x64")]
    [InlineData(RuntimeLibrary.Vulkan, "runtimes/parakeet/vulkan/win-x64")]
    public void GetRuntimePath_ForParakeet_ShouldUseIsolatedEnginePath(
        RuntimeLibrary runtimeLibrary,
        string expectedPath)
    {
        var path = RuntimePathResolver.GetRuntimePath(
            "runtimes",
            WhisperModelFamily.Parakeet,
            runtimeLibrary,
            "win",
            "x64");

        Assert.Equal(expectedPath.Replace('/', Path.DirectorySeparatorChar), path);
    }

    [Theory]
    [InlineData(RuntimeLibrary.CoreML)]
    [InlineData(RuntimeLibrary.OpenVino)]
    public void GetRuntimePath_ForParakeetWhisperOnlyBackend_ShouldThrow(RuntimeLibrary runtimeLibrary)
    {
        Assert.Throws<NotSupportedException>(() =>
            RuntimePathResolver.GetRuntimePath(
                "runtimes",
                WhisperModelFamily.Parakeet,
                runtimeLibrary,
                "win",
                "x64"));
    }
}
