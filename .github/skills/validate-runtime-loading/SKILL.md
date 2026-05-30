---
name: Validate runtime loading
description: Use when changing runtime selection, RuntimeOptions, NativeLibraryLoader, runtime package imports, or examples that force runtime order.
---

# Validate runtime loading

Use this skill when a task changes runtime discovery or selection, including `RuntimeOptions`, `RuntimeLibrary`, `NativeLibraryLoader`, runtime targets, dependency checker behavior, or examples that set `RuntimeOptions.RuntimeLibraryOrder`.

## Runtime surfaces to keep in sync

- `Whisper.net/LibraryLoader/RuntimeLibrary.cs`
- `Whisper.net/LibraryLoader/RuntimeOptions.cs`
- `Whisper.net/LibraryLoader/NativeLibraryLoader.cs`
- `Whisper.net/LibraryLoader/CudaHelper.cs` for CUDA-specific probing
- `tools/WhisperNetDependencyChecker/Program.cs`
- `examples/MultiRuntime/Program.cs`
- README runtime package and runtime-order documentation
- `runtimes/*.nuspec` and `runtimes/*/*.targets` when package layout changes

## Current runtime order

The default order is:

```text
Cuda, Cuda12, Vulkan, CoreML, OpenVino, Cpu, CpuNoAvx
```

Preserve this order unless the task intentionally changes runtime preference behavior. If the order changes, update XML docs and README text together.

## Validation

With native artifacts present under `runtimes/`, run the dependency checker for each affected runtime:

```powershell
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- CpuNoAvx
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- Cuda
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- Cuda12
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- Vulkan
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- CoreML
dotnet run --project .\tools\WhisperNetDependencyChecker\WhisperNetDependencyChecker.csproj -- OpenVino
```

Then run the managed test suite:

```powershell
dotnet test .\Whisper.net.slnx --logger "trx"
```

Only use the preview native libs shortcut when the change is managed-only and does not alter runtime package layout or native outputs.
