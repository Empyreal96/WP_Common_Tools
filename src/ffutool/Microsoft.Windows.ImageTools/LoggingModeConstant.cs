using System;

namespace Microsoft.Windows.ImageTools
{
	[Flags]
	internal enum LoggingModeConstant : uint
	{
		PrivateLoggerMode = 0x800u,
		PrivateInProc = 0x20000u
	}
}
