using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverStoreOfflineAddDriverPackageFlags : uint
	{
		None = 0u,
		SkipInstall = 1u,
		Inbox = 2u,
		F6 = 4u,
		SkipExternalFilePresenceCheck = 8u,
		NoTempCopy = 0x10u,
		UseHardLinks = 0x20u,
		InstallOnly = 0x40u,
		ReplacePackage = 0x80u,
		Force = 0x100u,
		BaseVersion = 0x200u
	}
}
