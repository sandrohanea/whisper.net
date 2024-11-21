// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Ggml;
using Whisper.net;
using Whisper.net.Wave;

// This examples shows how to use Whisper.net to create a transcription from an audio file having multiple channels at 16Khz sample rate.
// Each channel is represented by a speaker.
// Based on the time of the segments, the speaker is identified, and the text is assigned to the speaker.

// We declare three variables which we will use later, ggmlType, modelFileName and inputFileName
var ggmlType = GgmlType.Base;
var modelFileName = "ggml-base.bin";
var wavFileName = "multichannel.wav";

// This section detects whether the "ggml-base.bin" file exists in our project disk. If it doesn't, it downloads it from the internet
if (!File.Exists(modelFileName))
{
    await DownloadModel(modelFileName, ggmlType);
}

// This section creates the whisperFactory object which is used to create the processor object.
using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

// This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
using var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")
    .Build();

// This section opens the audio file and converts it to a wav file.
using var fileStream = File.OpenRead(wavFileName);

var waveParser = new WaveParser(fileStream);
await waveParser.InitializeAsync();
var channels = waveParser.Channels;
var sampleRate = waveParser.SampleRate;
var bitsPerSample = waveParser.BitsPerSample;
var headerSize = waveParser.DataChunkPosition;
var frameSize = bitsPerSample / 8 * channels;

var samples = await waveParser.GetAvgSamplesAsync(CancellationToken.None);
// This section processes the audio file and prints the results (start time, end time and text) to the console.
await foreach (var result in processor.ProcessAsync(samples))
{
    // Get the wave position for the specified time interval
    var startSample = (long)result.Start.TotalMilliseconds * sampleRate / 1000;
    var endSample = (long)result.End.TotalMilliseconds * sampleRate / 1000;

    // Calculate buffer size.
    var bufferSize = (int)(endSample - startSample) * frameSize;
    var readBuffer = new byte[bufferSize];

    // Set fileStream position.
    fileStream.Position = headerSize + startSample * frameSize;

    // Read the wave data for the specified time interval, into the readBuffer.
    var read = await fileStream.ReadAsync(readBuffer.AsMemory());

    // Process the readBuffer and convert to shorts.
    var buffer = new short[read / 2];
    for (var i = 0; i < read; i++)
    {
        // Handle endianess manually and convert bytes to Int16.
        buffer[i] = BitConverter.IsLittleEndian
            ? (short)(readBuffer[i * 2] | (readBuffer[i * 2 + 1] << 8))
            : (short)((readBuffer[i * 2] << 8) | readBuffer[i * 2 + 1]);
    }

    // Iterate in the wave data to calculate total energy in each channel, and find the channel with the maximum energy.
    var energy = new double[channels];
    var maxEnergy = 0d;
    var maxEnergyChannel = 0;
    for (var i = 0; i < buffer.Length; i++)
    {
        var channel = i % channels;
        energy[channel] += Math.Pow(buffer[i], 2);

        if (energy[channel] > maxEnergy)
        {
            maxEnergy = energy[channel];
            maxEnergyChannel = channel;
        }
    }

    Console.WriteLine($"{result.Start}->{result.End}: {result.Text}. Max energy in channel: {maxEnergyChannel}");

}

static async Task DownloadModel(string fileName, GgmlType ggmlType)
{
    Console.WriteLine($"Downloading Model {fileName}");
    using var modelStream = await WhisperGgmlDownloader.GetGgmlModelAsync(ggmlType);
    using var fileWriter = File.OpenWrite(fileName);
    await modelStream.CopyToAsync(fileWriter);
}
