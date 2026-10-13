// Licensed under the MIT license: https://opensource.org/licenses/MIT

#:property PublishAot=false

using System.Diagnostics;
using System.Text.Json;

if (args.Length == 1 && args[0] == "--help")
{
    Console.WriteLine("IosSimulator.cs ensure-runtime <version> | prepare-device <version> <device name>");
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
    else
    {
        throw new ArgumentException("Expected ensure-runtime <version> or prepare-device <version> <device name>.");
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
