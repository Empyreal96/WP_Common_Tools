using System;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary
{
	[Flags]
	internal enum LoggingModeConstant : uint
	{
		PrivateLoggerMode = 0x800u,
		PrivateInProc = 0x20000u
	}
}
