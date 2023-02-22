using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverPackageOpenFlag : uint
	{
		VersionOnly = 1u,
		FilesOnly = 2u,
		DefaultLanguage = 4u,
		LocalizableStrings = 8u,
		TargetOSVersion = 0x10u,
		StrictValidation = 0x20u,
		ClassSchemaOnly = 0x40u
	}
}
