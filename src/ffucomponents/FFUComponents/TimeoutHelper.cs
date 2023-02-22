using System;
using System.Diagnostics;

namespace FFUComponents
{
	internal class TimeoutHelper
	{
		private TimeSpan timeout;

		private Stopwatch stopWatch;

		public bool HasExpired => stopWatch.Elapsed > timeout;

		public TimeSpan Remaining => timeout - stopWatch.Elapsed;

		public TimeSpan Elapsed => stopWatch.Elapsed;

		public TimeoutHelper(int timeoutMilliseconds)
			: this(TimeSpan.FromMilliseconds(timeoutMilliseconds))
		{
		}

		public TimeoutHelper(TimeSpan timeout)
		{
			this.timeout = timeout;
			stopWatch = Stopwatch.StartNew();
		}
	}
}
