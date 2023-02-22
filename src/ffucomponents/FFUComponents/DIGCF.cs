using System;

namespace FFUComponents
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
