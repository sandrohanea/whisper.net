namespace Whisper.net
{
	public class AsyncAutoResetEvent
	{
		private static readonly Task Completed = Task.FromResult(true);
		private TaskCompletionSource<bool>? waitTcs;
		private bool isSignaled;

		public Task WaitAsync()
		{
			lock (this)
			{
				if (isSignaled)
				{
					isSignaled = false;
					return Completed;
				}
				else
				{
					var tcs = new TaskCompletionSource<bool>();
					waitTcs = tcs;
					return tcs.Task;
				}
			}
		}


		public void Set()
		{
			TaskCompletionSource<bool>? toRelease = null;
			lock (this)
			{
				if (waitTcs != null)
				{
					toRelease = waitTcs;
					waitTcs = null;
				}
				else
				{
					isSignaled = true;
				}
			}

			toRelease?.SetResult(true);
		}
	}
}
