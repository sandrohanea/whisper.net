---
name: Run .NET tests with preview native libs
description: Use when validating managed .NET changes that do not modify whisper.cpp or native runtime build outputs.
---

# Run .NET tests with preview native libs

Use this skill when you need to run Whisper.net managed tests and the change does not require rebuilding native runtimes or changing the `whisper.cpp` submodule.

## When to use this

- The task changes managed C# code, tests, docs, samples, or project files only.
- You need native libraries available locally for `dotnet test`.
- You do not need to validate a new `whisper.cpp` revision or native build-system change.

Do not use this shortcut for changes under `whisper.cpp`, native build scripts, runtime package targets, or native binary outputs. In those cases, build the relevant native runtimes instead.

## How the preview package is produced

The `.github/workflows/upload-build-artifacts.yml` workflow:

1. Reads the pinned `whisper.cpp` short commit SHA.
2. Creates a prerelease tag named `preview-nativelibs-<sha>`.
3. Uploads one release asset named `native-runtimes.zip`.
4. Stores the merged native build outputs inside the archive under `runtime-artifacts/`.

Example release:

```text
https://github.com/sandrohanea/whisper.net/releases/tag/preview-nativelibs-f24588a
```

## Restore workflow

Run from the repository root on Windows, macOS, or Linux:

```bash
dotnet run --project tools/RestoreNativeLibraries
```

The restorer derives the release tag from the pinned `whisper.cpp` gitlink. It caches each revision under `.whisper/native-runtimes/` in the primary checkout so linked worktrees do not download the same release repeatedly. Concurrent processes use a cache lock and publish completed entries atomically.

Useful diagnostics and overrides:

```bash
dotnet run --project tools/RestoreNativeLibraries -- --check
dotnet run --project tools/RestoreNativeLibraries -- --force
dotnet run --project tools/RestoreNativeLibraries -- --no-cache
dotnet run --project tools/RestoreNativeLibraries -- --cache-dir <path>
```

After the runtimes are available:

```bash
dotnet restore ./Whisper.net.slnx
dotnet build ./Whisper.net.slnx --no-restore -warnaserror
dotnet test ./Whisper.net.slnx --no-build --logger "trx"
```

## Test model handling

The release can contain test-model artifacts in addition to native runtimes. The restorer intentionally installs only `Whisper.net.Runtime*` directories; model population remains a separate workflow. Do not set `WHISPER_TEST_MODEL_PATH` to `runtimes/`. Without that variable, the tests download models through the normal test fixture path.

## Repository hygiene

- Treat downloaded native runtimes as local test inputs, not source changes.
- Do not commit copied binaries from `native-runtimes.zip` unless the task explicitly asks to update runtime artifacts.
- The restorer cleans up temporary archives and extraction directories automatically.
