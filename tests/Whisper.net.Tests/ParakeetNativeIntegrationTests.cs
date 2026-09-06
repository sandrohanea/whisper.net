// Licensed under the MIT license: https://opensource.org/licenses/MIT

using System.Security.Cryptography;
using Xunit;

namespace Whisper.net.Tests;

public class ParakeetNativeIntegrationTests
{
    private const string RealModelSha256 = "aa7fe2f5fb47d863ca23e8b1d490632d63a2599f515268b6d6bd656158dad45e";

    [EnvironmentFact("PARAKEET_SYNTHETIC_TEST_MODEL_PATH")]
    public async Task SyntheticModels_RunBothEnginesInSameProcess()
    {
        var modelPath = Environment.GetEnvironmentVariable("PARAKEET_SYNTHETIC_TEST_MODEL_PATH")!;
        var whisperModelPath = Path.Combine(Path.GetDirectoryName(modelPath)!, "for-tests-ggml-tiny.bin");
        Assert.True(File.Exists(whisperModelPath), $"Whisper synthetic model not found at '{whisperModelPath}'.");

        using (var whisperFactory = WhisperFactory.FromPath(whisperModelPath))
        using (var whisperProcessor = whisperFactory.CreateBuilder().Build())
        {
            await foreach (var _ in whisperProcessor.ProcessAsync(new float[16000]))
            {
            }
        }

        var progress = new List<int>();
        var encoderBeginCount = 0;

        using var factory = WhisperFactory.FromPath(modelPath, new()
        {
            ModelFamily = WhisperModelFamily.Parakeet,
            UseGpu = false,
        });
        using var processor = factory.CreateBuilder()
            .WithProgressHandler(progress.Add)
            .WithEncoderBeginHandler(_ =>
            {
                encoderBeginCount++;
                return true;
            })
            .Build();

        await foreach (var _ in processor.ProcessAsync(new float[16000]))
        {
        }

        Assert.NotEmpty(progress);
        Assert.Equal(1, encoderBeginCount);
    }

    [EnvironmentFact("PARAKEET_TEST_MODEL_PATH")]
    public async Task RealModel_TranscribesKennedyAudio()
    {
        var modelPath = Environment.GetEnvironmentVariable("PARAKEET_TEST_MODEL_PATH")!;
        using (var modelStream = File.OpenRead(modelPath))
        using (var sha256 = SHA256.Create())
        {
            var actualHash = BitConverter.ToString(sha256.ComputeHash(modelStream))
                .Replace("-", string.Empty)
                .ToLowerInvariant();
            Assert.Equal(RealModelSha256, actualHash);
        }

        using var factory = WhisperFactory.FromPath(modelPath, new()
        {
            ModelFamily = WhisperModelFamily.Parakeet,
        });
        using var processor = factory.CreateBuilder().Build();
        using var audio = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");

        var segments = new List<SegmentData>();
        await foreach (var segment in processor.ProcessAsync(audio))
        {
            segments.Add(segment);
        }

        var transcript = string.Concat(segments.Select(segment => segment.Text));
        Assert.Contains("nation should commit", transcript, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class EnvironmentFactAttribute : FactAttribute
    {
        public EnvironmentFactAttribute(string variableName)
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(variableName)))
            {
                Skip = $"Set {variableName} to run this native integration test.";
            }
        }
    }
}
