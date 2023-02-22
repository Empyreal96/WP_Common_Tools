using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverStoreReflectFlag : uint
	{
		None = 0u,
		FilesOnly = 1u,
		ActiveDrivers = 2u,
		ExternalOnly = 4u,
		Configurations = 8u
	}
}
