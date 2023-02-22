using System;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class PartitionIdentifier : BaseIdentifier, IDeviceIdentifier
	{
		[CLSCompliant(false)]
		public uint Size => 0u;

		[CLSCompliant(false)]
		public uint ElToritoValue { get; set; }

		public Guid GptValue { get; set; }

		[CLSCompliant(false)]
		public uint MbrPartitionNumber { get; set; }

		[CLSCompliant(false)]
		public IBlockIoIdentifier ParentIdentifier { get; set; }

		public void ReadFromStream(BinaryReader reader)
		{
			byte[] array = reader.ReadBytes(16);
			GptValue = new Guid(array);
			ElToritoValue = (uint)((array[3] << 24) | (array[2] << 16) | (array[1] << 8) | array[0]);
			MbrPartitionNumber = (uint)((array[3] << 24) | (array[2] << 16) | (array[1] << 8) | array[0]);
			ParentIdentifier = BlockIoIdentifierFactory.CreateFromStream(reader);
			ParentIdentifier.ReadFromStream(reader);
		}

		public void WriteToStream(BinaryWriter writer)
		{
			throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: This function isn't implemented.");
		}

		[CLSCompliant(false)]
		public void LogInfo(IULogger logger, int indentLevel)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Identifier: Partition");
			if (ParentIdentifier != null)
			{
				if (ParentIdentifier.BlockType == BlockIoType.HardDisk)
				{
					switch ((ParentIdentifier as HardDiskIdentifier).PartitionStyle)
					{
					case PartitionFormat.Gpt:
						logger.LogInfo(text + "GPT Partition Identifier: {{{0}}}", GptValue);
						break;
					case PartitionFormat.Mbr:
						logger.LogInfo(text + "MBR Partition Number: {0}", MbrPartitionNumber);
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
		}
	}
}
