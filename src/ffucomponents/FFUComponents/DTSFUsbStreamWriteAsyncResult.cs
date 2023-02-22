using System;

namespace FFUComponents
{
	internal class DTSFUsbStreamWriteAsyncResult : AsyncResultNoResult
	{
		public byte[] Buffer { get; set; }

		public int Offset { get; set; }

		public int Count { get; set; }

		public int RetryCount { get; set; }

		public DTSFUsbStreamWriteAsyncResult(AsyncCallback callback, object state)
			: base(callback, state)
		{
		}
	}
}
