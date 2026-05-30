---
name: Validate No-AVX tests
description: Use when changing CPU NoAvx runtime loading, package imports, or tests that must run without AVX.
---

# Validate No-AVX tests

Use this skill for changes involving `CpuNoAvx`, `Whisper.net.Runtime.NoAvx`, `USE_WHISPER_NOAVX_TESTS`, or the no-AVX CI workflow.

## Important behavior

Setting `USE_WHISPER_NOAVX_TESTS=true` changes which runtime targets are imported by the demo, test project, and dependency checker. It should be used only for no-AVX validation.

## Workflow

From the repository root with no-AVX runtime artifacts available under `runtimes/`:

```powershell
$env:USE_WHISPER_NOAVX_TESTS = "true"
dotnet restore .\Whisper.net.slnx
dotnet build .\Whisper.net.slnx --no-restore -warnaserror
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- CpuNoAvx
dotnet test .\Whisper.net.slnx --no-build --logger "trx"
Remove-Item Env:\USE_WHISPER_NOAVX_TESTS
```

If no-AVX native artifacts are not present and the task changed native no-AVX outputs, use the `Change native runtimes` skill to build or obtain matching artifacts instead of using the preview native libs shortcut.

## Files to inspect

- `.github/workflows/dotnet-noavx.yml`
- `.github/workflows/linux-noavx-native-build.yml`
- `.github/workflows/windows-noavx-native-build.yml`
- `runtimes/Whisper.net.Runtime.NoAvx.nuspec`
- `runtimes/Whisper.net.Runtime.NoAvx/Whisper.net.Runtime.NoAvx.targets`
- `Whisper.net/LibraryLoader/NativeLibraryLoader.cs`
- `tools/WhisperNetDependencyChecker/Program.cs`
