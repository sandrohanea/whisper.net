Running tests for Whisper.net

This document explains how to run the test suites locally, what you need installed, and optional environment variables that can improve reliability when downloading models.

Prerequisites
- .NET SDKs: The repository targets multiple TFMs. Install the latest 8.0, 9.0, and 10.0 .NET SDKs (as used in CI).
- Internet access: Some tests download Whisper ggml models on first run.
- Platform specifics for native runtimes may apply (see root readme for runtime prerequisites).
- Alternative: fully offline/local runs. You can pre-download a model once and point tests to it (see Offline/local model cache section below).

Test projects
- tests/Whisper.net.Tests: Standard .NET tests (xUnit) executed on Windows, Linux, and macOS.
- tests/Whisper.net.Maui.Tests: MAUI tests that target Android and iOS simulators (primarily exercised in CI).

Basic usage
- Run all tests in the main solution:
  - dotnet test ./Whisper.net.sln
- Run only the core tests:
  - dotnet test ./tests/Whisper.net.Tests/Whisper.net.Tests.csproj

Running tests from IDEs
- Visual Studio 2022: Open Whisper.net.sln and use Test Explorer to run tests. Ensure required SDKs/workloads are installed (see root README and DEVELOPMENT.md).
- JetBrains Rider: Open the solution and use the Unit Tests tool window. Rider discovers xUnit tests automatically; you can filter by project.
- Visual Studio Code: Use the Dev Containers extension for a preconfigured environment or install the C# extensions and .NET SDKs locally. Run tests via the integrated terminal (dotnet test) or configure tasks.

Model downloads during tests
- The xUnit tests download a small ggml model (e.g., tiny) when needed. Files are saved to your OS temp directory and cleaned up by the test fixture.
- To reduce rate limiting from Hugging Face when downloading models, you may set an access token via HF_TOKEN.

Offline/local model cache (no network)
- Pre-download a model (e.g., ggml-tiny) using your preferred method and keep it on disk.
- Point tests to that file by setting either of these environment variables:
  - WHISPER_TEST_MODEL_PATH=/full/path/to/your/ggml-tiny.bin  # source file on your disk
  - Note: The WHISPER_TEST_MODEL_PATH remains the source file path; the deterministic temp file is the destination the tests operate on.

Environment variables
- HF_TOKEN (optional)
  - Purpose: Adds an Authorization header to Hugging Face model downloads to avoid rate limiting.
  - Examples:
    - Bash: export HF_TOKEN=hf_xxx
    - PowerShell: $env:HF_TOKEN = "hf_xxx"

MAUI tests (advanced)
- Android
  - Requires Android SDK, an emulator, and (optionally) xharness if you want to reproduce CI flows locally.
  - Example CI command (runs inside GitHub Actions):
    - xharness android test --app=./maui-build-artifacts/net10.0-android/com.companyname.whisper.net.maui.tests-Signed.apk -p com.companyname.whisper.net.maui.tests -i com.companyname.whisper.net.maui.tests.AndroidMauiTestInstrumentation -o=./test-results/android
- iOS (macOS only)
  - Requires Xcode, iOS simulators, and xharness to mirror CI execution.
  - Example CI command:
    - xharness apple test --app=./maui-build-artifacts/net10.0-ios/iossimulator-arm64/Whisper.net.Maui.Tests.app --output-directory=./test-results/ios --target=ios-simulator-64 --device="iPhone 16" --timeout "00:30:00"
- Tip: For most local development, it is sufficient to run tests/Whisper.net.Tests. MAUI tests are primarily validated in CI.
- Docs: Dotnet XHarness documentation: https://github.com/dotnet/xharness

Native runtime usage in tests
- By default, tests use the native runtimes from the NuGet packages (no manual setup required).
- If you are developing native code locally and want tests to pick up your local builds instead of NuGet, see the root README section “Building The Runtime” and ensure your build output matches the expected runtimes layout under ./runtimes, or override probing as appropriate.

Troubleshooting
- Slow or failing downloads:
  - Set HF_TOKEN to avoid rate limiting by Hugging Face when fetching models.
  - Or run fully offline using WHISPER_TEST_MODEL_PATH as described above.
- Native runtime issues:
  - See the Runtimes Description and prerequisites in the root readme for OS-specific dependencies.

Links
- Root README: ../readme.md
- CI workflows (reference for environment variables and commands): .github/workflows/
- XHarness docs: https://github.com/dotnet/xharness
