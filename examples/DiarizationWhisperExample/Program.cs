// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Ggml;
using Whisper.net;

var ggmlType = GgmlType.Base;
var modelFileName = "ggml-base.bin";
var wavFileName = "multichannel.wav";

if (!File.Exists(modelFileName))
{
    await DownloadModel(modelFileName, ggmlType);
}

using var whisperFactory = WhisperFactory.FromPath("ggml-base.bin");

using var processor = whisperFactory.CreateBuilder()
    .WithLanguage("auto")
    .Build();

using var fileStream = File.OpenRead(wavFileName);

//TODO: Retrieve this directly from a wave parser when using a newer version where they are exposed.
var channels = 3;
var sampleRate = 16000;
var bitsPerSample = 16;
var headerSize = 44;
var frameSize = bitsPerSample / 8 * channels;

await foreach (var result in processor.ProcessAsync(fileStream))
{
    // 1. Get the wave position for the specified time interval
    var startSample = (long)result.Start.TotalMilliseconds * sampleRate / 1000;
    var endSample = (long)result.End.TotalMilliseconds * sampleRate / 1000;

    // Calculate buffer size.
    var bufferSize = (int)(endSample - startSample) * frameSize;
    var readBuffer = new byte[bufferSize];

    // Set fileStream position.
    fileStream.Position = headerSize + startSample * frameSize;

    // Read the wave data for the specified time interval
    await fileStream.ReadAsync(readBuffer.AsMemory());

    // Process the readBuffer and convert to shorts.
    var buffer = new short[bufferSize / 2];
    for (var i = 0; i < buffer.Length; i++)
    {
        // Handle endianess manually and convert bytes to Int16.
        buffer[i] = BitConverter.IsLittleEndian
            ? (short)(readBuffer[i * 2] | (readBuffer[i * 2 + 1] << 8))
            : (short)((readBuffer[i * 2] << 8) | readBuffer[i * 2 + 1]);
    }

    // 3. Iterate in the wave data to calculate total energy in each channel
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
