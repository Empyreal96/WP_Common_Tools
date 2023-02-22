using System;
using System.IO;

namespace Microsoft.WindowsPhone.Imaging
{
	public static class BlockIoIdentifierFactory
	{
		[CLSCompliant(false)]
		public static readonly uint SizeOnDisk = 40u;

		[CLSCompliant(false)]
		public static IBlockIoIdentifier CreateFromStream(BinaryReader reader)
		{
			IBlockIoIdentifier blockIoIdentifier = null;
			switch ((BlockIoType)reader.ReadUInt32())
			{
			case BlockIoType.HardDisk:
				return new HardDiskIdentifier();
			case BlockIoType.RemovableDisk:
				return new RemovableDiskIdentifier();
			case BlockIoType.CdRom:
				return new CdRomIdentifier();
			case BlockIoType.File:
				return new FileIdentifier("", BcdElementBootDevice.CreateBaseBootDevice());
			case BlockIoType.RamDisk:
				return new RamDiskIdentifier("", BcdElementBootDevice.CreateBaseBootDevice());
			case BlockIoType.VirtualHardDisk:
				return new VirtualDiskIdentifier();
			default:
				throw new ImageStorageException("The block IO type is unrecognized.");
			}
		}
	}
}
