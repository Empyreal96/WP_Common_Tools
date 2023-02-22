using System;

namespace USB_Test_Infrastructure
{
	[Flags]
	internal enum DIGCF
	{
		Default = 1,
		Present = 2,
		AllClasses = 4,
		Profile = 8,
		DeviceInterface = 0x10
	}
}
