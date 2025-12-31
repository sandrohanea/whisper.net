// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;
using Whisper.net.LibraryLoader;
using WhisperNetDependencyChecker.DependencyWalker;

var library = args.Length > 0 ? Enum.Parse<RuntimeLibrary>(args[0]) : RuntimeLibrary.Cpu;

var dependencyProvider = new NativeDependencyWalker();

var platform = Environment.OSVersion.Platform switch
{
    _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => "win",
    _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => "linux",
    _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => "macos",
    _ => throw new PlatformNotSupportedException($"Unsupported OS Version")
};

var architecture = RuntimeInformation.ProcessArchitecture switch
{
    Architecture.X64 => "x64",
    Architecture.X86 => "x86",
    Architecture.Arm => "arm",
    Architecture.Arm64 => "arm64",
    _ => throw new PlatformNotSupportedException($"Unsupported process architecture: {RuntimeInformation.ProcessArchitecture}")
};

var assemblyLocation = typeof(DependencyGraphLoader).Assembly.Location;
// NetFramework and Mono will crash if we try to get the directory of an empty string.
var assemblySearchPaths = new[]
    {
                AppDomain.CurrentDomain.RelativeSearchPath,
                AppDomain.CurrentDomain.BaseDirectory,
                GetSafeDirectoryName(assemblyLocation),
                GetSafeDirectoryName(Environment.GetCommandLineArgs().FirstOrDefault()),
            }.Where(it => !string.IsNullOrEmpty(it)).Distinct();

foreach (var assemblySearchPath in assemblySearchPaths)
{
    var runtimesPath = string.IsNullOrEmpty(assemblySearchPath)
         ? "runtimes"
         : Path.Combine(assemblySearchPath, "runtimes");
    var runtimePath = library switch
    {
        RuntimeLibrary.Cuda => Path.Combine(runtimesPath, "cuda", $"{platform}-{architecture}"),
        RuntimeLibrary.Cuda12 => Path.Combine(runtimesPath, "cuda12", $"{platform}-{architecture}"),
        RuntimeLibrary.Vulkan => Path.Combine(runtimesPath, "vulkan", $"{platform}-{architecture}"),
        RuntimeLibrary.Cpu => Path.Combine(runtimesPath, $"{platform}-{architecture}"),
        RuntimeLibrary.CpuNoAvx => Path.Combine(runtimesPath, "noavx", $"{platform}-{architecture}"),
        RuntimeLibrary.CoreML => Path.Combine(runtimesPath, "coreml", $"{platform}-{architecture}"),
        RuntimeLibrary.OpenVino => Path.Combine(runtimesPath, "openvino", $"{platform}-{architecture}"),
        _ => throw new InvalidOperationException("Unknown runtime library")
    };

    if (Directory.Exists(runtimePath))
    {
        Console.WriteLine("Trying from runtime path: " + runtimePath);
        var libName = GetLibraryPath(platform, "whisper", runtimePath);
        var loadResults = dependencyProvider.TryLoad(libName);
        var success = true;
        foreach (var loadResult in loadResults)
        {
            if (loadResult.WasLoaded)
            {
                Console.WriteLine($"[OK] Library `{loadResult.LibraryName}` was loaded successfully with the path: {loadResult.LibraryPath}.");
                continue;
            }

            success = false;
            Console.Write("[ERROR] ");
            if (loadResult.LibraryPath == null)
            {
                Console.WriteLine($"Couldn't resolve the path for the native library `{loadResult.LibraryName}`.");
            }
            else
            {
                Console.WriteLine($"Couldn't load the native library `{loadResult.LibraryName}` from the path: {loadResult.LibraryPath}. Error: {loadResult.LoadError}");
            }
        }

        if (success)
        {
            Console.WriteLine("Successfully loaded from runtime path");
            return 0;
        }
        Console.WriteLine("Failed to load from runtime path");
        Console.WriteLine();
    }
}

return 1;

static string GetLibraryPath(string platform, string libraryName, string runtimePath)
{
    var libraryFileName = platform switch
    {
        "win" => $"{libraryName}.dll",
        "macos" => $"lib{libraryName}.dylib",
        "linux" => $"lib{libraryName}.so",
        _ => throw new PlatformNotSupportedException($"Unsupported OS platform: {platform}")
    };
    return Path.Combine(runtimePath, libraryFileName);
}

static string? GetSafeDirectoryName(string? path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return null;
    }

    try
    {
        return Path.GetDirectoryName(path);
    }
    catch (Exception)
    {
        return null;
    }
}
