using System;
using System.Collections.Generic;

namespace FFUComponents
{
	internal class DisconnectTimer
	{
		private Dictionary<Guid, DisconnectDevice> devices;

		public DisconnectTimer()
		{
			devices = new Dictionary<Guid, DisconnectDevice>();
		}

		public void StopAllTimers()
		{
			DisconnectDevice[] array = new DisconnectDevice[devices.Values.Count];
			devices.Values.CopyTo(array, 0);
			DisconnectDevice[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Cancel();
			}
		}

		public void StartTimer(IFFUDeviceInternal device)
		{
			lock (devices)
			{
				DisconnectDevice value;
				if (devices.TryGetValue(device.DeviceUniqueID, out value))
				{
					throw new FFUException(device.DeviceFriendlyName, device.DeviceUniqueID, Resources.ERROR_MULTIPE_DISCONNECT_NOTIFICATIONS);
				}
				devices[device.DeviceUniqueID] = new DisconnectDevice(device, devices);
			}
		}

		public IFFUDeviceInternal StopTimer(IFFUDeviceInternal device)
		{
			IFFUDeviceInternal result = null;
			lock (devices)
			{
				DisconnectDevice value;
				if (devices.TryGetValue(device.DeviceUniqueID, out value))
				{
					value.Cancel();
					return value.FFUDevice;
				}
				return result;
			}
		}
	}
}
