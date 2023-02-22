using System;

namespace FFUComponents
{
	internal class AsyncResult<TResult> : AsyncResultNoResult
	{
		private TResult result;

		public AsyncResult(AsyncCallback asyncCallback, object state)
			: base(asyncCallback, state)
		{
		}

		public void SetAsCompleted(TResult result, bool completedSynchronously)
		{
			this.result = result;
			SetAsCompleted(null, completedSynchronously);
		}

		public new TResult EndInvoke()
		{
			base.EndInvoke();
			return result;
		}
	}
}
