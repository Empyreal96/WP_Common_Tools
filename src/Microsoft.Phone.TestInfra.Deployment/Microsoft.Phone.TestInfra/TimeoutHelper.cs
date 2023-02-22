using System;
using System.Diagnostics;

namespace Microsoft.Phone.TestInfra
{
	public class TimeoutHelper
	{
		private TimeSpan timeout;

		private Stopwatch stopWatch;

		public bool HasExpired => stopWatch.Elapsed > timeout;

		public TimeSpan Remaining => timeout - stopWatch.Elapsed;

		public TimeSpan Elapsed => stopWatch.Elapsed;

		public string Status => $"waited {Elapsed.TotalSeconds:0.000} of {timeout.TotalSeconds:0.000} seconds";

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
