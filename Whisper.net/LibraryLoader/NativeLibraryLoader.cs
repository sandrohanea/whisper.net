// Licensed under the MIT license: https://opensource.org/licenses/MIT
#if !IOS && !MACCATALYST && !TVOS && !ANDROID
using System.Runtime.InteropServices;
using Whisper.net.Native;
#endif

namespace Whisper.net.LibraryLoader;

public static class NativeLibraryLoader {
  private static ILibraryLoader? defaultLibraryLoader;

  /// <summary>
  /// Sets the library loader used to load the native libraries. Overwrite this
  /// only if you want some custom loading.
  /// </summary>
  /// <param name="libraryLoader">The library loader to be used.</param>
  /// <remarks>
  /// It needs to be set before the first <seealso cref="WhisperFactory"/> is
  /// created, otherwise it won't have any effect.
  /// </remarks>
  public static void SetLibraryLoader(ILibraryLoader libraryLoader) {
    defaultLibraryLoader = libraryLoader;
  }

  internal static LoadResult LoadNativeLibrary(string? path = default,
                                               bool bypassLoading = false) {

#if IOS || MACCATALYST || TVOS || ANDROID
    // If we're not bypass loading, and the path was set, and loader was set,
    // allow it to go through.
    if (!bypassLoading && defaultLibraryLoader != null) {
      return defaultLibraryLoader.OpenLibrary(path);
    }

    return LoadResult.Success;
#else
    // If the user has handled loading the library themselves, we don't need to
    // do anything.
    if (bypassLoading ||
        RuntimeInformation.OSArchitecture.ToString() == "Wasm") {
      return LoadResult.Success;
    }

    var architecture = RuntimeInformation.OSArchitecture switch {
      Architecture.X64 => "x64", Architecture.X86 => "x86",
      Architecture.Arm => "arm", Architecture.Arm64 => "arm64",
      _ => throw new PlatformNotSupportedException(
          $"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
    };

    var (platform, dynamicLibraryName) = Environment.OSVersion.Platform switch {
      _ when RuntimeInformation.IsOSPlatform(OSPlatform.Windows) =>
          ("win", "whisper.dll"),
      _ when RuntimeInformation.IsOSPlatform(OSPlatform.Linux) =>
          ("linux", "libwhisper.so"),
      _ when RuntimeInformation.IsOSPlatform(OSPlatform.OSX) =>
          ("macos", "libwhisper.dylib"),
      _ => throw new PlatformNotSupportedException(
          $"Unsupported OS platform, architecture: {RuntimeInformation.OSArchitecture}")
    };

    if (string.IsNullOrEmpty(path)) {
      var assemblySearchPath =
          new[] { AppDomain.CurrentDomain.RelativeSearchPath,
                  Path.GetDirectoryName(
                      typeof(NativeMethods).Assembly.Location),
                  Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]) }
              .Where(it => !string.IsNullOrEmpty(it))
              .FirstOrDefault();

      path =
          string.IsNullOrEmpty(assemblySearchPath)
              ? Path.Combine("runtimes", $"{platform}-{architecture}",
                             dynamicLibraryName)
              : Path.Combine(assemblySearchPath, "runtimes",
                             $"{platform}-{architecture}", dynamicLibraryName);
    }

    if (defaultLibraryLoader != null) {
      return defaultLibraryLoader.OpenLibrary(path);
    }

    if (!File.Exists(path)) {
      throw new FileNotFoundException(
          $"Native Library not found in path {path}. " +
          $"Verify you have have included the native Whisper library in your application, " +
          $"or install the default libraries with the Whisper.net.Runtime NuGet.");
    }

    ILibraryLoader libraryLoader = platform switch {
      "win" => new WindowsLibraryLoader(), "macos" => new MacOsLibraryLoader(),
      "linux" => new LinuxLibraryLoader(),
      _ => throw new PlatformNotSupportedException(
          $"Currently {platform} platform is not supported")
    };

    var result = libraryLoader.OpenLibrary(path);
    return result;
#endif
  }
}
