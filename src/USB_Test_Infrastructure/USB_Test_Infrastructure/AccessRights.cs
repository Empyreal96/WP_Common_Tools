using System;

namespace USB_Test_Infrastructure
{
	[Flags]
	internal enum AccessRights : uint
	{
		Read = 0x80000000u,
		Write = 0x40000000u,
		Execute = 0x20000000u,
		All = 0x10000000u
	}
}
