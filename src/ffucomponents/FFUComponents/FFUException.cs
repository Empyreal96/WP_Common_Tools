using System;
using System.Runtime.Serialization;

namespace FFUComponents
{
	[Serializable]
	public class FFUException : Exception
	{
		public string DeviceFriendlyName { get; private set; }

		public Guid DeviceUniqueID { get; private set; }

		public FFUException()
		{
		}

		public FFUException(string message)
			: base(message)
		{
		}

		public FFUException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected FFUException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			DeviceFriendlyName = (string)info.GetValue("DeviceFriendlyName", typeof(string));
			DeviceUniqueID = (Guid)info.GetValue("DeviceUniqueID", typeof(Guid));
		}

		protected new virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("DeviceFriendlyName", DeviceFriendlyName);
			info.AddValue("DeviceUniqueID", DeviceUniqueID);
		}

		public FFUException(string deviceName, Guid deviceId, string message)
			: base(message)
		{
			DeviceFriendlyName = deviceName;
			DeviceUniqueID = deviceId;
		}

		public FFUException(IFFUDevice device)
		{
			if (device != null)
			{
				DeviceFriendlyName = device.DeviceFriendlyName;
				DeviceUniqueID = device.DeviceUniqueID;
			}
		}

		public FFUException(IFFUDevice device, string message, Exception e)
			: base(message, e)
		{
			if (device != null)
			{
				DeviceFriendlyName = device.DeviceFriendlyName;
				DeviceUniqueID = device.DeviceUniqueID;
			}
		}
	}
}
