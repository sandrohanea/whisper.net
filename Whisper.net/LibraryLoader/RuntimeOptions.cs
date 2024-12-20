// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.LibraryLoader;

/// <summary>
/// Provides options for configuring the Whisper runtime.
/// </summary>
/// <remarks>
/// Setting values in this class will affect the behavior of the Whisper runtime only if they are done before any <seealso cref="WhisperFactory"/> is created.
/// </remarks>
public static class RuntimeOptions
{
    private static readonly List<RuntimeLibrary> defaultRuntimeOrder = [RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.CoreML, RuntimeLibrary.OpenVino, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx];

    /// <summary>
    /// Gets or sets a custom path to the Whisper native library.
    /// </summary>
    public static string? LibraryPath { get; set; }

    /// <summary>
    /// Gets or sets the order of the runtime libraries to use for processing.
    /// </summary>
    /// <remarks>
    /// The default order is [RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.CoreML, RuntimeLibrary.OpenVino, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx].
    /// </remarks>
    public static List<RuntimeLibrary> RuntimeLibraryOrder { get; set; } = defaultRuntimeOrder;

    /// <summary>
    /// Gets or sets the library that was loaded by the runtime.
    /// </summary>
    /// <remarks>
    /// Setting a custom value will bypass the automatic loading of the Whisper native library.
    /// If no custom value is used, the library will be loaded automatically based on the <see cref="RuntimeLibraryOrder"/> and the available libraries on the system.
    /// Once a library is loaded, it will be used for all subsequent processing.
    /// </remarks>
    public static RuntimeLibrary? LoadedLibrary { get; set; }
}
