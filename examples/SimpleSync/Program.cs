// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

public class Program {
  // This examples shows how to use Whisper.net to create a transcription from
  // an audio file with 16Khz sample rate, using the segment event handler and
  // synchronous processing.
  public static async Task Main(string[] args) {
    // We declare three variables which we will use later, ggmlType,
    // modelFileName and inputFileName
    var ggmlType = GgmlType.Base;
    var modelFileName = "ggml-base.bin";
    var wavFileName = "kennedy.wav";

    // This section detects whether the "ggml-base.bin" file exists in our
    // project disk. If it doesn't, it downloads it from the internet
    if (!File.Exists(modelFileName)) {
      await DownloadModel(modelFileName, ggmlType);
    }

    // Optional logging from the native library
    LogProvider.Instance.OnLog +=
        (level, message) => { Console.WriteLine($"{level}: {message}"); };

    // This section creates the whisperFactory object which is used to create
    // the processor object.
    using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

    // This section creates the processor object which is used to process the
    // audio file, it uses language `auto` to detect the language of the audio
    // file. It also sets the segment event handler, which is called every time
    // a new segment is detected.
    using var processor =
        whisperFactory.CreateBuilder()
            .WithLanguage("auto")
            .WithSegmentEventHandler((segment) => {
              // Do whetever you want with your segment here.
              Console.WriteLine(
                  $"{segment.Start}->{segment.End}: {segment.Text}");
            })
            .Build();

    // This section processes the audio file and prints the results (start time,
    // end time and text) to the console.
    using var fileStream = File.OpenRead(wavFileName);
    processor.Process(fileStream);
  }

  private static async Task DownloadModel(string fileName, GgmlType ggmlType) {
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream =
        await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
  }
}
