// Licensed under the MIT license: https://opensource.org/licenses/MIT

using Whisper.net.Internals;
using Xunit;

namespace Whisper.net.Tests;

public class AsyncAutoResetEventTests
{
    [Fact]
    public async Task Set_BeforeWait_CompletesOneWait()
    {
        var resetEvent = new AsyncAutoResetEvent();

        resetEvent.Set();

        await resetEvent.WaitAsync();
        Assert.False(resetEvent.WaitAsync().IsCompleted);
    }

    [Fact]
    public async Task Set_AfterWait_CompletesWait()
    {
        var resetEvent = new AsyncAutoResetEvent();
        var waitTask = resetEvent.WaitAsync();

        resetEvent.Set();

        await waitTask;
    }

    [Fact]
    public async Task Set_WithMultipleWaiters_CompletesOneWaitAtATime()
    {
        var resetEvent = new AsyncAutoResetEvent();
        var firstWaitTask = resetEvent.WaitAsync();
        var secondWaitTask = resetEvent.WaitAsync();

        resetEvent.Set();

        await firstWaitTask;
        Assert.False(secondWaitTask.IsCompleted);

        resetEvent.Set();
        await secondWaitTask;
    }
}
