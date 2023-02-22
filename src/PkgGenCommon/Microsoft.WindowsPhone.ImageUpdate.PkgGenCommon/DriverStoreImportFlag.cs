using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	[Flags]
	internal enum DriverStoreImportFlag : uint
	{
		None = 0u,
		SkipTempCopy = 1u,
		SkipExternalFileCheck = 2u,
		NoRestorePoint = 4u,
		NonInteractive = 8u,
		Replace = 0x20u,
		Hardlink = 0x40u,
		PublishSameName = 0x100u,
		Inbox = 0x200u,
		F6 = 0x400u,
		BaseVersion = 0x800u,
		SystemDefaultLocale = 0x1000u,
		SystemCritical = 0x2000u
	}
}
