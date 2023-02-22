using System;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public enum ElementDeviceType : uint
	{
		BootDevice = 1u,
		Partition = 2u,
		File = 3u,
		RamDisk = 4u,
		Unknown = 5u,
		QualifiedPartition = 6u,
		LocateDevice = 8u
	}
}
