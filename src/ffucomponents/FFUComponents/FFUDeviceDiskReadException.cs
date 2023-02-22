using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUDeviceDiskReadException : FFUException
	{
		public FFUDeviceDiskReadException()
		{
		}

		public FFUDeviceDiskReadException(string message)
			: base(message)
		{
		}

		public FFUDeviceDiskReadException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FFUDeviceDiskReadException(IFFUDevice device, string message, Exception e)
			: base(device, message, e)
		{
		}

		protected FFUDeviceDiskReadException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
