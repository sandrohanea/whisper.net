# Whisper.net
Open-Source Whisper.net

Dotnet bindings for OpenAI Whisper made possible by [whisper.cpp](https://github.com/ggerganov/whisper.cpp)

Native builds:

[![Linux](https://github.com/sandrohanea/whisper.net/actions/workflows/linux-native-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/linux-native-build.yml)
[![Windows](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-native-build.yml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/windows-native-build.yml)
[![MacOs](https://github.com/sandrohanea/whisper.net/actions/workflows/macos-native-build.yaml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/macos-native-build.yaml)
[![Android](https://github.com/sandrohanea/whisper.net/actions/workflows/android-native-build.yaml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/android-native-build.yaml)
[![Wasm](https://github.com/sandrohanea/whisper.net/actions/workflows/wasm-native-build.yaml/badge.svg?branch=main)](https://github.com/sandrohanea/whisper.net/actions/workflows/wasm-native-build.yaml)

## Getting started

To install Whisper.net, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

    PM> Install-Package Whisper.net
    PM> Install-Package Whisper.net.Runtime

or simply add a package reference in your csproj:

```
    <PackageReference Include="Whisper.net" Version="1.5.0" />
    <PackageReference Include="Whisper.net.Runtime" Version="1.5.0" />
```

## TFL Community

We also have a Teams for Life community, where people can ask questions about the library or just discuss with others about their experience.
You can join the community [using this link](https://teams.live.com/l/community/EBADsHLtqswx_2OnwI)

## Runtime

The runtime package, Whisper.net.Runtime, contains the native whisper.cpp library and it is required in order to run Whisper.net.

## CoreML Runtime

Whisper.net.Runtime.CoreML contains the native whisper.cpp library with Apple CoreML support enabled. Using this on Apple hardware (macOS, iOS, etc.) can net performance improvements over the core runtimes. To use it, reference the `Whisper.net.Runtime.CoreML` nuget,

```
    <PackageReference Include="Whisper.net" Version="1.5.0" />
    <PackageReference Include="Whisper.net.Runtime.CoreML" Version="1.5.0" />
```

Note that only the CoreML built libraries are available in this package and does not contain libraries for other platforms (Linux, Windows, etc). If you are creating a cross-platform application you can use conditional target frameworks to install the correct library package for each version.

Using the ggml whisper models with CoreML requires an additional `mlmodelc` file to be placed alongside your whisper model.

You can download and extract these using [WhisperGgmlDownloader](https://github.com/sandrohanea/whisper.net/blob/main/Whisper.net/Ggml/WhisperGgmlDownloader.cs#L45). Check the [CoreML example](https://github.com/sandrohanea/whisper.net/blob/main/examples/CoreML/Program.cs).

You can also generate these via the [whisper.cpp scripts](https://github.com/ggerganov/whisper.cpp#core-ml-support). As whisper.cpp uses filepaths to detect this folder, you must load your whisper model with a file path.

If successful, the whisper output logs will announce:

`whisper_init_state: loading Core ML model from...`

If not, it will announce an error and use the original core library instead.

## GPU Support

Dependeing on your GPU, you can use either `Whisper.net.Runtime.Cublas` or `Whisper.net.Runtime.Clblast`.
For now, they are only available on Windows x64 and Linux x64 (only Cublas).

Check the Cublas and Clblast examples.

## Blazor and WASM

Blazor is supported with both InteractivityServer and InteractivityWebAssemly. You can check the Blazor example [here](https://github.com/sandrohanea/whisper.net/tree/main/examples/BlazorApp).

## Versioning

Each version of Whisper.net is tied to a specific version of Whisper.cpp. The version of Whisper.net is the same as the version of Whisper it is based on. For example, Whisper.net 1.2.0 is based on Whisper.cpp 1.2.0.

However, the patch version is not tied to Whisper.cpp. For example, Whisper.net 1.2.1 is based on Whisper.cpp 1.2.0 and Whisper.net 1.5.0 is based on Whisper.cpp 1.5.1.

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
- Windows ARM
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
