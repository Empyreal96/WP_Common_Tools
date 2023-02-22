using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUDeviceRetailUnlockException : FFUException
	{
		public int EfiStatus { get; set; }

		public FFUDeviceRetailUnlockException()
		{
		}

		public FFUDeviceRetailUnlockException(string message)
			: base(message)
		{
		}

		public FFUDeviceRetailUnlockException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public FFUDeviceRetailUnlockException(IFFUDevice device, string message, Exception e)
			: base(device, message, e)
		{
		}

		public FFUDeviceRetailUnlockException(IFFUDevice device, int efiStatus)
			: base(device)
		{
			EfiStatus = efiStatus;
		}

		protected FFUDeviceRetailUnlockException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
