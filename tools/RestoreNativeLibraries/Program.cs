// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Diagnostics;
using System.IO.Compression;
using System.Net;

const string RepositoryName = "sandrohanea/whisper.net";
const string AssetName = "native-runtimes.zip";

try
{
    var options = Options.Parse(args);
    if (options.ShowHelp)
    {
        Options.PrintUsage();
        return 0;
    }

    var repositoryRoot = options.RepositoryRoot is null
        ? await RunGitAsync(Directory.GetCurrentDirectory(), "rev-parse", "--show-toplevel")
        : Path.GetFullPath(options.RepositoryRoot);
    var nativeCommit = await RunGitAsync(repositoryRoot, "rev-parse", "--short=7", "HEAD:whisper.cpp");
    var tag = $"preview-nativelibs-{nativeCommit}";
    var releaseUrl = $"https://github.com/{RepositoryName}/releases/download/{tag}/{AssetName}";
    var runtimesDirectory = Path.Combine(repositoryRoot, "runtimes");
    var cacheRoot = options.CacheDirectory is null
        ? await GetDefaultCacheRootAsync(repositoryRoot)
        : Path.GetFullPath(options.CacheDirectory);
    var cacheEntry = Path.Combine(cacheRoot, nativeCommit);

    Console.WriteLine($"Native runtimes: whisper.cpp {nativeCommit}");
    Console.WriteLine(options.NoCache ? "Cache: disabled" : $"Cache: {cacheEntry}");

    if (options.CheckOnly)
    {
        var cacheState = options.NoCache ? "disabled" : IsCompleteCacheEntry(cacheEntry) ? "ready" : "missing";
        var installedFileCount = CountInstalledRuntimeFiles(runtimesDirectory);
        var installedRuntimeFamiliesReady = IsValidArtifactsDirectory(runtimesDirectory);
        Console.WriteLine($"Cache state: {cacheState}");
        Console.WriteLine($"Installed runtime files: {installedFileCount}");
        Console.WriteLine($"Whisper and Parakeet runtime families: {(installedRuntimeFamiliesReady ? "ready" : "incomplete")}");
        Console.WriteLine($"Release: {releaseUrl}");
        return installedRuntimeFamiliesReady ? 0 : 1;
    }

    string artifactsDirectory;
    string? temporaryDirectory = null;

    if (options.NoCache)
    {
        temporaryDirectory = CreateTemporaryDirectory();
        try
        {
            artifactsDirectory = await DownloadAndExtractAsync(releaseUrl, temporaryDirectory);
        }
        catch
        {
            Directory.Delete(temporaryDirectory, recursive: true);
            throw;
        }
        Console.WriteLine("Cache: bypassed");
    }
    else
    {
        if (!options.Force && IsCompleteCacheEntry(cacheEntry))
        {
            Console.WriteLine("Cache: hit");
        }
        else
        {
            Directory.CreateDirectory(cacheRoot);
            var lockPath = Path.Combine(cacheRoot, $"{nativeCommit}.lock");
            await using var cacheLock = await AcquireCacheLockAsync(lockPath);

            if (options.Force && Directory.Exists(cacheEntry))
            {
                Directory.Delete(cacheEntry, recursive: true);
            }

            if (!IsCompleteCacheEntry(cacheEntry))
            {
                if (Directory.Exists(cacheEntry))
                {
                    Directory.Delete(cacheEntry, recursive: true);
                }

                var stagingDirectory = Path.Combine(cacheRoot, $".{nativeCommit}.partial-{Environment.ProcessId}-{Guid.NewGuid():N}");
                try
                {
                    await DownloadAndExtractAsync(releaseUrl, stagingDirectory);
                    await File.WriteAllTextAsync(
                        Path.Combine(stagingDirectory, ".complete"),
                        $"tag={tag}{Environment.NewLine}url={releaseUrl}{Environment.NewLine}");
                    Directory.Move(stagingDirectory, cacheEntry);
                }
                finally
                {
                    if (Directory.Exists(stagingDirectory))
                    {
                        Directory.Delete(stagingDirectory, recursive: true);
                    }
                }

                Console.WriteLine("Cache: downloaded");
            }
            else
            {
                Console.WriteLine("Cache: filled by another restore process");
            }
        }

        artifactsDirectory = Path.Combine(cacheEntry, "runtime-artifacts");
    }

    try
    {
        var copiedFileCount = CopyRuntimeArtifacts(artifactsDirectory, runtimesDirectory);
        Console.WriteLine($"Installed {copiedFileCount} runtime files into {runtimesDirectory}");
        return 0;
    }
    finally
    {
        if (temporaryDirectory is not null && Directory.Exists(temporaryDirectory))
        {
            Directory.Delete(temporaryDirectory, recursive: true);
        }
    }
}
catch (CommandLineException exception)
{
    Console.Error.WriteLine(exception.Message);
    Console.Error.WriteLine();
    Options.PrintUsage();
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Failed to restore native runtimes: {exception.Message}");
    return 1;
}

