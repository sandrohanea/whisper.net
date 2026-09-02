// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System;
using System.IO;
using Whisper.net;
using Whisper.net.Ggml;

const string modelFileName = "ggml-parakeet-tdt-0.6b-v3-q4_0.bin";

if (!File.Exists(modelFileName))
{
    using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlParakeetModelAsync(
        ParakeetModelType.Tdt0_6B_V3,
        ParakeetQuantizationType.Q4_0);
    using var modelFile = File.Create(modelFileName);
    await modelStream.CopyToAsync(modelFile);
}

using var factory = WhisperFactory.FromPath(modelFileName, new()
{
    ModelFamily = WhisperModelFamily.Parakeet,
});
using var processor = factory.CreateBuilder().Build();
using var audio = File.OpenRead("kennedy.wav");

await foreach (var segment in processor.ProcessAsync(audio))
{
    Console.WriteLine($"{segment.Start}->{segment.End}: {segment.Text}");
}
