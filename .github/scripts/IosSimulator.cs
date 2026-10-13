// Licensed under the MIT license: https://opensource.org/licenses/MIT

#:property PublishAot=false

using System.Diagnostics;
using System.Text.Json;

if (args.Length == 1 && args[0] == "--help")
{
    Console.WriteLine("IosSimulator.cs ensure-runtime <version> | prepare-device <version> <device name> | configure-mlaunch <wrapper path>");
    return 0;
}

try
{
    if (args.Length == 2 && args[0] == "ensure-runtime")
    {
        await EnsureRuntimeAsync(args[1]);
    }
    else if (args.Length == 3 && args[0] == "prepare-device")
    {
        await PrepareDeviceAsync(args[1], args[2]);
    }
    else if (args.Length == 2 && args[0] == "configure-mlaunch")
    {
        await ConfigureMlaunchAsync(args[1]);
    }
    else
    {
        throw new ArgumentException("Expected ensure-runtime <version>, prepare-device <version> <device name>, or configure-mlaunch <wrapper path>.");
    }
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error.Message);
    return 1;
}

static async Task EnsureRuntimeAsync(string version)
{
    using (var data = await ReadSimulatorsAsync())
    {
        if (FindRuntime(data.RootElement, version).ValueKind != JsonValueKind.Undefined)
        {
            Console.WriteLine($"iOS {version} runtime is already available.");
            return;
        }
    }

    var result = await RunAsync("xcodebuild", ["-downloadPlatform", "iOS", "-buildVersion", version]);
    using var installed = await ReadSimulatorsAsync();
    if (FindRuntime(installed.RootElement, version).ValueKind == JsonValueKind.Undefined)
    {
        throw new InvalidOperationException($"iOS {version} runtime is not available after installation (xcodebuild exit code: {result.ExitCode}).");
    }

    // Xcode can return 70 for an already-downloaded runtime. Require an available runtime.
    Console.WriteLine($"iOS {version} runtime is available (xcodebuild exit code: {result.ExitCode}).");
}

static async Task ConfigureMlaunchAsync(string wrapperPath)
{
    var result = await RunAsync("dotnet", ["tool", "list", "--global", "--format", "json"], captureOutput: true);
    if (result.ExitCode != 0)
    {
        throw new InvalidOperationException($"Could not list installed .NET tools (exit code: {result.ExitCode}).");
    }
    using var tools = JsonDocument.Parse(result.Output);
    const string packageId = "microsoft.dotnet.xharness.cli";
    var tool = tools.RootElement.GetProperty("data").EnumerateArray()
        .FirstOrDefault(item => string.Equals(item.GetProperty("packageId").GetString(), packageId, StringComparison.OrdinalIgnoreCase));
    if (tool.ValueKind == JsonValueKind.Undefined)
    {
        throw new InvalidOperationException("XHarness is not installed as a global .NET tool.");
    }
    var version = tool.GetProperty("version").GetString()!;
    var mlaunchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".dotnet", "tools", ".store", packageId, version, packageId, version,
        "runtimes", "any", "native", "mlaunch", "bin", "mlaunch");
    if (!File.Exists(mlaunchPath))
    {
        throw new FileNotFoundException("Could not find the mlaunch bundled with the installed XHarness.", mlaunchPath);
    }
    var help = await RunAsync(mlaunchPath, ["--help"], captureOutput: true);
    if (help.ExitCode != 0 || !help.Output.Contains("--quiet-simulator", StringComparison.Ordinal))
    {
        throw new InvalidOperationException("The installed mlaunch does not support --quiet-simulator.");
    }
    if (OperatingSystem.IsWindows())
    {
        throw new PlatformNotSupportedException("The iOS launcher wrapper requires macOS.");
    }
    wrapperPath = Path.GetFullPath(wrapperPath);
    Directory.CreateDirectory(Path.GetDirectoryName(wrapperPath)!);
    // Single-quote the executable path and forward each original argument intact.
    var quotedPath = "'" + mlaunchPath.Replace("'", "'\"'\"'") + "'";
    await File.WriteAllTextAsync(wrapperPath, $"#!/bin/bash\nset -euo pipefail\nexec {quotedPath} --quiet-simulator \"$@\"\n");
    File.SetUnixFileMode(wrapperPath, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
    var outputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
    if (!string.IsNullOrEmpty(outputFile))
    {
        await File.AppendAllTextAsync(outputFile, $"path={wrapperPath}{Environment.NewLine}");
    }
    Console.WriteLine($"Using mlaunch from XHarness {version} with --quiet-simulator: {wrapperPath}");
}

