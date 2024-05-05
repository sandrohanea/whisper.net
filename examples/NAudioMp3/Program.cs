// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;
using Whisper.net;
using Whisper.net.Ggml;

public class Program
{
  // This example shows how to use the NAudio library to convert an mp3 file to
  // a wav file with 16Khz sample rate and then use the Whisper library to
  // process the wav file.
  public static async Task Main(string[] args)
  {
    // We declare three variables which we will use later, ggmlType,
    // modelFileName and mp3FileName
    var ggmlType = GgmlType.Base;
    var modelFileName = "ggml-base.bin";
    var mp3FileName = "kennedy.mp3";

    // This section detects whether the "ggml-base.bin" file exists in our
    // project disk. If it doesn't, it downloads it from the internet
    if (!File.Exists(modelFileName))
    {
      await DownloadModel(modelFileName, ggmlType);
    }

    // This section creates the whisperFactory object which is used to create
    // the processor object.
    using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

    // This section creates the processor object which is used to process the
    // audio file, it uses language `auto` to detect the language of the audio
    // file.
    using var processor =
        whisperFactory.CreateBuilder().WithLanguage("auto").Build();

    // This section opens the mp3 file and converts it to a wav file with 16Khz
    // sample rate.
    using var fileStream = File.OpenRead(mp3FileName);

    using var wavStream = new MemoryStream();

    using var reader = new Mp3FileReader(fileStream);
    var resampler =
        new WdlResamplingSampleProvider(reader.ToSampleProvider(), 16000);
    WaveFileWriter.WriteWavFileToStream(wavStream,
                                        resampler.ToWaveProvider16());

    // This section sets the wavStream to the beginning of the stream. (This is
    // required because the wavStream was written to in the previous section)
    wavStream.Seek(0, SeekOrigin.Begin);

    // This section processes the audio file and prints the results (start time,
    // end time and text) to the console.
    await foreach (var result in processor.ProcessAsync(wavStream))
    {
      Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
    }
  }

  private static async Task DownloadModel(string fileName, GgmlType ggmlType)
  {
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream =
        await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
  }
}
