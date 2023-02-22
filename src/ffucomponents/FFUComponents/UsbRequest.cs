using System;

namespace FFUComponents
{
	[Flags]
	internal enum UsbRequest
	{
		DeviceToHost = 0x80,
		HostToDevice = 0,
		Standard = 0,
		Class = 0x20,
		Vendor = 0x40,
		Reserved = 0x60,
		ForDevice = 0,
		ForInterface = 1,
		ForEndpoint = 2,
		ForOther = 3
	}
}
