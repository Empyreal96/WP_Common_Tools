using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class RamDiskIdentifier : BaseIdentifier, IBlockIoIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public uint Size => Source.Size + 24;

		[CLSCompliant(false)]
		public BlockIoType BlockType => BlockIoType.RamDisk;

		[CLSCompliant(false)]
		public ulong ImageBase { get; set; }

		[CLSCompliant(false)]
		public ulong ImageSize { get; set; }

		[CLSCompliant(false)]
		public uint ImageOffset { get; set; }

		public FileIdentifier Source { get; set; }

		public RamDiskIdentifier(string filePath, BcdElementBootDevice parentDevice)
		{
			Source = new FileIdentifier(filePath, parentDevice);
		}

		public void ReadFromStream(BinaryReader reader)
		{
			reader.ReadUInt32();
			ImageBase = reader.ReadUInt32();
			ImageSize = reader.ReadUInt64();
			ImageOffset = reader.ReadUInt32();
			Source.ReadFromStream(reader);
		}

		[CLSCompliant(false)]
		public void ReplaceParentDeviceIdentifier(IDeviceIdentifier identifier)
		{
			Source.ReplaceParentDeviceIdentifier(identifier);
		}

		public void WriteToStream(BinaryWriter writer)
		{
			writer.Write(3uL);
			writer.Write(0u);
			writer.Write(0uL);
			writer.Write(0u);
			Source.WriteToStream(writer);
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Block IO Type: RamDisk");
			logger.LogInfo(text + "ImageBase:     0x{0:x}", ImageBase);
			logger.LogInfo(text + "ImageSize:     0x{0:x}", ImageSize);
			logger.LogInfo(text + "ImageOffset:   0x{0:x}", ImageOffset);
			logger.LogInfo(text + "File Path:     {0}", Source.Path);
			if (Source.ParentDevice != null)
			{
				Source.ParentDevice.LogInfo(logger, checked(indentLevel + 2));
			}
		}
	}
}
