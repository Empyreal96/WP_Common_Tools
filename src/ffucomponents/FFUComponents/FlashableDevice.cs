using System;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[Guid("CBA774B0-D968-4363-898D-D7FCDCFBDDB2")]
	[ClassInterface(ClassInterfaceType.None)]
	[ComSourceInterfaces(typeof(IFlashableDeviceNotify))]
	[ComVisible(true)]
	public class FlashableDevice : IFlashableDevice, IDisposable
	{
		private IFFUDevice theDev;

		public event ProgressHandler Progress;

		public FlashableDevice(IFFUDevice ffuDev)
		{
			theDev = ffuDev;
			theDev.ProgressEvent += theDev_ProgressEvent;
		}

		public void Dispose()
		{
			theDev.ProgressEvent -= theDev_ProgressEvent;
		}

		private void theDev_ProgressEvent(object sender, ProgressEventArgs e)
		{
			if (this.Progress != null)
			{
				this.Progress(e.Position, e.Length);
			}
		}

		public string GetFriendlyName()
		{
			return theDev.DeviceFriendlyName;
		}

		public string GetUniqueIDStr()
		{
			return theDev.DeviceUniqueID.ToString();
		}

		public string GetSerialNumberStr()
		{
			return theDev.SerialNumber.ToString();
		}

		public bool FlashFFU(string filePath)
		{
			bool result = true;
			try
			{
				theDev.FlashFFUFile(filePath);
				return result;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}