static async Task<string> GetDefaultCacheRootAsync(string repositoryRoot)
{
    var commonGitDirectory = await RunGitAsync(
        repositoryRoot,
        "rev-parse",
        "--path-format=absolute",
        "--git-common-dir");
    var commonGitDirectoryPath = Path.IsPathRooted(commonGitDirectory)
        ? commonGitDirectory
        : Path.GetFullPath(commonGitDirectory, repositoryRoot);
    var primaryCheckout = Directory.GetParent(commonGitDirectoryPath)?.FullName
        ?? throw new InvalidOperationException($"Cannot determine the primary checkout from '{commonGitDirectoryPath}'.");

    return Path.Combine(primaryCheckout, ".whisper", "native-runtimes");
}

static async Task<string> RunGitAsync(string workingDirectory, params string[] arguments)
{
    var startInfo = new ProcessStartInfo("git")
    {
        WorkingDirectory = workingDirectory,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false
    };
    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }

    using var process = Process.Start(startInfo)
        ?? throw new InvalidOperationException("Failed to start git. Ensure it is installed and available on PATH.");
    var standardOutput = await process.StandardOutput.ReadToEndAsync();
    var standardError = await process.StandardError.ReadToEndAsync();
    await process.WaitForExitAsync();

    if (process.ExitCode != 0)
    {
        throw new InvalidOperationException($"git {string.Join(' ', arguments)} failed: {standardError.Trim()}");
    }

    return standardOutput.Trim();
}

static async Task<FileStream> AcquireCacheLockAsync(string lockPath)
{
    var waitingMessageWritten = false;
    var timeoutAt = DateTime.UtcNow.AddMinutes(10);
    while (true)
    {
        try
        {
            return new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException) when (DateTime.UtcNow < timeoutAt)
        {
            if (!waitingMessageWritten)
            {
                Console.WriteLine("Cache: waiting for another restore process");
                waitingMessageWritten = true;
            }

            await Task.Delay(500);
        }
    }
}

static async Task<string> DownloadAndExtractAsync(string releaseUrl, string destinationDirectory)
{
    Directory.CreateDirectory(destinationDirectory);
    var archivePath = Path.Combine(destinationDirectory, AssetName);

    using var httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromMinutes(10)
    };
    httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("whisper.net-native-runtime-restorer/1.0");

    for (var attempt = 1; ; attempt++)
    {
        try
        {
            Console.WriteLine($"Downloading {releaseUrl}");
            using var response = await httpClient.GetAsync(releaseUrl, HttpCompletionOption.ResponseHeadersRead);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new InvalidOperationException(
                    $"No preview native runtime release exists at {releaseUrl}. " +
                    "Build the native runtimes when the pinned whisper.cpp revision or native build inputs have changed.");
            }

            response.EnsureSuccessStatusCode();
            await using var archive = new FileStream(archivePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(archive);
            break;
        }
        catch (Exception exception) when (attempt < 4 && exception is HttpRequestException or TaskCanceledException)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
            Console.WriteLine($"Download failed; retrying in {delay.TotalSeconds:0} second(s)");
            await Task.Delay(delay);
        }
    }

    var extractionDirectory = Path.Combine(destinationDirectory, "extracted");
    ZipFile.ExtractToDirectory(archivePath, extractionDirectory);
    File.Delete(archivePath);

    var extractedArtifactsDirectory = Path.Combine(extractionDirectory, "runtime-artifacts");
    ValidateArtifactsDirectory(extractedArtifactsDirectory);
    var artifactsDirectory = Path.Combine(destinationDirectory, "runtime-artifacts");
    Directory.Move(extractedArtifactsDirectory, artifactsDirectory);
    Directory.Delete(extractionDirectory, recursive: true);
    return artifactsDirectory;
}

static bool IsCompleteCacheEntry(string cacheEntry)
{
    var artifactsDirectory = Path.Combine(cacheEntry, "runtime-artifacts");
    return File.Exists(Path.Combine(cacheEntry, ".complete")) && IsValidArtifactsDirectory(artifactsDirectory);
}

static void ValidateArtifactsDirectory(string artifactsDirectory)
{
    if (!IsValidArtifactsDirectory(artifactsDirectory))
    {
        throw new InvalidDataException(
            "The downloaded archive does not contain complete Whisper.net.Runtime and Whisper.net.Runtime.Parakeet families.");
    }
}

