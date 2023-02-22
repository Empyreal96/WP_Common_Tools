using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[Guid("71A8CA8E-ED31-4C25-8CFF-689C40E6946E")]
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	public class FlashingManager : IFlashingManager
	{
		public bool Start()
		{
			FFUManager.Start();
			return true;
		}

		public bool Stop()
		{
			FFUManager.Stop();
			return true;
		}

		public bool GetFlashableDevices(ref IEnumerator result)
		{
			ICollection<IFFUDevice> devices = new List<IFFUDevice>();
			FFUManager.GetFlashableDevices(ref devices);
			if (devices.Count == 0)
			{
				devices = null;
				return false;
			}
			result = new FlashableDeviceCollection(devices);
			return true;
		}

		public IFlashableDevice GetFlashableDevice(string instancePath, bool enableFallback)
		{
			IFFUDevice flashableDevice = FFUManager.GetFlashableDevice(instancePath, enableFallback);
			if (flashableDevice == null)
			{
				return null;
			}
			return new FlashableDevice(flashableDevice);
		}
	}
}
