using System;
using System.Threading;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class WaitHandleEx
	{
		public static void Acquire(this WaitHandle handle, TimeoutHelper timeoutHelper)
		{
			WaitHandleHelper.Acquire(handle, timeoutHelper);
		}

		public static void Acquire(this WaitHandle handle, TimeSpan timeout)
		{
			WaitHandleHelper.Acquire(handle, timeout);
		}

		public static bool TryToAcquire(this WaitHandle handle)
		{
			return WaitHandleHelper.TryToAcquire(handle);
		}
	}
}
