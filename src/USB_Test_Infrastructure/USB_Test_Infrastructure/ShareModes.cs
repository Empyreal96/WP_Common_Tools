using System;

namespace USB_Test_Infrastructure
{
	[Flags]
	internal enum ShareModes : uint
	{
		Read = 1u,
		Write = 2u,
		Delete = 4u
	}
}
