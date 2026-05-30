---
name: Validate MAUI mobile tests
description: Use when changing MAUI target frameworks, mobile runtime packaging, or mobile test behavior.
---

# Validate MAUI mobile tests

Use this skill for changes involving MAUI target frameworks, mobile runtime packaging, iOS/Android test projects, or `.github/workflows/dotnet-maui.yml`.

## Environment flags

MAUI builds rely on these flags:

```powershell
$env:USE_WHISPER_MAUI = "true"
$env:USE_WHISPER_MAUI_TESTS = "true"
```

Clear them after local validation:

```powershell
Remove-Item Env:\USE_WHISPER_MAUI
Remove-Item Env:\USE_WHISPER_MAUI_TESTS
```

## Restore and build

From the repository root:

```powershell
$env:USE_WHISPER_MAUI = "true"
$env:USE_WHISPER_MAUI_TESTS = "true"
dotnet workload restore .\Whisper.net.Maui.Tests.slnx
dotnet restore .\Whisper.net.Maui.Tests.slnx
dotnet build .\Whisper.net.Maui.Tests.slnx --no-restore -warnaserror
```

## Device and simulator tests

The CI workflow uses XHarness for simulator/device execution. Prefer matching `.github/workflows/dotnet-maui.yml` over inventing local commands, because target frameworks and simulator images change over time.

Run mobile tests locally only when the required workloads, SDKs, emulators, and simulator images are installed. Otherwise, validate restore/build locally and rely on CI for simulator execution.

## Files to inspect

- `.github/workflows/dotnet-maui.yml`
- `Whisper.net.Maui.Tests.slnx`
- `Whisper.net/Whisper.net.csproj`
- `tests/Whisper.net.Tests.Maui/Whisper.net.Tests.Maui.csproj`
- `tests/Whisper.net.Tests/Whisper.net.Tests.csproj`
- Mobile runtime package `.nuspec` and `.targets` files under `runtimes/`
