using System;
using System.Collections.Generic;
using System.Threading;

namespace FFUComponents
{
	internal class DisconnectDevice
	{
		private EventWaitHandle cancelEvent;

		private Dictionary<Guid, DisconnectDevice> DiscCollection;

		private Thread removalThread;

		public IFFUDeviceInternal FFUDevice { get; private set; }

		public Guid DeviceUniqueId => FFUDevice.DeviceUniqueID;

		~DisconnectDevice()
		{
			cancelEvent.Set();
		}

		public DisconnectDevice(IFFUDeviceInternal device, Dictionary<Guid, DisconnectDevice> collection)
		{
			FFUDevice = device;
			DiscCollection = collection;
			cancelEvent = new EventWaitHandle(false, EventResetMode.ManualReset);
			removalThread = new Thread(WaitAndRemove);
			removalThread.Start(this);
		}

		public void Cancel()
		{
			cancelEvent.Set();
			Remove();
		}

		public bool WaitForReconnect()
		{
			return cancelEvent.WaitOne(30000, false);
		}

		private static void WaitAndRemove(object obj)
		{
			DisconnectDevice disconnectDevice = obj as DisconnectDevice;
			if (!disconnectDevice.WaitForReconnect())
			{
				disconnectDevice.Remove();
			}
		}

		private void Remove()
		{
			lock (DiscCollection)
			{
				DiscCollection.Remove(DeviceUniqueId);
			}
		}
	}
}
