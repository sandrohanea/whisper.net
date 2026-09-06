// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.LibraryLoader;

internal static class RuntimePathResolver
{
    public static string GetRuntimePath(
        string runtimesPath,
        WhisperModelFamily modelFamily,
        RuntimeLibrary runtimeLibrary,
        string platform,
        string architecture)
    {
        if (modelFamily == WhisperModelFamily.Parakeet
            && runtimeLibrary is RuntimeLibrary.CoreML or RuntimeLibrary.OpenVino)
        {
            throw new NotSupportedException($"{runtimeLibrary} is not supported by the Parakeet engine.");
        }

        var enginePath = modelFamily == WhisperModelFamily.Parakeet
            ? Path.Combine(runtimesPath, "parakeet")
            : runtimesPath;

        return runtimeLibrary switch
        {
            RuntimeLibrary.Cuda => Path.Combine(enginePath, "cuda", $"{platform}-{architecture}"),
            RuntimeLibrary.Cuda12 => Path.Combine(enginePath, "cuda12", $"{platform}-{architecture}"),
            RuntimeLibrary.Vulkan => Path.Combine(enginePath, "vulkan", $"{platform}-{architecture}"),
            RuntimeLibrary.Cpu => Path.Combine(enginePath, $"{platform}-{architecture}"),
            RuntimeLibrary.CpuNoAvx => Path.Combine(enginePath, "noavx", $"{platform}-{architecture}"),
            RuntimeLibrary.CoreML => Path.Combine(enginePath, "coreml", $"{platform}-{architecture}"),
            RuntimeLibrary.OpenVino => Path.Combine(enginePath, "openvino", $"{platform}-{architecture}"),
            _ => throw new InvalidOperationException("Unknown runtime library")
        };
    }
}
