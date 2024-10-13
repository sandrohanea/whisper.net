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

To install Whisper.net, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

    PM> Install-Package Whisper.net
    PM> Install-Package Whisper.net.Runtime

or simply add a package reference in your csproj:

```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime" Version="1.7.0" />
```

## GPT for whisper

We also have a custom-built GPT inside chatgpt, which can help you with information based on this code, previous issues and releases available [here](https://chat.openai.com/g/g-GQU8iEnAa-whisper-net-helper).

Please, make sure you try to ask it before publishing a new question here, as it can be a lot faster.

## Runtime

The runtime package, Whisper.net.Runtime, contains the native whisper.cpp library and it is required in order to run Whisper.net.

## CoreML Runtime

Whisper.net.Runtime.CoreML contains the native whisper.cpp library with Apple CoreML support enabled. Using this on Apple hardware (macOS, iOS, etc.) can net performance improvements over the core runtimes. To use it, reference the `Whisper.net.Runtime.CoreML` nuget,

```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime.CoreML" Version="1.7.0" />
```

Note that only the CoreML built libraries are available in this package and does not contain libraries for other platforms (Linux, Windows, etc). If you are creating a cross-platform application you can use conditional target frameworks to install the correct library package for each version.

Using the ggml whisper models with CoreML requires an additional `mlmodelc` file to be placed alongside your whisper model.

You can download and extract these using [WhisperGgmlDownloader](https://github.com/sandrohanea/whisper.net/blob/main/Whisper.net/Ggml/WhisperGgmlDownloader.cs#L45). Check the [CoreML example](https://github.com/sandrohanea/whisper.net/blob/main/examples/CoreML/Program.cs).

You can also generate these via the [whisper.cpp scripts](https://github.com/ggerganov/whisper.cpp#core-ml-support). As whisper.cpp uses filepaths to detect this folder, you must load your whisper model with a file path.

If successful, the whisper output logs will announce:

`whisper_init_state: loading Core ML model from...`

If not, it will announce an error and use the original core library instead.

## GPU Support

We support GPU acceleration with the following runtimes:

 - *CUDA (NVidia):* `Whisper.net.Runtime.Cuda` => for Windows x64 and Linux x64
 - *Vulkan:* `Whisper.net.Runtime.Vulkan` => for Windows x64.
 - *OpenVINO:* `Whisper.net.Runtime.OpenVino` => for Windows x64 and Linux x64.

To use any of these, reference the associated nuget package.

Example:

```
    <PackageReference Include="Whisper.net" Version="1.7.0" />
    <PackageReference Include="Whisper.net.Runtime.Cuda" Version="1.7.0" />
```

Note: when using the GPU runtime, make sure you have the latest drivers and the dependency for each platform:

- For Cuda, you will need NVidia Drivers with Cuda and Cublas support (minimum version 12.1.0)
- For Vulkan, you will need [Vulkan Runtime](https://www.vulkan.org/tools#vulkan-gpu-resources)
- For OpenVino, you will need [OpenVino Runtime](https://github.com/openvinotoolkit/openvino/releases)

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

Blazor is supported with both InteractivityServer and InteractivityWebAssemly. You can check the Blazor example [here](https://github.com/sandrohanea/whisper.net/tree/main/examples/BlazorApp).

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

## Examples

Check more examples [here](https://github.com/sandrohanea/whisper.net/tree/main/examples)

## Documentation

You can find the documentation and code samples here: [https://github.com/sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net)

## Building The Runtime

The build scripts are a combination of PowerShell scripts and a Makefile. You can read each of them for the underlying `cmake` commands being used, or run them directly from the scripts.

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

## Supported platforms

Whisper.net is supported on the following platforms:
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
