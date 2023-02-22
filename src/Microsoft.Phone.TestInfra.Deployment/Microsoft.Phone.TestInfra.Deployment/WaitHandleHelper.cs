using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class WaitHandleHelper
	{
		public static void Acquire(WaitHandle handle, TimeoutHelper timeoutHelper)
		{
			if (handle == null)
			{
				throw new ArgumentNullException("handle");
			}
			if (timeoutHelper == null)
			{
				throw new ArgumentNullException("timeoutHelper");
			}
			TimeSpan remaining = timeoutHelper.Remaining;
			if (timeoutHelper.IsExpired)
			{
				throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "Timeout expired: {0}", new object[1] { timeoutHelper.Timeout }));
			}
			Acquire(handle, remaining);
		}

		public static void Acquire(WaitHandle handle, TimeSpan timeout)
		{
			if (handle == null)
			{
				throw new ArgumentNullException("handle");
			}
			try
			{
				if (!handle.WaitOne(timeout))
				{
					throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "Unable to acquire mutex within the {0}", new object[1] { timeout }));
				}
			}
			catch (AbandonedMutexException)
			{
			}
		}

		public static bool TryToAcquire(WaitHandle handle)
		{
			try
			{
				Acquire(handle, TimeSpan.Zero);
			}
			catch (TimeoutException)
			{
				return false;
			}
			return true;
		}

		public static void AcquireAll(IEnumerable<WaitHandle> handles, TimeSpan timeout)
		{
			AcquireAll(handles, new TimeoutHelper(timeout));
		}

		public static void AcquireAll(IEnumerable<WaitHandle> handles, TimeoutHelper timeoutHelper)
		{
			if (handles == null || !handles.Any())
			{
				throw new ArgumentNullException("handles");
			}
			if (timeoutHelper == null)
			{
				throw new ArgumentNullException("timeoutHelper");
			}
			List<WaitHandle> list = new List<WaitHandle>();
			List<WaitHandle> list2 = new List<WaitHandle>(handles);
			for (int i = 0; i < list2.Count; i++)
			{
				for (int j = i + 1; j < list2.Count; j++)
				{
					if (list2[i] == list2[j])
					{
						throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Handles list contains equal handles"));
					}
				}
			}
			try
			{
				while (list2.Any())
				{
					for (int k = 0; k < list2.Count; k++)
					{
						if (TryToAcquire(list2[k]))
						{
							list.Add(list2[k]);
							list2.RemoveAt(k);
							k--;
						}
					}
					if (timeoutHelper.IsExpired)
					{
						throw new TimeoutException(string.Format(CultureInfo.InvariantCulture, "Unable to acquire all handles within {0}", new object[1] { timeoutHelper.Timeout }));
					}
				}
			}
			catch
			{
				foreach (Mutex item in list)
				{
					try
					{
						item.ReleaseMutex();
					}
					catch (ApplicationException)
					{
					}
				}
				throw;
			}
		}
	}
}
