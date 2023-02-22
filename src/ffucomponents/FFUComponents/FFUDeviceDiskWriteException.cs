using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUDeviceDiskWriteException : FFUException
	{
		public FFUDeviceDiskWriteException()
		{
		}

		public FFUDeviceDiskWriteException(string message)
			: base(message)
		{
		}

		public FFUDeviceDiskWriteException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FFUDeviceDiskWriteException(IFFUDevice device, string message, Exception e)
			: base(device, message, e)
		{
		}

		protected FFUDeviceDiskWriteException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
