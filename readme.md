# Whisper.net

Open-Source Whisper.net

Dotnet bindings for OpenAI Whisper made possible by [whisper.cpp](https://github.com/ggerganov/whisper.cpp)

Native builds:
[![Linux](https://github.com/sandrohanea/whisper.net/actions/workflows/linux-native-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/linux-native-build.yml)
[![Linux OpenVINO](https://github.com/sandrohanea/whisper.net/actions/workflows/linux-openvino-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/linux-openvino-build.yml)
[![Windows / CUDA](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-native-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-native-build.yml)
[![Windows ARM](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-arm-native-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-arm-native-build.yml)
[![Windows OpenVINO](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-openvino-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-openvino-build.yml)
[![Windows Vulkan](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-vulkan-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-vulkan-build.yml)
[![MacOs](https://github.com/sandrohanea/whisper.net/actions/workflows/macos-native-build.yaml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/macos-native-build.yaml)
[![Android](https://github.com/sandrohanea/whisper.net/actions/workflows/android-native-build.yaml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/android-native-build.yaml)
[![Wasm](https://github.com/sandrohanea/whisper.net/actions/workflows/wasm-native-build.yaml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/wasm-native-build.yaml)

## Getting started

To install Whisper.net with all the available runtimes, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

    PM> Install-Package Whisper.net.AllRuntimes

or simply add a package reference in your csproj:

```
    <PackageReference Include="Whisper.net.AllRuntimes" Version="1.7.0" />
```

`Whisper.net` is the main package that contains the core functionality but does not include any runtimes. `Whisper.net.AllRuntimes` includes all available runtimes for Whisper.net.

If you want to install a specific runtime, you can install them individually and combine them as needed. For example, to install the CPU runtime, run the following command:

```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime" Version="1.7.0" />
```

## GPT for whisper

We also have a custom-built GPT inside chatgpt, which can help you with information based on this code, previous issues and releases available [here](https://chat.openai.com/g/g-GQU8iEnAa-whisper-net-helper).

Please, make sure you try to ask it before publishing a new question here, as it can be a lot faster.

## Runtimes description

Whisper.net comes with multiple runtimes to support different platforms and hardware acceleration. The runtimes are:

### Whisper.net.Runtime

Whisper.net.Runtime is the default runtime that uses the CPU for inference. It is available on all platforms and does not require any additional dependencies. To use it alone, reference the `Whisper.net.Runtime` nuget,
```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime" Version="1.7.0" />
```

#### Supported platforms:

- Windows x86
- Windows x64
- Windows ARM64
- Linux x64
- Linux ARM64
- Linux ARM
- macOS x64
- macOS ARM64 (Apple Silicon)
- Android
- iOS
- MacCatalyst
- tvOS
- WebAssembly

### Whisper.net.Runtime.NoAvx

If you are running on a CPU that does not support AVX instructions, you can use the `Whisper.net.Runtime.NoAvx` runtime. This will provide a fallback for CPUs that do not support AVX instructions.
To use it, reference the `Whisper.net.Runtime.NoAvx` nuget,
```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime.NoAvx" Version="1.7.0" />
```

Examples: [Multiple Examples here](https://github.com/sandrohanea/whisper.net/tree/main/examples)

#### Supported platforms:

- Windows x86
- Windows x64
- Windows ARM64
- Linux x64
- Linux ARM64
- Linux ARM

### Whisper.net.Runtime.Cuda

Whisper.net.Runtime.Cuda contains the native whisper.cpp library with NVidia CUDA support enabled.
Using this on NVidia hardware can net performance improvements over the core runtimes, especially for larger models.
To use it, reference the `Whisper.net.Runtime.Cuda` nuget,
```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime.Cuda" Version="1.7.0" />
```

Example: [CUDA example](https://github.com/sandrohanea/whisper.net/tree/main/examples/NvidiaCuda)

Note: The CUDA runtime requires NVidia drivers with CUDA and CuBLAS support (minimum version 12.1.0).

#### Supported platforms:

- Windows x64
- Linux x64


### Whisper.net.Runtime.CoreML

Whisper.net.Runtime.CoreML contains the native whisper.cpp library with Apple CoreML support enabled. Using this on Apple hardware (macOS, iOS, etc.) can net performance improvements over the core runtimes.
o use it, reference the `Whisper.net.Runtime.CoreML` nuget,

```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime.CoreML" Version="1.7.0" />
```

Example: [CoreML example](https://github.com/sandrohanea/whisper.net/tree/main/examples/CoreML)

Using the ggml whisper models with CoreML requires an additional `mlmodelc` file to be placed alongside your whisper model.

You can download and extract these using [WhisperGgmlDownloader](https://github.com/sandrohanea/whisper.net/blob/main/Whisper.net/Ggml/WhisperGgmlDownloader.cs#L45). Check the [CoreML example](https://github.com/sandrohanea/whisper.net/blob/main/examples/CoreML/Program.cs).

You can also generate these via the [whisper.cpp scripts](https://github.com/ggerganov/whisper.cpp#core-ml-support). As whisper.cpp uses filepaths to detect this folder, you must load your whisper model with a file path.

If successful, the whisper output logs will announce:

`whisper_init_state: loading Core ML model from...`

If not, it will announce an error and use the original core library instead.

#### Supported platforms:

- macOS x64
- macOS ARM64 (Apple Silicon)
- iOS
- MacCatalyst
- tvOS

### Whisper.net.Runtime.OpenVino

Whisper.net.Runtime.OpenVino contains the native whisper.cpp library with Intel OpenVino support enabled.
Using this on Intel hardware can net performance improvements over the core runtimes, especially for larger models.
To use it, reference the `Whisper.net.Runtime.OpenVino` nuget,
```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime.OpenVino" Version="1.7.0" />
```

Example: [OpenVino Example](https://github.com/sandrohanea/whisper.net/tree/main/examples/OpenVinoExample)

Note: This packages requires the [OpenVino Runtime](https://github.com/openvinotoolkit/openvino/releases) to be installed on the target machine.

#### Supported platforms:

- Windows x64
- Linux x64

### Whisper.net.Runtime.Vulkan

Whisper.net.Runtime.Vulkan contains the native whisper.cpp library with Vulkan support enabled.
To use it on Windows, reference the `Whisper.net.Runtime.Vulkan` nuget,
```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime.Vulkan" Version="1.7.0" />
```

Example: [Vulkan Example](https://github.com/sandrohanea/whisper.net/tree/main/examples/Vulkan)

Note: This packages requires the [Vulkan Runtime](https://www.vulkan.org/tools#vulkan-gpu-resources) to be installed on the target machine.

#### Supported platforms:

- Windows x64

## Multiple Runtimes Support

You can install and use multiple runtimes in the same project. For example, you can use `Whisper.net.Runtime` for Windows and `Whisper.net.Runtime.CoreML` for Apple devices.

The runtime will be automatically selected based on the platform you are running the application on and the availability of the native runtime.

The following order of priority will be used be default:

 - `Whisper.net.Runtime.Cuda` (NVidia devices with all drivers installed)
 - `Whisper.net.Runtime.Vulkan` (Windows x64 with Vulkan installed)
 - `Whisper.net.Runtime.CoreML` (Apple devices)
 - `Whisper.net.Runtime.OpenVino` (Intel devices)
 - `Whisper.net.Runtime` (CPU inference)
 - `Whisper.net.Runtime.NoAvx` (CPU inference without AVX support)

 If you want to change the order or force a specific runtime, you can do it by seting the RuntimeOrder on the RuntimeOptions.

 ```csharp
     RuntimeOptions.SetRuntimeLibraryOrder([RuntimeLibrary.CoreML, RuntimeLibrary.OpenVino, RuntimeLibrary.Cuda, RuntimeLibrary.Cpu]);
 ```

## Blazor and WASM

Blazor is supported with both InteractivityServer and InteractivityWebAssemly with `Whisper.net.Runtime` package. You can check the Blazor example [here](https://github.com/sandrohanea/whisper.net/tree/main/examples/BlazorApp).

## Versioning

Each version of Whisper.net is tied to a specific version of Whisper.cpp. The version of Whisper.net is the same as the version of Whisper it is based on. For example, Whisper.net 1.2.0 is based on Whisper.cpp 1.2.0.

However, the patch version is not tied to Whisper.cpp. For example, Whisper.net 1.2.1 is based on Whisper.cpp 1.2.0 and Whisper.net 1.7.0 is based on Whisper.cpp 1.7.1.

## Ggml Models

Whisper.net uses Ggml models to perform speech recognition and translation.

You can find more about Ggml models [here](https://github.com/ggerganov/whisper.cpp/tree/master/models)

Also, for easier integration Whisper.net provides a Downloader which is using https://huggingface.co.

```csharp

    var modelName = "ggml-base.bin";
    if (!File.Exists(modelName))
    {
        using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(GgmlType.Base);
        using var fileWriter = File.OpenWrite(modelName);
        await modelStream.CopyToAsync(fileWriter);
    }

```

## Usage

```csharp

    using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

    using var processor = whisperFactory.CreateBuilder()
        .WithLanguage("auto")
        .Build();

    using var fileStream = File.OpenRead(wavFileName);

    await foreach(var result in processor.ProcessAsync(fileStream))
    {
        Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
    }
```

## Documentation

You can find the documentation and code samples here: [https://github.com/sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net)

## Building The Runtime

The build scripts are a combination of PowerShell scripts and a Makefile. You can read each of them for the underlying `cmake` commands being used, or run them directly from the scripts.

You can also check the github actions available [here](https://github.com/sandrohanea/whisper.net/tree/main/.github/workflows)

### Android:

`make android`

- Before running, create an environment variable for `NDK_PATH` with the path to your Android NDK. For example,

`NDK_PATH=/Users/UserName/Library/Developer/Xamarin/android-sdk-macosx/ndk-bundle`

### Apple:

`make apple`

- Compiling the Apple libraries requires a Mac with Xcode installed.

### Apple CoreML:

`make apple_coreml`

- Compiling the Apple libraries requires a Mac with Xcode installed.

### Linux:

`make linux`

### Windows:

- Import the powershel module `Import-Module ./windows-scripts.ps1`
- Run `BuildWindowsAll` to build all Windows libraries


## License

MIT Licence
[https://github.com/sandrohanea/whisper.net/blob/main/LICENSE](https://github.com/sandrohanea/whisper.net/blob/main/LICENSE)
