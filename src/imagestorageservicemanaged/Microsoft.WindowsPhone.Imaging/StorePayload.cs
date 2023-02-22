using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class StorePayload
	{
		public enum BlockPhase
		{
			Phase1,
			Phase2,
			Phase3,
			Invalid
		}

		public ImageStoreHeader StoreHeader { get; set; }

		public List<ValidationEntry> ValidationEntries { get; set; }

		public List<DataBlockEntry> Phase1DataEntries { get; set; }

		public List<DataBlockEntry> Phase2DataEntries { get; set; }

		public List<DataBlockEntry> Phase3DataEntries { get; set; }

		public StorePayload()
		{
			StoreHeader = new ImageStoreHeader();
			Phase1DataEntries = new List<DataBlockEntry>();
			Phase2DataEntries = new List<DataBlockEntry>();
			Phase3DataEntries = new List<DataBlockEntry>();
		}

		public List<DataBlockEntry> GetPhaseEntries(BlockPhase phase)
		{
			List<DataBlockEntry> result = null;
			switch (phase)
			{
			case BlockPhase.Phase1:
				result = Phase1DataEntries;
				break;
			case BlockPhase.Phase2:
				result = Phase2DataEntries;
				break;
			case BlockPhase.Phase3:
				result = Phase3DataEntries;
				break;
			}
			return result;
		}

		private int GetDescriptorSize()
		{
			int num = 0;
			for (BlockPhase blockPhase = BlockPhase.Phase1; blockPhase != BlockPhase.Invalid; blockPhase++)
			{
				foreach (DataBlockEntry phaseEntry in GetPhaseEntries(blockPhase))
				{
					num += phaseEntry.SizeInBytes;
				}
			}
			return num;
		}

		public int GetMetadataSize()
		{
			return StoreHeader.GetStructureSize() + GetDescriptorSize();
		}

		public byte[] GetMetadata(uint alignment)
		{
			MemoryStream memoryStream = new MemoryStream();
			if (StoreHeader.StoreDataSizeInBytes == 0)
			{
				StoreHeader.StoreDataSizeInBytes = (uint)GetDescriptorSize();
			}
			StoreHeader.WriteToStream(memoryStream);
			for (BlockPhase blockPhase = BlockPhase.Phase1; blockPhase != BlockPhase.Invalid; blockPhase++)
			{
				foreach (DataBlockEntry phaseEntry in GetPhaseEntries(blockPhase))
				{
					phaseEntry.WriteEntryToStream(memoryStream);
				}
			}
			long num = memoryStream.Length % (long)alignment;
			if (num != 0L)
			{
				memoryStream.SetLength(memoryStream.Length + alignment - num);
			}
			return memoryStream.ToArray();
		}

		public void ReadMetadataFromStream(Stream sourceStream)
		{
			StoreHeader = ImageStoreHeader.ReadFromStream(sourceStream);
			uint num = StoreHeader.InitialPartitionTableBlockIndex + StoreHeader.InitialPartitionTableBlockCount;
			uint num2 = StoreHeader.FlashOnlyPartitionTableBlockIndex + StoreHeader.FlashOnlyPartitionTableBlockCount;
			uint num3 = StoreHeader.FinalPartitionTableBlockIndex + StoreHeader.FinalPartitionTableBlockCount;
			BinaryReader reader = new BinaryReader(sourceStream);
			uint num4;
			for (num4 = 0u; num4 < num; num4++)
			{
				DataBlockEntry dataBlockEntry = new DataBlockEntry(StoreHeader.BytesPerBlock);
				dataBlockEntry.ReadEntryFromStream(reader, num4);
				Phase1DataEntries.Add(dataBlockEntry);
			}
			for (; num4 < num2; num4++)
			{
				DataBlockEntry dataBlockEntry2 = new DataBlockEntry(StoreHeader.BytesPerBlock);
				dataBlockEntry2.ReadEntryFromStream(reader, num4);
				Phase2DataEntries.Add(dataBlockEntry2);
			}
			for (; num4 < num3; num4++)
			{
				DataBlockEntry dataBlockEntry3 = new DataBlockEntry(StoreHeader.BytesPerBlock);
				dataBlockEntry3.ReadEntryFromStream(reader, num4);
				Phase3DataEntries.Add(dataBlockEntry3);
			}
			reader = null;
		}

		public void LogInfo(IULogger logger, bool logStoreHeader, bool logDataEntries)
		{
			if (logStoreHeader)
			{
				StoreHeader.LogInfo(logger);
			}
			if (!logDataEntries)
			{
				return;
			}
			for (BlockPhase blockPhase = BlockPhase.Phase1; blockPhase != BlockPhase.Invalid; blockPhase++)
			{
				logger.LogInfo("  {0} entries", blockPhase);
				foreach (DataBlockEntry phaseEntry in GetPhaseEntries(blockPhase))
				{
					phaseEntry.LogInfo(logger, false, 4);
				}
				logger.LogInfo("");
			}
		}
	}
}
