// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Runtime.InteropServices;

namespace WhisperNetDependencyChecker.DependencyWalker;

internal class NativeDependencyWalker
{
    private readonly DependencyGraphLoader dependencyGraphLoader;

    public NativeDependencyWalker()
    {
        INativeDependencyProvider nativeDependencyProvider = RuntimeInformation.OSArchitecture switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new WindowsNativeDependencyProvider(),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) => new LinuxNativeDependencyProvider(),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => new MacOsNativeDependencyProvider(),
            _ => throw new NotImplementedException("Unsupported OS Platform")
        };

        IKnownLibraryPathProvider knownPathProvider = RuntimeInformation.OSArchitecture switch
        {
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) => new WindowsKnownLibraryPathProvider(),
            _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX) => new UnixLibraryPathProvider(),
            _ => throw new NotImplementedException("Unsupported OS Platform")
        };
        dependencyGraphLoader = new DependencyGraphLoader(nativeDependencyProvider, knownPathProvider);
    }

    public IEnumerable<LoadLibResult> TryLoad(string nativeLibraryPath)
    {
        var loadOrder = dependencyGraphLoader.BuildLoadOrder(nativeLibraryPath);
        var loadedLibraries = new Dictionary<string, IntPtr>(StringComparer.OrdinalIgnoreCase);

        // Use the same base directory as the initial library for resolution.
        var baseDirectory = Path.GetDirectoryName(nativeLibraryPath);

        foreach (var libName in loadOrder)
        {
            // Resolve full path.
            var fullPath = dependencyGraphLoader.ResolveLibraryPath(libName, baseDirectory);
            if (fullPath == null)
            {
                yield return new LoadLibResult()
                {
                    LibraryName = libName,
                    LibraryPath = null,
                    LoadError = "Couldn't resolve the path for the native library.",
                    WasLoaded = false
                };
                continue;
            }

            // Try loading the library.
            if (!NativeLibrary.TryLoad(fullPath, out var handle))
            {
                yield return new LoadLibResult()
                {
                    LibraryName = libName,
                    LibraryPath = fullPath,
                    LoadError = "Couldn't load the native library on this system.",
                    WasLoaded = false
                };
                continue;
            }

            loadedLibraries[libName] = handle;
            yield return new LoadLibResult()
            {
                LibraryName = libName,
                LibraryPath = fullPath,
                LoadError = null,
                WasLoaded = true
            };
        }
    }
}
