using System;

namespace FFUComponents
{
	internal interface IFFUDeviceInternal : IFFUDevice, IDisposable
	{
		string UsbDevicePath { get; }

		bool NeedsTimer();
	}
}
