using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class HardDiskIdentifier : BaseIdentifier, IBlockIoIdentifier, IDeviceIdentifier
	{
		private byte[] _rawIdentifier;

		[CLSCompliant(false)]
		public uint Size => 24u;

		[CLSCompliant(false)]
		public BlockIoType BlockType => BlockIoType.HardDisk;

		[CLSCompliant(false)]
		public PartitionFormat PartitionStyle { get; set; }

		[CLSCompliant(false)]
		public uint MbrSignature
		{
			get
			{
				return (uint)((_rawIdentifier[3] << 24) | (_rawIdentifier[2] << 16) | (_rawIdentifier[1] << 8) | _rawIdentifier[0]);
			}
			set
			{
				_rawIdentifier[3] = (byte)((value & 0xFF000000u) >> 24);
				_rawIdentifier[2] = (byte)((value & 0xFF0000) >> 16);
				_rawIdentifier[1] = (byte)((value & 0xFF00) >> 8);
				_rawIdentifier[0] = (byte)(value & 0xFFu);
				for (int i = 4; i < 16; i++)
				{
					_rawIdentifier[i] = 0;
				}
			}
		}

		public Guid GptSignature
		{
			get
			{
				return new Guid(_rawIdentifier);
			}
			set
			{
				_rawIdentifier = value.ToByteArray();
			}
		}

		[CLSCompliant(false)]
		public uint RawDiskNumber
		{
			get
			{
				return (uint)((_rawIdentifier[3] << 24) | (_rawIdentifier[2] << 16) | (_rawIdentifier[1] << 8) | _rawIdentifier[0]);
			}
			set
			{
				_rawIdentifier[3] = (byte)((value & 0xFF000000u) >> 24);
				_rawIdentifier[2] = (byte)((value & 0xFF0000) >> 16);
				_rawIdentifier[1] = (byte)((value & 0xFF00) >> 8);
				_rawIdentifier[0] = (byte)(value & 0xFFu);
				for (int i = 4; i < 16; i++)
				{
					_rawIdentifier[i] = 0;
				}
			}
		}

		internal bool AsVirtualDisk { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			PartitionStyle = (PartitionFormat)reader.ReadUInt32();
			_rawIdentifier = reader.ReadBytes(16);
		}

		public void WriteToStream(BinaryWriter writer)
		{
			writer.Write((uint)BlockType);
			writer.Write((uint)PartitionStyle);
			writer.Write(_rawIdentifier);
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: Hard Disk");
			logger.LogInfo(text + $"Partition Style:   {PartitionStyle}");
			switch (PartitionStyle)
			{
			case PartitionFormat.Gpt:
				logger.LogInfo(text + "GPT Guid:          {{{0}}}", GptSignature);
				break;
			case PartitionFormat.Mbr:
				logger.LogInfo(text + $"MBR Signature:     0x{MbrSignature:x}");
				break;
			case PartitionFormat.Raw:
				logger.LogInfo(text + $"Raw Disk Number:   {RawDiskNumber}");
				break;
			}
		}

		[CLSCompliant(false)]
		public static HardDiskIdentifier CreateSimpleMbr(uint diskSignature)
		{
			return new HardDiskIdentifier
			{
				PartitionStyle = PartitionFormat.Mbr,
				_rawIdentifier = new byte[16],
				MbrSignature = diskSignature
			};
		}

		[CLSCompliant(false)]
		public static HardDiskIdentifier CreateSimpleGpt(Guid diskId)
		{
			return new HardDiskIdentifier
			{
				PartitionStyle = PartitionFormat.Gpt,
				_rawIdentifier = new byte[16],
				GptSignature = diskId
			};
		}
	}
}
