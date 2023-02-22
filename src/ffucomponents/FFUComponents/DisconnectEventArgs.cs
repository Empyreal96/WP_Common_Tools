using System;

namespace FFUComponents
{
	public class DisconnectEventArgs : EventArgs
	{
		private Guid deviceUniqueId;

		public Guid DeviceUniqueId => deviceUniqueId;

		private DisconnectEventArgs()
		{
		}

		public DisconnectEventArgs(Guid deviceUniqueId)
		{
			this.deviceUniqueId = deviceUniqueId;
		}
	}
}
