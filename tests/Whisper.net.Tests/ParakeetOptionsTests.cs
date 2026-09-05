// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.LibraryLoader;
using Xunit;

namespace Whisper.net.Tests;

public class ParakeetOptionsTests
{
    [Fact]
    public void FactoryOptions_DefaultsToWhisper()
    {
        Assert.Equal(WhisperModelFamily.Whisper, WhisperFactoryOptions.Default.ModelFamily);
    }

    [Fact]
    public void FromModelLoader_WithUnknownModelFamily_ShouldThrow()
    {
        var options = WhisperFactoryOptions.Default;
        options.ModelFamily = (WhisperModelFamily)int.MaxValue;

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WhisperFactory.FromModelLoader(new EmptyModelLoader(), options));
    }

    [Fact]
    public void FromModelLoader_WithParakeetFlashAttention_ShouldThrow()
    {
        var options = WhisperFactoryOptions.Default;
        options.ModelFamily = WhisperModelFamily.Parakeet;
        options.UseFlashAttention = true;

        var exception = Assert.Throws<NotSupportedException>(() =>
            WhisperFactory.FromModelLoader(new EmptyModelLoader(), options));

        Assert.Contains("Flash attention", exception.Message);
    }

    [Fact]
    public void FromModelLoader_WithParakeetDtw_ShouldThrow()
    {
        var options = WhisperFactoryOptions.Default;
        options.ModelFamily = WhisperModelFamily.Parakeet;
        options.UseDtwTimeStamps = true;

        var exception = Assert.Throws<NotSupportedException>(() =>
            WhisperFactory.FromModelLoader(new EmptyModelLoader(), options));

        Assert.Contains("DTW", exception.Message);
    }

    [Fact]
    public void RuntimeOptions_ParakeetDefaultsExcludeWhisperOnlyBackends()
    {
        Assert.Equal(
            [RuntimeLibrary.Cuda, RuntimeLibrary.Cuda12, RuntimeLibrary.Vulkan, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx],
            RuntimeOptions.ParakeetRuntimeLibraryOrder);
        Assert.DoesNotContain(RuntimeLibrary.CoreML, RuntimeOptions.ParakeetRuntimeLibraryOrder);
        Assert.DoesNotContain(RuntimeLibrary.OpenVino, RuntimeOptions.ParakeetRuntimeLibraryOrder);
    }

    [Fact]
    public void ProcessorOptions_WithParakeetPrintProgress_ShouldThrow()
    {
        var options = new WhisperProcessorOptions
        {
            ModelFamily = WhisperModelFamily.Parakeet,
            PrintProgress = true
        };

        var exception = Assert.Throws<NotSupportedException>(options.ValidateModelFamilyCompatibility);

        Assert.Contains("Printing progress", exception.Message);
    }

    [Fact]
    public void ProcessorOptions_WithParakeetPrintProgressDisabled_ShouldNotThrow()
    {
        var options = new WhisperProcessorOptions
        {
            ModelFamily = WhisperModelFamily.Parakeet,
            PrintProgress = false
        };

        options.ValidateModelFamilyCompatibility();
    }

    private sealed class EmptyModelLoader : IWhisperModelLoader
    {
        public bool IsEof => true;

        public void Reset()
        {
        }

        public int CopyTo(Span<byte> destination)
        {
            return 0;
        }

        public void Close()
        {
        }

        public void Dispose()
        {
        }
    }
}
