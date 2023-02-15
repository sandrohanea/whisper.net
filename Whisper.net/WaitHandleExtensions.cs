using System;
using System.Collections.Generic;
using System.Text;

namespace Whisper.net
{
	internal static class WaitHandleExtensions
	{
		public static ValueTask AsValueTask(this WaitHandle handle, TimeSpan timeout, CancellationToken token)
		{
			// Handle synchronous cases.
			var alreadySignalled = handle.WaitOne(0);
			if (alreadySignalled || timeout == TimeSpan.Zero)
				return new ValueTask();

			token.ThrowIfCancellationRequested();

			return new ValueTask(HandleAsync(handle, timeout, token));
		}

		private static async Task<bool> HandleAsync(WaitHandle handle, TimeSpan timeout, CancellationToken token)
		{
			var tcs = new TaskCompletionSource<bool>();
			using (new ThreadPoolRegistration(handle, timeout, tcs))
			using (token.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs, useSynchronizationContext: false))
				return await tcs.Task.ConfigureAwait(false);
		}

		private sealed class ThreadPoolRegistration : IDisposable
		{
			private readonly RegisteredWaitHandle registeredWaitHandle;

			public ThreadPoolRegistration(WaitHandle handle, TimeSpan timeout, TaskCompletionSource<bool> tcs)
			{
				registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
					(state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), tcs,
					timeout, executeOnlyOnce: true);
			}

			void IDisposable.Dispose() => registeredWaitHandle.Unregister(null);
		}
	}
}
