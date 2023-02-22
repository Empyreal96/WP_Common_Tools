using System;

namespace Microsoft.WindowsPhone.Imaging
{
	[CLSCompliant(false)]
	public enum BlockIoType : uint
	{
		HardDisk,
		RemovableDisk,
		CdRom,
		RamDisk,
		Partition,
		File,
		VirtualHardDisk
	}
}
