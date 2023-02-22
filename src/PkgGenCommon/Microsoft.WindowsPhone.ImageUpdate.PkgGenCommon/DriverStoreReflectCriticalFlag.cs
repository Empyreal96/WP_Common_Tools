using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverStoreReflectCriticalFlag : uint
	{
		None = 0u,
		Force = 1u,
		Configurations = 2u
	}
}
