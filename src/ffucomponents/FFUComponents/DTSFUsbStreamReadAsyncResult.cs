using System;

namespace FFUComponents
{
	internal class DTSFUsbStreamReadAsyncResult : AsyncResult<int>
	{
		public byte[] Buffer { get; set; }

		public int Offset { get; set; }

		public int Count { get; set; }

		public int RetryCount { get; set; }

		public DTSFUsbStreamReadAsyncResult(AsyncCallback callback, object state)
			: base(callback, state)
		{
		}
	}
}
