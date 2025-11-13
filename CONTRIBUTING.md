# Contributing to Whisper.net

Thanks for your interest in improving Whisper.net! This guide describes how the project is organised, how to get a development environment running, and the expectations for pull requests.

## Table of contents
- [Project scope and etiquette](#project-scope-and-etiquette)
- [Repository layout](#repository-layout)
- [Prerequisites](#prerequisites)
- [Getting the source](#getting-the-source)
- [Working with native runtimes](#working-with-native-runtimes)
- [Building the managed code](#building-the-managed-code)
- [Running tests](#running-tests)
- [Coding style](#coding-style)
- [Updating dependencies](#updating-dependencies)
- [Submitting pull requests](#submitting-pull-requests)

## Project scope and etiquette
- Use GitHub issues to report bugs or request features. For security-sensitive reports, please follow the disclosure process described in [`SECURITY.md`](SECURITY.md).
- Whisper.net targets modern .NET (currently .NET 8 and 9) with bindings to the [`whisper.cpp`](https://github.com/ggerganov/whisper.cpp) native library. Please keep contributions focused on these goals.
- Be respectful and collaborative. Discussions in issues and pull requests should stay on-topic and constructive.

## Repository layout
The repository combines managed code, native build tooling, and samples:
- `Whisper.net/` – the main library (`Whisper.net.csproj`).
- `tests/` – unit tests and MAUI UI tests. Test assets live under `tests/TestData`.
- `examples/` – sample applications demonstrating runtime integration scenarios.
- `runtimes/` – MSBuild targets and prebuilt artifacts for native runtimes (CPU, CUDA, Vulkan, CoreML, etc.).
- `whisper.cpp/` – git submodule containing the upstream C/C++ implementation.
- `tools/` – supporting tooling such as the dependency checker.
- Root `CMakeLists.txt` and `Makefile` contain scripts for building the native runtimes on many platforms.

Understanding this structure helps you place new code and tests where maintainers expect them.

## Prerequisites
The minimum toolchain varies depending on what you plan to build:

| Component | Requirement |
|-----------|-------------|
| .NET SDK | .NET 8 SDK or newer (the library multi-targets `netstandard2.0`, `net8.0`, and `net9.0`). |
| IDE/editor | Visual Studio 2026, Visual Studio Code, Rider, or any editor that honours `.editorconfig`. |
| Native build tools | CMake ≥ 3.10 and a compiler toolchain for your target platform when building runtimes from source (see [`Makefile`](Makefile) for platform-specific switches). |
| Optional | Android/iOS tooling if you are working on MAUI or mobile runtimes. |

Install platform SDKs (CUDA, Vulkan SDK, Xcode, etc.) if you are modifying the respective runtimes.

## Getting the source
Clone the repository and initialise the `whisper.cpp` submodule:

```bash
git clone https://github.com/sandrohanea/whisper.net.git
cd whisper.net
git submodule update --init --recursive
```

Keep the submodule on the revision pinned in `.gitmodules`. When proposing updates, include rationale and test coverage for all affected platforms.

## Working with native runtimes
Most features depend on native runtimes placed under `runtimes/Whisper.net.Runtime*`. If you do not want to build every flavour locally, download the prebuilt artifacts:

```bash
wget https://github.com/sandrohanea/whisper.net/releases/download/preview-nativelibs-41fc9de/native-runtimes.zip
unzip native-runtimes.zip
cp runtime-artifacts/* runtimes -r
rm -r runtime-artifacts
```

The `Makefile` documents how to build each runtime manually. If you modify native code, re-run the relevant make targets (e.g. `make linux`, `make windows`, `make wasm`) and update the copied binaries in `runtimes/` accordingly.

## Building the managed code
Restore and build from the solution root:

```bash
dotnet restore
# Build everything without Maui-specific targets
dotnet build Whisper.net.slnx -c Release
```

To include the MAUI targets, pass `-p:USE_WHISPER_MAUI=true` to `dotnet build` or work directly with `Whisper.net.Tests.Maui.slnx`.

## Running tests
Run the cross-platform unit tests after updating any code:

```bash
# Uses the native runtimes copied under runtimes/
dotnet test tests/Whisper.net.Tests/Whisper.net.Tests.csproj
```

The tests target .NET 8, .NET 9, and .NET Framework on Windows. When running on hardware without AVX instructions, set `-p:USE_WHISPER_NOAVX_TESTS=true` so the test project consumes the No-AVX runtime targets.

MAUI integration tests can be executed from compatible environments (Android/iOS toolchains installed):

```bash
# Example: run Android tests
USE_WHISPER_MAUI_TESTS=true dotnet test tests/Whisper.net.Tests.Maui/Whisper.net.Tests.Maui.csproj -f net9.0-android
```

Please ensure new features include automated tests whenever possible, and update `tests/TestData` cautiously because assets are shared between projects.

## Coding style
We rely on the repository-wide `.editorconfig` for formatting and code-style rules:
- Use four spaces for indentation in C#, `var` for locals, and file-scoped namespaces where appropriate.
- Keep nullable reference types enabled and prefer idiomatic C# features such as pattern matching and using declarations.
- Favour descriptive names, explicit disposal via `IDisposable`, and guard clauses over deeply nested conditionals.
- Follow the existing layout in files like [`WhisperFactory.cs`](Whisper.net/WhisperFactory.cs) when introducing new APIs.

Please run `dotnet format` (or enable automatic formatting in your editor) before committing.

## Updating dependencies
The repository uses [Central Package Management](https://learn.microsoft.com/nuget/consume-packages/Central-Package-Management) via [`Directory.Packages.props`](Directory.Packages.props). Update versions there instead of individual project files.

When adding a new package dependency, confirm that it is compatible with all target frameworks. Avoid adding references that break `netstandard2.0` unless absolutely necessary.

## Submitting pull requests
1. Create a topic branch from `main`.
2. Keep commits focused and write clear messages describing *why* the change is necessary.
3. Update documentation (including this file and `readme.md`) when behaviour or tooling changes.
4. Run the relevant builds and tests locally. Mention the commands you ran in the PR description.
5. Ensure CI passes. Changes to native code must pass the platform builds defined in [`.github/workflows`](.github/workflows/).
6. Be responsive to maintainer feedback and keep discussions in the PR thread.

Thank you for helping make Whisper.net better!
