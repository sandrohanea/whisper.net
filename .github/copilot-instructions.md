# Whisper.net Copilot instructions

## Project skills

- When running managed .NET tests for changes that do not modify `whisper.cpp` or native runtime build outputs, use the repository skill in `.github/skills/run-dotnet-tests-with-preview-nativelibs/SKILL.md`.
- Prefer downloading the latest `preview-nativelibs-*` release asset (`native-runtimes.zip`) and copying its `runtime-artifacts/` contents into `runtimes/` for local test validation instead of rebuilding native runtimes.
- Do not commit downloaded native binaries unless the task explicitly asks to update runtime artifacts.
