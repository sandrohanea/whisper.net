---
name: Validate managed API changes
description: Use when changing Whisper.net managed C# APIs, processors, builders, tests, or examples without changing native runtime outputs.
---

# Validate managed API changes

Use this skill when a task changes managed code in `Whisper.net/`, managed tests, examples, or project metadata, and does not require rebuilding `whisper.cpp` or native runtime packages.

## Start with the managed surface

- Inspect the public API in `Whisper.net/` before changing tests or examples.
- Keep nullable annotations, async disposal, and cancellation behavior consistent with existing code.
- Update examples and README snippets when a public API shape, option, or default behavior changes.
- Add or update tests in `tests/Whisper.net.Tests/` for behavior changes.

## Validation workflow

If native runtimes are already present locally, run:

```powershell
dotnet restore .\Whisper.net.slnx
dotnet build .\Whisper.net.slnx --no-restore -warnaserror
dotnet test .\Whisper.net.slnx --no-build --logger "trx"
```

If native runtimes are not present and the change does not touch `whisper.cpp`, native build scripts, runtime package targets, or native binaries, use the `Run .NET tests with preview native libs` skill first to populate `runtimes/`.

## Test model handling

Most tests can download required models automatically. If model downloads are not appropriate for the environment, use the `Manage Whisper test models` skill to pre-populate `WHISPER_TEST_MODEL_PATH`.

## Avoid over-validating

Do not run native runtime builds for a purely managed API change unless the managed change alters runtime loading, package target imports, or platform-specific behavior.
