# Whisper.net
Open-Source Whisper.net

Dotnet bindings for OpenAI Whisper made possible by [whisper.cpp](https://github.com/ggerganov/whisper.cpp)


## Getting started

To install Whisper.net, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

    PM> Install-Package Whisper.net
    PM> Install-Package Whisper.net.Runtime

or simply add a package reference in your csproj:

```
    <PackageReference Include="Whisper.net" Version="1.4.4" />
    <PackageReference Include="Whisper.net.Runtime" Version="1.4.4" />
```

## Runtime

The runtime package, Whisper.net.Runtime, contains the native whisper.cpp library and it is required in order to run Whisper.net.

## CoreML Runtime

Whisper.net.Runtime.CoreML contains the native whisper.cpp library with Apple CoreML support enabled. Using this on Apple hardware (macOS, iOS, etc.) can net performance improvements over the core runtimes. To use it, reference the `Whisper.net.Runtime.CoreML` nuget,

```
    <PackageReference Include="Whisper.net" Version="1.4.4" />
    <PackageReference Include="Whisper.net.Runtime.CoreML" Version="1.4.4" />
```

Note that only the CoreML built libraries are available in this package and does not contain libraries for other platforms (Linux, Windows, etc). If you are creating a cross-platform application you can use conditional target frameworks to install the correct library package for each version.

Using the ggml whisper models with CoreML requires an additional `mlmodelc` file to be placed alongside your whisper model. You can generate these via the [whisper.cpp scripts](https://github.com/ggerganov/whisper.cpp#core-ml-support). As whisper.cpp uses filepaths to detect this folder, you must load your whisper model with a file path. If successful, the whisper output logs will announce:

`whisper_init_state: loading Core ML model from...`

If not, it will announce an error and use the original core library instead.

## GPU Support on Windows

Dependeing on your GPU, you can use either `Whisper.net.Runtime.Cublas` or `Whisper.net.Runtime.Clblast`.
For now, they are only available on Windows x64.

Check the Cublas and Clblast examples.

## Blazor and WASM

For Blazor WebAssembly support, check the BlazorWasm example.
Instead of `Whisper.net.Runtime`, you'll need to use `Whisper.net.Runtime.Wasm` but you'll need to also switch `WasmBuildNative` flag to true.

## Versioning

Each version of Whisper.net is tied to a specific version of Whisper.cpp. The version of Whisper.net is the same as the version of Whisper it is based on. For example, Whisper.net 1.2.0 is based on Whisper 1.2.0.

However, there can be cases where Whisper.net patch version is incremented without a corresponding Whisper.cpp version change. This is the case when the changes in Whisper.net are not related to the changes in Whisper.cpp.

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

or

`pwsh ./scripts/build-osx.ps1`

- Compiling the Apple libraries requires a Mac with Xcode installed.

### Apple CoreML:

`make apple_coreml`

- Compiling the Apple libraries requires a Mac with Xcode installed.

### Linux:

`make linux`

or

`pwsh ./scripts/build-linux.ps1`

### Windows:

- Run the `.bat` files in the root of this repo, or the powershell `./script/build-windows.ps1`

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
