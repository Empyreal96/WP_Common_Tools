using System;
using System.Diagnostics;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class LogEventArgs : EventArgs
	{
		public DateTime TimeStamp { get; private set; }

		public string Message { get; private set; }

		public TraceLevel TraceLevel { get; private set; }

		public LogEventArgs(string message, TraceLevel traceLevel)
		{
			Message = message;
			TimeStamp = DateTime.Now;
			TraceLevel = traceLevel;
		}
	}
}