static async Task PrepareDeviceAsync(string version, string deviceName)
{
    using var data = await ReadSimulatorsAsync();
    var root = data.RootElement;
    var runtime = FindRuntime(root, version);
    if (runtime.ValueKind == JsonValueKind.Undefined)
    {
        throw new InvalidOperationException($"No available iOS {version} simulator runtime.");
    }
    var deviceType = root.GetProperty("devicetypes").EnumerateArray()
        .FirstOrDefault(item => item.GetProperty("name").GetString() == deviceName);
    if (deviceType.ValueKind == JsonValueKind.Undefined)
    {
        throw new InvalidOperationException($"Xcode does not support simulator device {deviceName}.");
    }

    var runtimeId = runtime.GetProperty("identifier").GetString()!;
    var device = root.GetProperty("devices").TryGetProperty(runtimeId, out var devices)
        ? devices.EnumerateArray().FirstOrDefault(item => IsAvailable(item) && item.GetProperty("name").GetString() == deviceName)
        : default;
    string deviceId;
    string state;
    if (device.ValueKind != JsonValueKind.Undefined)
    {
        deviceId = device.GetProperty("udid").GetString()!;
        state = device.GetProperty("state").GetString()!;
    }
    else
    {
        var created = await RunAsync("xcrun", ["simctl", "create", deviceName, deviceType.GetProperty("identifier").GetString()!, runtimeId], captureOutput: true);
        if (created.ExitCode != 0)
        {
            throw new InvalidOperationException($"Could not create {deviceName} simulator (exit code: {created.ExitCode}).");
        }
        deviceId = created.Output.Trim();
        state = "Shutdown";
    }

    if (!Guid.TryParse(deviceId, out _) || state is not ("Shutdown" or "Booted" or "Booting"))
    {
        throw new InvalidOperationException("Simulator returned an invalid UDID or device state.");
    }
    Console.WriteLine($"Selected {deviceName} on iOS {version}: {deviceId} ({state}).");
    var output = $"udid={deviceId}{Environment.NewLine}state={state}{Environment.NewLine}";
    var outputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
    if (!string.IsNullOrEmpty(outputFile))
    {
        await File.AppendAllTextAsync(outputFile, output);
    }
    else
    {
        Console.Write(output);
    }
}

static JsonElement FindRuntime(JsonElement data, string version)
{
    var identifier = "com.apple.CoreSimulator.SimRuntime.iOS-" + version.Replace('.', '-');
    return data.GetProperty("runtimes").EnumerateArray()
        .FirstOrDefault(item => IsAvailable(item) && item.GetProperty("identifier").GetString() == identifier);
}

static bool IsAvailable(JsonElement item)
{
    return item.TryGetProperty("isAvailable", out var available) && available.ValueKind == JsonValueKind.True;
}

static async Task<JsonDocument> ReadSimulatorsAsync()
{
    var result = await RunAsync("xcrun", ["simctl", "list", "--json"], captureOutput: true);
    if (result.ExitCode != 0)
    {
        throw new InvalidOperationException($"Could not query simulators (simctl exit code: {result.ExitCode}).");
    }
    return JsonDocument.Parse(result.Output);
}

static async Task<(int ExitCode, string Output)> RunAsync(string executable, string[] arguments, bool captureOutput = false)
{
    var startInfo = new ProcessStartInfo(executable)
    {
        UseShellExecute = false,
        RedirectStandardOutput = captureOutput
    };
    foreach (var argument in arguments)
    {
        startInfo.ArgumentList.Add(argument);
    }
    using var process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Could not start {executable}.");
    var output = captureOutput ? await process.StandardOutput.ReadToEndAsync() : string.Empty;
    await process.WaitForExitAsync();
    return (process.ExitCode, output);
}
