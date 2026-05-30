# Whisper.net Copilot instructions

When working in this repository, prefer the project skills under `.github/skills/` for repeatable workflows.

## Skill routing

- Use `Validate managed API changes` for ordinary C# API, processor, builder, test, example, and project-file changes.
- Use `Run .NET tests with preview native libs` before managed test runs when native runtimes are needed locally and the task does not change `whisper.cpp`, native build scripts, runtime package targets, or native binary outputs.
- Use `Change native runtimes` when changing `whisper.cpp`, native build scripts, native workflows, runtime artifact layout, or native runtime package contents.
- Use `Validate runtime loading` when changing `RuntimeOptions`, `RuntimeLibrary`, `NativeLibraryLoader`, runtime selection, dependency checking, or examples that force runtime order.
- Use `Validate No-AVX tests` for `CpuNoAvx`, `Whisper.net.Runtime.NoAvx`, or `USE_WHISPER_NOAVX_TESTS` changes.
- Use `Validate MAUI mobile tests` for MAUI target frameworks, mobile runtime packaging, or iOS/Android test behavior.
- Use `Validate runtime packages and releases` for NuGet packaging, `.nuspec`, `.targets`, versioning, or release workflow changes.
- Use `Manage Whisper test models` when `WHISPER_TEST_MODEL_PATH` is involved or model downloads/cache behavior blocks tests.

## Repository hygiene

The preview native libs are published by `.github/workflows/upload-build-artifacts.yml` as prerelease tags named `preview-nativelibs-<whisper.cpp short sha>` with an asset named `native-runtimes.zip`.

Do not commit downloaded native binaries, model files, or packed NuGet artifacts unless the task explicitly asks to update those artifacts.
