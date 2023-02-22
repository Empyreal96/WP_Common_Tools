using System;

namespace FFUComponents
{
	[Flags]
	internal enum DICSFlags : uint
	{
		Global = 1u,
		ConfigSpecific = 2u,
		ConfigGeneral = 4u
	}
}
