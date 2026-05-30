---
name: Change native runtimes
description: Use when changing whisper.cpp, native build scripts, runtime artifact layout, or native runtime package contents.
---

# Change native runtimes

Use this skill when a task changes `whisper.cpp`, native build workflows, `windows-scripts.ps1`, `Makefile`, native dependency packaging, or files under `runtimes/` that describe native runtime packages.

## Do not use preview native libs

Do not validate these changes with downloaded preview native libs. Preview native libs are only a shortcut for managed-only validation. Native runtime changes must build or consume artifacts produced from the changed native inputs.

## Key places to inspect

- `.github/workflows/*native-build.yml` for the CI build matrix.
- `.github/workflows/build-all.yml` and `.github/workflows/upload-build-artifacts.yml` for artifact aggregation and preview release publishing.
- `windows-scripts.ps1` and `Makefile` for local native build entry points.
- `runtimes/*.nuspec` and `runtimes/*/*.targets` for package contents and MSBuild imports.
- `tools/WhisperNetDependencyChecker/Program.cs` for runtime load validation.

## Windows local build entry points

From the repository root, dot-source the script and build only the relevant runtime when possible:

```powershell
. .\windows-scripts.ps1
BuildWindows "Release"
BuildWindowsIntel "Release"
BuildWindowsArm "Release"
BuildWindowsAll "Release"
```

Use the specific function that matches the task. Do not rebuild every native runtime unless the change affects shared native build logic.

## Dependency checker validation

After placing built artifacts under `runtimes/`, validate native loading for the changed runtime:

```powershell
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- CpuNoAvx
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- Cuda
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- Cuda12
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- Vulkan
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- CoreML
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- OpenVino
```

Only run checks for runtimes relevant to the change and available on the current machine.

## Package validation

When native package contents or targets change, also use the `Validate runtime packages and releases` skill before opening a PR.
