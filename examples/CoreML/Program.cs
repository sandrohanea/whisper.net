// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

public class Program {
  // This examples shows how to use Whisper.net to create a transcription from
  // an audio file with 16Khz sample rate, using CoreML
  public static async Task Main(string[] args) {
    // We declare for variables which we will use later, ggmlType,
    // modelFileName, wavFileName and coreMlModelcName
    var ggmlType = GgmlType.Base;
    var modelFileName = "ggml-base.bin";
    var wavFileName = "kennedy.wav";
    var coreMlModelcName = "ggml-base-encoder.mlmodelc";

    // This section detects whether the "ggml-base.bin" file exists in our
    // project disk. If it doesn't, it downloads it from the internet
    if (!File.Exists(modelFileName)) {
      await DownloadModel(modelFileName, ggmlType);
    }

    // This sections detects whether the modelc directory (used by CoreML) is in
    // out project disk. If it doesn't, it downloads it and extract it to the
    // current folder.
    if (!Directory.Exists(coreMlModelcName)) {
      // Note: The modelc directory needs to be extracted at the same level as
      // the "ggml-base.bin" file (and the current executable).
      await WhisperGgmlDownloader.GetEncoderCoreMLModelAsync(ggmlType)
          .ExtractToPath(".");
    }

    // Optional logging from the native library
    LogProvider.Instance.OnLog +=
        (level, message) => { Console.Write($"{level}: {message}"); };

    // This section creates the whisperFactory object which is used to create
    // the processor object.
    using var whisperFactory = WhisperFactory.FromPath(modelFileName);

    // This section creates the processor object which is used to process the
    // audio file, it uses language `auto` to detect the language of the audio
    // file.
    using var processor =
        whisperFactory.CreateBuilder().WithLanguage("auto").Build();

    using var fileStream = File.OpenRead(wavFileName);

    // This section processes the audio file and prints the results (start time,
    // end time and text) to the console.
    await foreach (var result in processor.ProcessAsync(fileStream)) {
      Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
    }
  }

  private static async Task DownloadModel(string fileName, GgmlType ggmlType) {
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream =
        await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
  }
}
