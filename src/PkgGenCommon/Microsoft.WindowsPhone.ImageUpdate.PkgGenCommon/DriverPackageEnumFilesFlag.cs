using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverPackageEnumFilesFlag : uint
	{
		Copy = 1u,
		Delete = 2u,
		Rename = 4u,
		Inf = 0x10u,
		Catalog = 0x20u,
		Binaries = 0x40u,
		CopyInfs = 0x80u,
		IncludeInfs = 0x100u,
		External = 0x1000u,
		UniqueSource = 0x2000u,
		UniqueDestination = 0x4000u
	}
}
