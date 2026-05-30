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

## PowerShell workflow

Run from the repository root:

```powershell
$tag = gh release list --repo sandrohanea/whisper.net --limit 100 --json tagName,isPrerelease,publishedAt --jq '.[] | select(.isPrerelease and (.tagName | startswith("preview-nativelibs-"))) | .tagName' | Select-Object -First 1
gh release download $tag --repo sandrohanea/whisper.net --pattern native-runtimes.zip --clobber
Expand-Archive .\native-runtimes.zip -DestinationPath . -Force
Copy-Item .\runtime-artifacts\* .\runtimes\ -Recurse -Force
Remove-Item .\runtime-artifacts -Recurse -Force
Remove-Item .\native-runtimes.zip
dotnet restore .\Whisper.net.slnx
dotnet build .\Whisper.net.slnx --no-restore -warnaserror
dotnet test .\Whisper.net.slnx --no-build --logger "trx"
```

## Bash workflow

Run from the repository root:

```bash
tag="$(gh release list --repo sandrohanea/whisper.net --limit 100 --json tagName,isPrerelease,publishedAt --jq '.[] | select(.isPrerelease and (.tagName | startswith("preview-nativelibs-"))) | .tagName' | head -n 1)"
gh release download "$tag" --repo sandrohanea/whisper.net --pattern native-runtimes.zip --clobber
unzip -o native-runtimes.zip
cp -R runtime-artifacts/* runtimes/
rm -rf runtime-artifacts native-runtimes.zip
dotnet restore ./Whisper.net.slnx
dotnet build ./Whisper.net.slnx --no-restore -warnaserror
dotnet test ./Whisper.net.slnx --no-build --logger "trx"
```

## Test model handling

The preview native libs release contains native runtime artifacts, not necessarily test model files. Do not set `WHISPER_TEST_MODEL_PATH` to `runtimes/` unless the required `ggml-*.bin` model files are present there. Without that variable, the tests download models through the normal test fixture path.

## Repository hygiene

- Treat downloaded native runtimes as local test inputs, not source changes.
- Do not commit copied binaries from `native-runtimes.zip` unless the task explicitly asks to update runtime artifacts.
- Remove temporary `native-runtimes.zip` and `runtime-artifacts/` files after unpacking.