static bool IsValidArtifactsDirectory(string artifactsDirectory)
{
    var cpuRuntimeDirectory = Path.Combine(artifactsDirectory, "Whisper.net.Runtime");
    var parakeetCpuRuntimeDirectory = Path.Combine(artifactsDirectory, "Whisper.net.Runtime.Parakeet");
    return ContainsMainLibrary(cpuRuntimeDirectory, "whisper")
        && ContainsMainLibrary(parakeetCpuRuntimeDirectory, "parakeet");
}

static bool ContainsMainLibrary(string runtimeDirectory, string libraryName)
{
    if (!Directory.Exists(runtimeDirectory))
    {
        return false;
    }

    var expectedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        $"{libraryName}.dll",
        $"lib{libraryName}.so",
        $"lib{libraryName}.dylib",
        $"lib{libraryName}.a",
    };
    return Directory.EnumerateFiles(runtimeDirectory, "*", SearchOption.AllDirectories)
        .Any(file => expectedFileNames.Contains(Path.GetFileName(file)));
}

static int CopyRuntimeArtifacts(string sourceDirectory, string destinationDirectory)
{
    var copiedFileCount = 0;
    foreach (var runtimeDirectory in Directory.EnumerateDirectories(sourceDirectory, "Whisper.net.Runtime*"))
    {
        foreach (var sourceFile in Directory.EnumerateFiles(runtimeDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, sourceFile);
            var destinationFile = Path.Combine(destinationDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationFile)!);
            File.Copy(sourceFile, destinationFile, overwrite: true);
            copiedFileCount++;
        }
    }

    return copiedFileCount;
}

static int CountInstalledRuntimeFiles(string runtimesDirectory)
{
    if (!Directory.Exists(runtimesDirectory))
    {
        return 0;
    }

    return Directory.EnumerateDirectories(runtimesDirectory, "Whisper.net.Runtime*")
        .SelectMany(directory => Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        .Count(file => !file.EndsWith(".targets", StringComparison.OrdinalIgnoreCase));
}

static string CreateTemporaryDirectory()
{
    var directory = Path.Combine(Path.GetTempPath(), $"whisper-native-runtimes-{Guid.NewGuid():N}");
    Directory.CreateDirectory(directory);
    return directory;
}

internal sealed record Options(
    bool CheckOnly,
    bool Force,
    bool NoCache,
    bool ShowHelp,
    string? CacheDirectory,
    string? RepositoryRoot)
{
    public static Options Parse(string[] arguments)
    {
        var checkOnly = false;
        var force = false;
        var noCache = false;
        var showHelp = false;
        string? cacheDirectory = null;
        string? repositoryRoot = null;

        for (var index = 0; index < arguments.Length; index++)
        {
            switch (arguments[index])
            {
                case "--check":
                    checkOnly = true;
                    break;
                case "--force":
                    force = true;
                    break;
                case "--no-cache":
                    noCache = true;
                    break;
                case "--cache-dir":
                    cacheDirectory = ReadValue(arguments, ref index, "--cache-dir");
                    break;
                case "--repository":
                    repositoryRoot = ReadValue(arguments, ref index, "--repository");
                    break;
                case "--help" or "-h":
                    showHelp = true;
                    break;
                default:
                    throw new CommandLineException($"Unknown option '{arguments[index]}'.");
            }
        }

        if (noCache && cacheDirectory is not null)
        {
            throw new CommandLineException("--no-cache and --cache-dir cannot be used together.");
        }

        if (checkOnly && force)
        {
            throw new CommandLineException("--check and --force cannot be used together.");
        }

        return new Options(checkOnly, force, noCache, showHelp, cacheDirectory, repositoryRoot);
    }

    public static void PrintUsage()
    {
        Console.WriteLine(
            """
            Restore the preview native libraries matching the pinned whisper.cpp revision.

            Usage:
              dotnet run --project tools/RestoreNativeLibraries -- [options]

            Options:
              --check              Report cache and installation state without making changes.
              --force              Redownload the matching cache entry.
              --no-cache           Download to a temporary directory instead of using the shared cache.
              --cache-dir <path>   Override the shared cache directory.
              --repository <path>  Override the repository root (primarily useful for automation).
              -h, --help           Show this help.
            """);
    }

    private static string ReadValue(string[] arguments, ref int index, string option)
    {
        if (++index >= arguments.Length || string.IsNullOrWhiteSpace(arguments[index]))
        {
            throw new CommandLineException($"{option} requires a path.");
        }

        return arguments[index];
    }
}

internal sealed class CommandLineException(string message) : Exception(message);
