// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.LibraryLoader;

public class RuntimeOptions
{
    private static readonly List<RuntimeLibrary> defaultRuntimeOrder = [RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.CoreML, RuntimeLibrary.OpenVino, RuntimeLibrary.Cpu, RuntimeLibrary.CpuNoAvx];
    internal bool BypassLoading { get; private set; }
    internal string? LibraryPath { get; private set; }
    internal bool UseGpu { get; private set; }
    internal bool UseFlashAttention { get; private set; }
    internal bool UseDtwTimeStamps { get; private set; }
    internal int HeadsPreset { get; private set; }
    internal int GpuDevice { get; private set; }
    internal List<RuntimeLibrary> RuntimeLibraryOrder { get; private set; }
    internal RuntimeLibrary? LoadedLibrary { get; private set; }

    public static RuntimeOptions Instance { get; } = new();

    private RuntimeOptions()
    {
        BypassLoading = false;
        LibraryPath = null;
        UseGpu = true;
        UseFlashAttention = false;
        UseDtwTimeStamps = false;
        HeadsPreset = 0;
        RuntimeLibraryOrder = defaultRuntimeOrder;
        GpuDevice = 0;
    }

    /// <summary>
    /// Sets a custom path to the Whisper native library.
    /// </summary>
    /// <remarks>
    /// By default it is null and automatic path is inferred from the current platform.
    /// </remarks>
    public void SetLibraryPath(string? path)
    {
        LibraryPath = path;
    }

    /// <summary>
    /// Bypasses the automatic loading of the Whisper native library.
    /// </summary>
    /// <remarks>
    /// By default, it is false.
    /// </remarks>
    public void SetBypassLoading(bool bypass)
    {
        BypassLoading = bypass;
    }

    /// <summary>
    /// Sets whether to use the GPU for processing.
    /// </summary>
    /// <remarks>
    /// By default, it is true.
    /// </remarks>
    public void SetUseGpu(bool useGpu)
    {
        UseGpu = useGpu;
    }

    /// <summary>
    /// Sets whether to use the FlashAttention for processing.
    /// </summary>
    /// <remarks>
    /// By default, it is false.
    /// </remarks>
    public void SetUseFlashAttention(bool useFlashAttention)
    {
        UseFlashAttention = useFlashAttention;
    }

    /// <summary>
    /// Sets the GPU device to use for processing.
    /// </summary>
    /// <remarks>
    /// By default, it is 0.
    /// </remarks>
    public void SetGpuDevice(int device)
    {
        GpuDevice = device;
    }

    /// <summary>
    /// Sets the order of the runtime libraries to use for processing.
    /// </summary>
    /// <remarks>
    /// By default, it is [RuntimeLibrary.Cuda, RuntimeLibrary.Vulkan, RuntimeLibrary.CoreML, RuntimeLibrary.OpenVino, RuntimeLibrary.Cpu].
    /// </remarks>
    public void SetRuntimeLibraryOrder(List<RuntimeLibrary> order)
    {
        RuntimeLibraryOrder = order;
    }

    /// <summary>
    /// Sets the loaded library if it was loaded manually.
    /// </summary>
    public void SetLoadedLibrary(RuntimeLibrary library)
    {
        LoadedLibrary = library;
    }

    /// <summary>
    /// Sets whether to use DTW timestamps.
    /// </summary>
    /// <remarks>
    /// By default, it is false.
    /// </remarks>
    public void SetUseDtwTimeStamps(bool useDtw)
    {
        UseDtwTimeStamps = useDtw;
    }

    /// <summary>
    /// Sets heads preset for DTW.
    /// </summary>
    /// <remarks>
    /// By default, it is 0.
    /// </remarks>
    public void SetHeadsPreset(int headsPreset)
    {
        HeadsPreset = headsPreset;
    }

    /// <summary>
    /// Resets the runtime options to their default values.
    /// </summary>
    public void Reset()
    {
        BypassLoading = false;
        LibraryPath = null;
        UseGpu = true;
        UseFlashAttention = false;
        UseDtwTimeStamps = false;
        HeadsPreset = 0;
        RuntimeLibraryOrder = defaultRuntimeOrder;
        GpuDevice = 0;
    }

}
