---
name: Validate runtime packages and releases
description: Use when changing NuGet packaging, runtime targets, versioning, or release workflow behavior.
---

# Validate runtime packages and releases

Use this skill for changes to NuGet packaging, `.nuspec` files, runtime `.targets`, package versioning, release automation, or runtime package composition.

## Source of truth

- `Whisper.net/Whisper.net.csproj` contains the main package version and target frameworks.
- `Directory.Packages.props` centralizes package versions.
- `runtimes/*.nuspec` defines runtime package metadata and dependency relationships.
- `runtimes/*/*.targets` controls which native assets are copied into consuming projects.
- `.github/workflows/pack-all.yml` packs all NuGet packages.
- `.github/workflows/release.yml` orchestrates native builds, packing, and optional NuGet push.
- `windows-scripts.ps1` contains the `PackAll` entry point used by CI.

## Local pack workflow

Populate `runtimes/` with the native artifacts being packaged, then run:

```powershell
$version = Select-Xml -Path .\Whisper.net\Whisper.net.csproj -XPath "/Project/PropertyGroup/Version" | ForEach-Object { $_.Node.InnerText }
$env:USE_WHISPER_MAUI = "TRUE"
dotnet workload restore .\Whisper.net.Maui.Tests.slnx
. .\windows-scripts.ps1
PackAll $version
Remove-Item Env:\USE_WHISPER_MAUI
```

If the task does not affect MAUI package targets, it is acceptable to skip MAUI workload installation locally and rely on CI for MAUI-specific packaging, but state that limitation.

## Package checks

- Verify every changed runtime package includes only the intended runtime assets.
- Keep package IDs, dependency IDs, and target import paths aligned.
- If adding a runtime, update all affected package surfaces: enum/loading code, dependency checker support, nuspecs, targets, README runtime list, examples, and workflows.
- Do not commit packed `.nupkg` files unless the task explicitly asks for release artifacts.

## Validation with tests

After package metadata changes, run managed build/tests with populated `runtimes/`:

```powershell
dotnet restore .\Whisper.net.slnx
dotnet build .\Whisper.net.slnx --no-restore -warnaserror
dotnet test .\Whisper.net.slnx --no-build --logger "trx"
```
