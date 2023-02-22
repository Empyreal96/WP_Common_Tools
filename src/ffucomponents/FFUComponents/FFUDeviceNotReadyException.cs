using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUDeviceNotReadyException : FFUException
	{
		public FFUDeviceNotReadyException()
		{
		}

		public FFUDeviceNotReadyException(string message)
			: base(message)
		{
		}

		public FFUDeviceNotReadyException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FFUDeviceNotReadyException(IFFUDevice device)
			: base(device)
		{
		}

		protected FFUDeviceNotReadyException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
