using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class TimeoutHelper
	{
		public static readonly TimeSpan InfiniteTimeSpan = TimeSpan.FromMilliseconds(-1.0);

		private readonly TimeSpan timeout;

		private readonly Stopwatch stopWatch;

		public bool IsExpired
		{
			get
			{
				if (timeout == InfiniteTimeSpan || timeout == TimeSpan.Zero)
				{
					return false;
				}
				return timeout < stopWatch.Elapsed;
			}
		}

		public TimeSpan Remaining
		{
			get
			{
				if (timeout == InfiniteTimeSpan || timeout == TimeSpan.Zero)
				{
					return timeout;
				}
				TimeSpan timeSpan = timeout - stopWatch.Elapsed;
				return (timeSpan == InfiniteTimeSpan) ? TimeSpan.FromMilliseconds(-2.0) : timeSpan;
			}
		}

		public TimeSpan Elapsed => stopWatch.Elapsed;

		public TimeSpan Timeout => timeout;

		public string Status => string.Format(CultureInfo.InvariantCulture, "Waited {0:0.000} of {1:0.000} seconds", new object[2] { Elapsed.TotalSeconds, timeout.TotalSeconds });

		public TimeoutHelper(int timeoutMilliseconds)
			: this(TimeSpan.FromMilliseconds(timeoutMilliseconds))
		{
		}

		public TimeoutHelper(TimeSpan timeout)
		{
			if (timeout < InfiniteTimeSpan)
			{
				throw new ArgumentOutOfRangeException("timeout", timeout, "Timeout is a negative number other than -1 milliseconds, which represents an infinite time-out.");
			}
			if (timeout.TotalMilliseconds > 2147483647.0)
			{
				throw new ArgumentOutOfRangeException("timeout", timeout, "Timeout is greater than Int32.MaxValue.");
			}
			this.timeout = timeout;
			stopWatch = Stopwatch.StartNew();
		}
	}
}
