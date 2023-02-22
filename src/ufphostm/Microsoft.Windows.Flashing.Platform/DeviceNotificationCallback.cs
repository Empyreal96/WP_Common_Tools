using System.Runtime.InteropServices;

namespace Microsoft.Windows.Flashing.Platform
{
	public abstract class DeviceNotificationCallback
	{
		public abstract void Connected([In] string DevicePath);

		public abstract void Disconnected([In] string DevicePath);

		public DeviceNotificationCallback()
		{
		}
	}
}
