using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class PartitionIdentifierEx : BaseIdentifier, IDeviceIdentifier
	{
		private byte[] _rawData;

		[CLSCompliant(false)]
		public uint Size
		{
			get
			{
				if (ParentIdentifier != null)
				{
					return 16 + Math.Max(ParentIdentifier.Size, BlockIoIdentifierFactory.SizeOnDisk);
				}
				return 16 + BlockIoIdentifierFactory.SizeOnDisk;
			}
		}

		[CLSCompliant(false)]
		public uint ElToritoValue
		{
			get
			{
				return (uint)((_rawData[3] << 24) | (_rawData[2] << 16) | (_rawData[1] << 8) | _rawData[0]);
			}
			set
			{
				_rawData[3] = (byte)((value & 0xFF000000u) >> 24);
				_rawData[2] = (byte)((value & 0xFF0000) >> 16);
				_rawData[1] = (byte)((value & 0xFF00) >> 8);
				_rawData[0] = (byte)(value & 0xFFu);
				for (int i = 4; i < 16; i++)
				{
					_rawData[i] = 0;
				}
			}
		}

		public Guid GptValue
		{
			get
			{
				return new Guid(_rawData);
			}
			set
			{
				_rawData = value.ToByteArray();
			}
		}

		[CLSCompliant(false)]
		public ulong MbrPartitionOffset
		{
			get
			{
				return (ulong)((_rawData[7] << 24) | (_rawData[6] << 16) | (_rawData[5] << 8) | _rawData[4] | (_rawData[3] << 24) | (_rawData[2] << 16) | (_rawData[1] << 8) | _rawData[0]);
			}
			set
			{
				_rawData[7] = (byte)((value & 0xFF000000u) >> 56);
				_rawData[6] = (byte)((value & 0xFF0000) >> 48);
				_rawData[5] = (byte)((value & 0xFF00) >> 40);
				_rawData[4] = (byte)((value & 0xFF) >> 32);
				_rawData[3] = (byte)((value & 0xFF000000u) >> 24);
				_rawData[2] = (byte)((value & 0xFF0000) >> 16);
				_rawData[1] = (byte)((value & 0xFF00) >> 8);
				_rawData[0] = (byte)(value & 0xFF);
				for (int i = 8; i < 16; i++)
				{
					_rawData[i] = 0;
				}
			}
		}

		[CLSCompliant(false)]
		public IBlockIoIdentifier ParentIdentifier { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			_rawData = reader.ReadBytes(16);
			ParentIdentifier = BlockIoIdentifierFactory.CreateFromStream(reader);
			ParentIdentifier.ReadFromStream(reader);
		}

		public void WriteToStream(BinaryWriter writer)
		{
			long position = writer.BaseStream.Position;
			writer.Write(_rawData);
			ParentIdentifier.WriteToStream(writer);
			while (writer.BaseStream.Position < position + Size)
			{
				writer.Write((byte)0);
			}
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: PartitionEx");
			if (ParentIdentifier != null)
			{
				if (ParentIdentifier.BlockType == BlockIoType.HardDisk || ParentIdentifier.BlockType == BlockIoType.VirtualHardDisk)
				{
					HardDiskIdentifier hardDiskIdentifier = ParentIdentifier as HardDiskIdentifier;
					if (hardDiskIdentifier == null)
					{
						hardDiskIdentifier = (ParentIdentifier as VirtualDiskIdentifier).InternalIdentifer;
					}
					switch (hardDiskIdentifier.PartitionStyle)
					{
					case PartitionFormat.Gpt:
						logger.LogInfo(text + "GPT Partition Identifier: {{{0}}}", GptValue);
						break;
					case PartitionFormat.Mbr:
						logger.LogInfo(text + "MBR Partition Offset: 0x{0:x}", MbrPartitionOffset);
						break;
					case PartitionFormat.Raw:
						throw new ImageStorageException("Cannot use a partition identifier on a RAW disk.");
					}
				}
				else if (ParentIdentifier.BlockType == BlockIoType.CdRom)
				{
					logger.LogInfo(text + "El Torito Value: {0}", ElToritoValue);
				}
				else
				{
					logger.LogInfo(text + "Value: Unsure of the partition style.");
				}
			}
			else
			{
				logger.LogInfo(text + "Value: Unsure of the partition style.");
			}
			if (ParentIdentifier != null)
			{
				logger.LogInfo("");
				ParentIdentifier.LogInfo(logger, checked(indentLevel + 2));
			}
		}

		[CLSCompliant(false)]
		public static PartitionIdentifierEx CreateSimpleMbr(ulong partitionOffset, uint diskSignature)
		{
			return new PartitionIdentifierEx
			{
				_rawData = new byte[16],
				MbrPartitionOffset = partitionOffset,
				ParentIdentifier = HardDiskIdentifier.CreateSimpleMbr(diskSignature)
			};
		}

		[CLSCompliant(false)]
		public static PartitionIdentifierEx CreateSimpleGpt(Guid diskId, Guid partitionId)
		{
			return new PartitionIdentifierEx
			{
				_rawData = new byte[16],
				GptValue = partitionId,
				ParentIdentifier = HardDiskIdentifier.CreateSimpleGpt(diskId)
			};
		}
	}
}
