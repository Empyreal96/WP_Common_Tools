using Microsoft.Windows.Flashing.Platform;

namespace FFUComponents
{
	public class NotificationCallback : DeviceNotificationCallback
	{
		public override void Connected(string devicePath)
		{
			FFUManager.OnDeviceConnect(devicePath);
		}

		public override void Disconnected(string devicePath)
		{
			FFUManager.OnDeviceDisconnect(devicePath);
		}
	}
}
