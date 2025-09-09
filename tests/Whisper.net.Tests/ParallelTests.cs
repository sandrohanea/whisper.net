// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Wave;
using Xunit;

namespace Whisper.net.Tests;

[Collection("ParallelTests")]
[CollectionDefinition("ParallelTests", DisableParallelization = true)]
// This collection ensures that tests within it are run sequentially, preventing parallel execution issues with other tests
// This issues were observed in the past as various github actions hosts ran out of memory when running multiple tests in parallel that used the same model file
public class ParallelTests(TinyModelFixture model) : IClassFixture<TinyModelFixture>
{

    [Fact]
    public async Task ProcessAsync_ParallelExecution_WillCompleteEverytime()
    {
        var parallelExecutions = 2;
        var totalExecutions = 10;
        Console.WriteLine($"Parallel executions: {parallelExecutions}, Total executions: {totalExecutions}");

        var segments = new List<List<SegmentData>>();

        using var factory = WhisperFactory.FromPath(model.ModelFile);
        using var fileReader = await TestDataProvider.OpenFileStreamAsync("kennedy.wav");
        var waveParser = new WaveParser(fileReader);
        var samples = await waveParser.GetAvgSamplesAsync();

        Task CreateTask()
        {
            var currentSegments = new List<SegmentData>();
            segments.Add(currentSegments);
            var task = Task.Run(async () =>
            {
                await using var processor = factory.CreateBuilder()
                    .WithLanguage("en")
                    .Build();

                await foreach (var segment in processor.ProcessAsync(samples))
                {
                    Thread.Sleep(100);
                    currentSegments.Add(segment);
                }
            });
            return task;
        }

        var tasks = new List<Task>();
        var startedTasks = 0;
        for (var i = 0; i < parallelExecutions; i++)
        {
            startedTasks++;
            var task = CreateTask();
            tasks.Add(task);

        }

        while (startedTasks < totalExecutions)
        {
            var firstCompleted = await Task.WhenAny(tasks);
            tasks.Remove(firstCompleted);

            var task = CreateTask();
            tasks.Add(task);
            startedTasks++;
        }

        await Task.WhenAll(tasks);

        // Assert
        for (var i = 1; i < parallelExecutions; i++)
        {
            Assert.True(segments[i].Count > 0);
            Assert.True(segments[i].SequenceEqual(segments[0], new SegmentDataComparer()));
        }
    }
}
