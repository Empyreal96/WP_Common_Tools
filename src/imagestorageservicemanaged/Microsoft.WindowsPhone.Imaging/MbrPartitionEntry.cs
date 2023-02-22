using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class MbrPartitionEntry
	{
		public const int SizeInBytes = 16;

		public bool Bootable { get; set; }

		public byte PartitionType { get; set; }

		public uint StartingSector { get; set; }

		public uint SectorCount { get; set; }

		public bool TypeIsContainer
		{
			get
			{
				if (PartitionType != 5)
				{
					return PartitionType == 19;
				}
				return true;
			}
		}

		public uint StartingSectorOffset { get; set; }

		public uint AbsoluteStartingSector => StartingSectorOffset + StartingSector;

		public void ReadFromStream(BinaryReader reader)
		{
			Bootable = (reader.ReadByte() & 0x80) != 0;
			reader.ReadBytes(3);
			PartitionType = reader.ReadByte();
			reader.ReadBytes(3);
			StartingSector = reader.ReadUInt32();
			SectorCount = reader.ReadUInt32();
		}

		public void WriteToStream(BinaryWriter writer)
		{
			writer.Write((byte)(Bootable ? 128u : 0u));
			writer.Write((byte)0);
			writer.Write((byte)0);
			writer.Write((byte)0);
			writer.Write(PartitionType);
			writer.Write((byte)0);
			writer.Write((byte)0);
			writer.Write((byte)0);
			writer.Write(StartingSector);
			writer.Write(SectorCount);
		}

		public void ZeroData()
		{
			PartitionType = 0;
			StartingSector = 0u;
			StartingSectorOffset = 0u;
			SectorCount = 0u;
		}

		public void LogInfo(IULogger logger, MasterBootRecord masterBootRecord, ushort indentLevel = 0)
		{
			if (StartingSector != 0 && SectorCount != 0 && PartitionType != 0)
			{
				string text = new StringBuilder().Append(' ', indentLevel).ToString();
				string text2 = "<EBR>";
				if (!TypeIsContainer)
				{
					text2 = masterBootRecord.GetPartitionName(this);
				}
				logger.LogInfo(text + "Partition Name : {0}", text2);
				logger.LogInfo(text + "Partition Type : 0x{0:x}", PartitionType);
				logger.LogInfo(text + "Starting Sector: 0x{0:x}", StartingSector);
				logger.LogInfo(text + "Sector Count   : 0x{0:x}", SectorCount);
				if (masterBootRecord.IsExtendedBootRecord())
				{
					logger.LogInfo(text + "Absolute Starting Sector: 0x{0:x}", AbsoluteStartingSector);
				}
				logger.LogInfo("");
			}
		}
	}
}
