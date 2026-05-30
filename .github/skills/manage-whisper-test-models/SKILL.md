---
name: Manage Whisper test models
description: Use when tests need deterministic model files, model downloads fail, or WHISPER_TEST_MODEL_PATH is involved.
---

# Manage Whisper test models

Use this skill when tests fail because model files are missing, the environment cannot download models, or a task touches model download/test fixture behavior.

## Model path contract

`WHISPER_TEST_MODEL_PATH` must point to a directory that contains test model files, not to a single model file. The test helper expects file names like:

```text
ggml-tiny-noquantization.bin
ggml-tiny-q5_0.bin
```

Do not set `WHISPER_TEST_MODEL_PATH` to `runtimes/` unless those `ggml-*.bin` files are present there.

## Populate the model cache

Use the repository tool to download the models expected by tests:

```powershell
$env:WHISPER_TEST_MODEL_PATH = "$PWD\.test-models"
dotnet run --project .\tools\DownloadModelForTests\DownloadModelForTests.csproj
dotnet test .\Whisper.net.slnx --logger "trx"
Remove-Item Env:\WHISPER_TEST_MODEL_PATH
```

Keep `.test-models/` or any other downloaded model cache out of source control.

## Hugging Face token

Some model downloads may require authentication. If the environment provides `HF_TOKEN`, preserve and pass it through rather than hardcoding tokens or adding tokens to config files.

## Troubleshooting

- If tests fail before downloading, check that `WHISPER_TEST_MODEL_PATH` points to a writable directory.
- If tests fail after downloading, verify both expected file names exist in the directory.
- If native library loading fails, use runtime validation skills instead; model files and native runtimes are separate inputs.
