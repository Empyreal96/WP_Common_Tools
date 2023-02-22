using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverStoreConfigureFlags : uint
	{
		None = 0u,
		Force = 1u,
		ActiveOnly = 2u,
		SourceConfigurations = 0x10000u,
		SourceDeviceIds = 0x20000u,
		TargetDeviceNodes = 0x100000u
	}
}
