using System;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public enum ApplicationTypeValue : uint
	{
		FirmwareBootManager = 1u,
		WindowsBootManager = 2u,
		WindowsBootLoader = 3u,
		WindowsResumeApplication = 4u,
		MemoryTester = 5u,
		LegacyNtLdr = 6u,
		LegacySetupLdr = 7u,
		BootSector = 8u,
		StartupModule = 9u,
		GenericApplication = 10u,
		Reserved = 1048575u
	}
}
