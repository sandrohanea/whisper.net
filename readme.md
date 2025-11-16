# Whisper.net

Open-Source Whisper.net

Dotnet bindings for OpenAI Whisper made possible by [whisper.cpp](https://github.com/ggerganov/whisper.cpp)

## Build Status

| Build type | Build Status |
|----------|--------------|
| CI Status (Native + dotnet) | [![CI (Native + dotnet)](https://github.com/sandrohanea/whisper.net/actions/workflows/build-all.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/build-all.yml) |

## Getting Started

To install Whisper.net with all the available runtimes, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

```
    PM> Install-Package Whisper.net.AllRuntimes
```

Or add a package reference in your `.csproj` file:

```
    <PackageReference Include="Whisper.net.AllRuntimes" Version="1.9.0" />
```

`Whisper.net` is the main package that contains the core functionality but does not include any runtimes. `Whisper.net.AllRuntimes` includes all available runtimes for Whisper.net.

### Installing Specific Runtimes

To install a specific runtime, you can install them individually and combine them as needed. For example, to install the CPU runtime, add the following package references:

```
    <PackageReference Include="Whisper.net" Version="1.9.0" />
```
```
    <PackageReference Include="Whisper.net.Runtime" Version="1.9.0" />
```

## GPT for Whisper

We also have a custom-built GPT inside ChatGPT, which can help you with information based on this code, previous issues, and releases. Available [here](https://chat.openai.com/g/g-GQU8iEnAa-whisper-net-helper).

Please try to ask it before publishing a new question here, as it can help you a lot faster.

## Runtimes Description

Whisper.net comes with multiple runtimes to support different platforms and hardware acceleration. Below are the available runtimes:

### Whisper.net.Runtime

The default runtime that uses the CPU for inference. It is available on all platforms and does not require any additional dependencies.

#### Examples:

 - [Simple usage example](https://github.com/sandrohanea/whisper.net/tree/main/examples/Simple)
 - [Simple usage example (without Async processing) ](https://github.com/sandrohanea/whisper.net/blob/main/examples/SimpleSync/Program.cs)
 - [NAudio integration for mp3](https://github.com/sandrohanea/whisper.net/blob/main/examples/NAudioMp3/Program.cs)
 - [NAudio integration for resampled wav](https://github.com/sandrohanea/whisper.net/blob/main/examples/NAudioResampleWav/Program.cs)
 - [Simple channel diarization](https://github.com/sandrohanea/whisper.net/blob/main/examples/Diarization/Program.cs)
 - [Blazor example](https://github.com/sandrohanea/whisper.net/tree/main/examples/BlazorApp)

#### Pre-requisites

 - Windows: Microsoft Visual C++ Redistributable for at least Visual Studio 2022 (x64) [Download Link](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)
 - Windows 11 or Windows Server 2022 (or newer) is required
 - Linux: `libstdc++6`, `glibc 2.31`
 - macOS: TBD
 - For x86/x64 platforms, the CPU must support AVX, AVX2, FMA and F16C instructions. If your CPU does not support these instructions, you'll need to use the `Whisper.net.Runtime.NoAvx` runtime instead.

#### Supported Platforms

- Windows x86, x64, ARM64
- Linux x64, ARM64, ARM
- macOS x64, ARM64 (Apple Silicon)
- Android
- iOS
- MacCatalyst
- tvOS
- WebAssembly

### Whisper.net.Runtime.NoAvx

For CPUs that do not support AVX instructions.

#### Pre-requisites

 - Windows: Microsoft Visual C++ Redistributable for at least Visual Studio 2022 (x64) [Download Link](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-170#latest-microsoft-visual-c-redistributable-version)
 - Windows 11 or Windows Server 2022 (or newer) is required
 - Linux: `libstdc++6`, `glibc 2.31`
 - macOS: TBD

#### Supported Platforms

- Windows x86, x64, ARM64
- Linux x64, ARM64, ARM

### Whisper.net.Runtime.Cuda

Contains the native whisper.cpp library with NVidia CUDA support enabled.

#### Examples

 - [CUDA usage example](https://github.com/sandrohanea/whisper.net/tree/main/examples/NvidiaCuda)

#### Pre-requisites

- Everything from Whisper.net.Runtime pre-requisites
- NVidia GPU with CUDA support
- [CUDA Toolkit (>= 13.0.1)](https://developer.nvidia.com/cuda-downloads)

#### Supported Platforms

- Windows x64
- Linux x64

### Whisper.net.Runtime.CoreML

Contains the native whisper.cpp library with Apple CoreML support enabled.

#### Examples:

- [CoreML example](https://github.com/sandrohanea/whisper.net/tree/main/examples/CoreML)

#### Supported Platforms

- macOS x64, ARM64 (Apple Silicon)
- iOS
- MacCatalyst

### Whisper.net.Runtime.OpenVino

Contains the native whisper.cpp library with Intel OpenVino support enabled.

#### Examples

- [OpenVino Example](https://github.com/sandrohanea/whisper.net/tree/main/examples/OpenVinoExample)

#### Pre-requisites

- Everything from Whisper.net.Runtime pre-requisites
- [OpenVino Toolkit (>= 2024.4)](https://github.com/openvinotoolkit/openvino)

#### Supported Platforms

- Windows x64
- Linux x64

### Whisper.net.Runtime.Vulkan

Contains the native whisper.cpp library with Vulkan support enabled.

#### Examples

- [Vulkan Example](https://github.com/sandrohanea/whisper.net/tree/main/examples/Vulkan)

#### Pre-requisites

- Everything from Whisper.net.Runtime pre-requisites
- [Vulkan Toolkit (>= 1.4.321.1)](https://vulkan.lunarg.com/sdk/home)]

#### Supported Platforms

- Windows x64

## Multiple Runtimes Support

You can install and use multiple runtimes in the same project. The runtime will be automatically selected based on the platform you are running the application on and the availability of the native runtime.

The following order of priority will be used by default:

1. `Whisper.net.Runtime.Cuda` (NVidia devices with all drivers installed)
2. `Whisper.net.Runtime.Vulkan` (Windows x64 with Vulkan installed)
3. `Whisper.net.Runtime.CoreML` (Apple devices)
4. `Whisper.net.Runtime.OpenVino` (Intel devices)
5. `Whisper.net.Runtime` (CPU inference)
6. `Whisper.net.Runtime.NoAvx` (CPU inference without AVX support)

To change the order or force a specific runtime, set the `RuntimeLibraryOrder` on the `RuntimeOptions`:

```csharp
RuntimeOptions.RuntimeLibraryOrder =
[
    RuntimeLibrary.CoreML,
    RuntimeLibrary.OpenVino,
    RuntimeLibrary.Cuda,
    RuntimeLibrary.Cpu
];
```

### Pluggable native runtimes
- Whisper.net can run with any compatible compilation of the native whisper.cpp libraries; the package Whisper.net.Runtime is just one of the possible builds we publish.
- You may build your own native binaries (CPU, CUDA, CoreML, OpenVINO, Vulkan, NoAvx) and use them with Whisper.net as long as their files are arranged under ./runtimes in the same layout as our NuGet packages. The NativeLibraryLoader will probe them at runtime.
- For reproducible builds, you can use the attached GitHub workflows as references or entry points to produce artifacts: .github/workflows/ (e.g., dotnet.yml, dotnet-noavx.yml, dotnet-maui.yml). These workflows compile and package native libraries across platforms and can be adapted for your needs.

## Versioning

Whisper.net follows semantic versioning.

Starting from version `1.8.0`, Whisper.net does not follow the same versioning scheme as `whisper.cpp`, which creates releases based on specific commits in their `master` branch (e.g., b2254, b2255).

To track the `whisper.cpp` version used in a specific Whisper.net release, you can check the `whisper.cpp` submodule. The commit hash for the tag associated with the release will indicate the corresponding `whisper.cpp` version.

## Ggml Models

Whisper.net uses Ggml models to perform speech recognition and translation. You can find more about Ggml models [here](https://github.com/ggerganov/whisper.cpp/tree/master/models).

For easier integration, Whisper.net provides a Downloader using [Hugging Face](https://huggingface.co).

```csharp
var modelName = "ggml-base.bin";
if (!File.Exists(modelName))
{
    using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.Base);
    using var fileWriter = File.OpenWrite(modelName);
    await modelStream.CopyToAsync(fileWriter);
}
```

### Environment variables for model downloads

- HF_TOKEN
  - Optional. If set, Whisper.net will add an Authorization header when downloading models from Hugging Face to avoid rate limiting.
  - Example:
    - Bash: `export HF_TOKEN=hf_xxx`
    - PowerShell: `$env:HF_TOKEN = "hf_xxx"`

## Usage

```csharp
using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

using var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")
    .Build();

using var fileStream = File.OpenRead(wavFileName);

await foreach (var result in processor.ProcessAsync(fileStream))
{
    Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
}
```

## Documentation

You can find the documentation and code samples [here](https://github.com/sandrohanea/whisper.net).

- Development environment setup notes are available in DEVELOPMENT.md.

## Running tests

For instructions on running the test suites locally (including required .NET SDKs, optional environment variables like HF_TOKEN), see tests/README.md.

- Offline/local alternative: You can run tests fully locally without network by pre-downloading all ggml models required by tests and pointing tests to them via WHISPER_TEST_MODEL_PATH.
- MAUI tests use the Dotnet XHarness CLI to drive emulators/simulators. Docs: https://github.com/dotnet/xharness
- Native runtimes: By default, tests and are using the locally built native binaries instead, see “Building The Runtime” in DEVELOPMENT.md and ensure the output matches the expected runtimes layout.

## License

MIT License. See [LICENSE](https://github.com/sandrohanea/whisper.net/blob/main/LICENSE) for details.
