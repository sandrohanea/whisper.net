# Whisper.net
Open-Source Whisper.net

Dotnet bindings for OpenAI Whisper made possible by [whisper.cpp] (https://github.com/ggerganov/whisper.cpp)

	
## Getting started

To install Whisper.net, run the following command in the [Package Manager Console](http://docs.nuget.org/docs/start-here/using-the-package-manager-console):

    PM> Install-Package Whisper.net

or simply add a package reference in your csproj:

```
    <PackageReference Include="Whisper.net" Version="1.2.1" />
```

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
        .WithSegmentEventHandler(OnNewSegment)
        .WithFileModel("ggml-base.bin")
        .WithTranslate()
        .WithLanguage("auto")
        .Build();

    void OnNewSegment(object sender, OnSegmentEventArgs e)
    {
        Console.WriteLine($"CSSS {e.Start} ==> {e.End} : {e.Segment}");
    }

    using var fileStream = File.OpenRead("yourAudio.wav");
    processor.Process()
```	

## Documentation

You can find the documentation and code samples here: [https://github.com/sandrohanea/whisper.net](https://github.com/sandrohanea/whisper.net)

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
