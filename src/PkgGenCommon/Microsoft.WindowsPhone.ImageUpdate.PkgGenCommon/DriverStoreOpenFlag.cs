using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverStoreOpenFlag : uint
	{
		None = 0u,
		Create = 1u,
		Exclusive = 2u
	}
}
