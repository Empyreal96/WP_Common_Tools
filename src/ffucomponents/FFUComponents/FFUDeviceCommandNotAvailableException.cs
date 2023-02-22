using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUDeviceCommandNotAvailableException : FFUException
	{
		public FFUDeviceCommandNotAvailableException()
		{
		}

		public FFUDeviceCommandNotAvailableException(string message)
			: base(message)
		{
		}

		public FFUDeviceCommandNotAvailableException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FFUDeviceCommandNotAvailableException(IFFUDevice device)
			: base(device)
		{
		}

		protected FFUDeviceCommandNotAvailableException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
