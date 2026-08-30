// Licensed under the MIT license: https://opensource.org/licenses/MIT

namespace Whisper.net.Internals;

internal class AsyncAutoResetEvent
{
    private static readonly Task Completed = Task.CompletedTask;
    private readonly object sync = new();
    private readonly Queue<TaskCompletionSource<bool>> waiters = new();
    private bool isSignaled;

    public Task WaitAsync()
    {
        lock (sync)
        {
            if (isSignaled)
            {
                isSignaled = false;
                return Completed;
            }

            var waiter = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            waiters.Enqueue(waiter);
            return waiter.Task;
        }
    }

    public void Set()
    {
        TaskCompletionSource<bool>? toRelease;
        lock (sync)
        {
            toRelease = waiters.Count > 0 ? waiters.Dequeue() : null;

            if (toRelease == null)
            {
                isSignaled = true;
            }
        }

        toRelease?.SetResult(true);
    }
}
