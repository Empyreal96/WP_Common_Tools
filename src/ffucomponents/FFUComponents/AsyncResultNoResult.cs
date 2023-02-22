using System;
using System.Threading;

namespace FFUComponents
{
	internal class AsyncResultNoResult : IAsyncResult
	{
		private readonly AsyncCallback asyncCallback;

		private readonly object asyncState;

		private const int statePending = 0;

		private const int stateCompletedSynchronously = 1;

		private const int stateCompletedAsynchronously = 2;

		private int completedState;

		private ManualResetEvent asyncWaitHandle;

		private Exception exception;

		public AsyncCallback AsyncCallback => asyncCallback;

		public object AsyncState => asyncState;

		public bool CompletedSynchronously => Thread.VolatileRead(ref completedState) == 1;

		public WaitHandle AsyncWaitHandle
		{
			get
			{
				if (asyncWaitHandle == null)
				{
					bool isCompleted = IsCompleted;
					ManualResetEvent manualResetEvent = new ManualResetEvent(isCompleted);
					if (Interlocked.CompareExchange(ref asyncWaitHandle, manualResetEvent, null) != null)
					{
						manualResetEvent.Close();
					}
					else if (!isCompleted && IsCompleted)
					{
						asyncWaitHandle.Set();
					}
				}
				return asyncWaitHandle;
			}
		}

		public bool IsCompleted => Thread.VolatileRead(ref completedState) != 0;

		public AsyncResultNoResult(AsyncCallback asyncCallback, object state)
		{
			this.asyncCallback = asyncCallback;
			asyncState = state;
		}

		public void SetAsCompleted(Exception exception, bool completedSynchronously)
		{
			this.exception = exception;
			if (Interlocked.Exchange(ref completedState, completedSynchronously ? 1 : 2) != 0)
			{
				throw new InvalidOperationException(Resources.ERROR_RESULT_ALREADY_SET);
			}
			if (asyncWaitHandle != null)
			{
				asyncWaitHandle.Set();
			}
			if (asyncCallback != null)
			{
				asyncCallback(this);
			}
		}

		public void EndInvoke()
		{
			if (!IsCompleted)
			{
				TimeSpan timeout = TimeSpan.FromMinutes(2.0);
				try
				{
					if (!AsyncWaitHandle.WaitOne(timeout, false))
					{
						throw new TimeoutException();
					}
				}
				finally
				{
					AsyncWaitHandle.Close();
					asyncWaitHandle = null;
				}
			}
			if (exception != null)
			{
				throw exception;
			}
		}
	}
}
