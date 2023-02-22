using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class MbrDataEntryGenerator : IEntryGenerator
	{
		private ISourceAllocation _allocation;

		private StorePayload _payload;

		private SafeFileHandle _sourceHandle;

		private MasterBootRecord _table;

		private FullFlashUpdateImage _image;

		private uint _sectorSize;

		private List<DataBlockEntry> _phase2PartitionTableEntries;

		private List<FullFlashUpdateImage.FullFlashUpdatePartition> _finalPartitions;

		private IULogger _logger;

		public MbrDataEntryGenerator(IULogger logger, StorePayload storePayload, ISourceAllocation sourceAllocation, SafeFileHandle sourceHandle, uint sourceSectorSize, FullFlashUpdateImage image)
		{
			_payload = storePayload;
			_allocation = sourceAllocation;
			_sourceHandle = sourceHandle;
			_image = image;
			_sectorSize = sourceSectorSize;
			_table = new MasterBootRecord(logger, (int)sourceSectorSize);
			_finalPartitions = new List<FullFlashUpdateImage.FullFlashUpdatePartition>();
			_logger = logger;
			if (storePayload.StoreHeader.BytesPerBlock < image.Stores[0].SectorSize)
			{
				throw new ImageStorageException("The data block size is less than the device sector size.");
			}
			if (storePayload.StoreHeader.BytesPerBlock % image.Stores[0].SectorSize != 0)
			{
				throw new ImageStorageException("The data block size is not a multiple of the device sector size.");
			}
			if (storePayload.StoreHeader.BytesPerBlock > sourceAllocation.GetAllocationSize())
			{
				throw new ImageStorageException("The payload block size is larger than the allocation size of the temp store.");
			}
			if (sourceAllocation.GetAllocationSize() % storePayload.StoreHeader.BytesPerBlock != 0)
			{
				throw new ImageStorageException("The allocation size of the temp store is not a multiple of the payload block size.");
			}
		}

		public void GenerateEntries(bool onlyAllocateDefinedGptEntries)
		{
			_payload.Phase1DataEntries = GeneratePhase1Entries();
			_payload.Phase2DataEntries = GeneratePhase2Entries();
			_payload.Phase3DataEntries = GeneratePhase3Entries();
		}

		private List<DataBlockEntry> GeneratePhase1Entries()
		{
			List<DataBlockEntry> list = new List<DataBlockEntry>((int)(131072u / _payload.StoreHeader.BytesPerBlock));
			for (int i = 0; i < list.Capacity; i++)
			{
				DataBlockEntry dataBlockEntry = new DataBlockEntry(_payload.StoreHeader.BytesPerBlock);
				dataBlockEntry.DataSource.Source = DataBlockSource.DataSource.Zero;
				dataBlockEntry.BlockLocationsOnDisk.Add(new DiskLocation((uint)i, DiskLocation.DiskAccessMethod.DiskBegin));
				list.Add(dataBlockEntry);
			}
			_payload.StoreHeader.InitialPartitionTableBlockCount = (uint)list.Count;
			return list;
		}

		private List<DataBlockEntry> GeneratePhase2Entries()
		{
			DiskStreamSource streamSource = new DiskStreamSource(_sourceHandle, _payload.StoreHeader.BytesPerBlock);
			List<DataBlockEntry> list = new List<DataBlockEntry>();
			List<string> list2 = new List<string>();
			using (DataBlockStream dataBlockStream = new DataBlockStream(streamSource, _payload.StoreHeader.BytesPerBlock))
			{
				_table.ReadFromStream(dataBlockStream, MasterBootRecord.MbrParseType.Normal);
				for (int i = 0; i < _image.Stores[0].PartitionCount; i++)
				{
					FullFlashUpdateImage.FullFlashUpdatePartition fullFlashUpdatePartition = _image.Stores[0].Partitions[i];
					if (fullFlashUpdatePartition.RequiredToFlash)
					{
						list2.Add(fullFlashUpdatePartition.Name);
						continue;
					}
					_finalPartitions.Add(fullFlashUpdatePartition);
					_table.RemovePartition(fullFlashUpdatePartition.Name);
				}
				_table.DiskSignature = ImageConstants.SYSTEM_STORE_SIGNATURE;
				_table.WriteToStream(dataBlockStream, false);
				_phase2PartitionTableEntries = dataBlockStream.BlockEntries;
			}
			foreach (string item in list2)
			{
				MbrPartitionEntry mbrPartitionEntry = _table.FindPartitionByName(item);
				if (mbrPartitionEntry.AbsoluteStartingSector != 0 && mbrPartitionEntry.StartingSector != 0)
				{
					ulong byteCount = (ulong)mbrPartitionEntry.SectorCount * (ulong)_sectorSize;
					List<DataBlockEntry> list3 = GenerateDataEntriesFromDisk((long)mbrPartitionEntry.AbsoluteStartingSector * (long)_sectorSize, (long)byteCount);
					int count = list3.Count;
					FilterUnAllocatedDataEntries(list3);
					_logger.LogInfo("Recording {0} of {1} blocks from partiton {2} ({3} bytes)", list3.Count, count, _table.GetPartitionName(mbrPartitionEntry), list3.Count * _payload.StoreHeader.BytesPerBlock);
					list.AddRange(list3);
				}
			}
			FilterPartitionTablesFromDataBlocks(list);
			_payload.StoreHeader.FlashOnlyPartitionTableBlockCount = (uint)_phase2PartitionTableEntries.Count;
			_payload.StoreHeader.FlashOnlyPartitionTableBlockIndex = _payload.StoreHeader.InitialPartitionTableBlockCount + (uint)list.Count;
			list.AddRange(_phase2PartitionTableEntries);
			return list;
		}

		private List<DataBlockEntry> GeneratePhase3Entries()
		{
			List<DataBlockEntry> list = new List<DataBlockEntry>();
			using (DataBlockStream dataBlockStream = new DataBlockStream(new DiskStreamSource(_sourceHandle, _payload.StoreHeader.BytesPerBlock), _payload.StoreHeader.BytesPerBlock))
			{
				_table = new MasterBootRecord(_logger, (int)_sectorSize);
				_table.ReadFromStream(dataBlockStream, MasterBootRecord.MbrParseType.Normal);
				_table.DiskSignature = ImageConstants.SYSTEM_STORE_SIGNATURE;
				_table.WriteToStream(dataBlockStream, false);
				foreach (FullFlashUpdateImage.FullFlashUpdatePartition finalPartition in _finalPartitions)
				{
					MbrPartitionEntry mbrPartitionEntry = _table.FindPartitionByName(finalPartition.Name);
					if (mbrPartitionEntry.AbsoluteStartingSector != 0 && mbrPartitionEntry.StartingSector != 0)
					{
						ulong byteCount = (ulong)mbrPartitionEntry.SectorCount * (ulong)_sectorSize;
						List<DataBlockEntry> list2 = GenerateDataEntriesFromDisk((long)mbrPartitionEntry.AbsoluteStartingSector * (long)_sectorSize, (long)byteCount);
						int count = list2.Count;
						FilterUnAllocatedDataEntries(list2);
						_logger.LogInfo("Recording {0} of {1} blocks from partiton {2} ({3} bytes)", list2.Count, count, finalPartition.Name, list2.Count * _payload.StoreHeader.BytesPerBlock);
						list.AddRange(list2);
					}
				}
				FilterPartitionTablesFromDataBlocks(list);
				_payload.StoreHeader.FinalPartitionTableBlockCount = (uint)dataBlockStream.BlockEntries.Count;
				_payload.StoreHeader.FinalPartitionTableBlockIndex = _payload.StoreHeader.FlashOnlyPartitionTableBlockIndex + _payload.StoreHeader.FlashOnlyPartitionTableBlockCount + (uint)list.Count;
				list.AddRange(dataBlockStream.BlockEntries);
				return list;
			}
		}

		private void FilterPartitionTablesFromDataBlocks(List<DataBlockEntry> dataBlocks)
		{
			int bytesPerBlock = (int)_payload.StoreHeader.BytesPerBlock;
			foreach (DataBlockEntry dataBlock in dataBlocks)
			{
				foreach (DataBlockEntry phase2PartitionTableEntry in _phase2PartitionTableEntries)
				{
					if (dataBlock.BlockLocationsOnDisk[0].BlockIndex == phase2PartitionTableEntry.BlockLocationsOnDisk[0].BlockIndex)
					{
						dataBlock.DataSource = new DataBlockSource();
						dataBlock.DataSource.Source = phase2PartitionTableEntry.DataSource.Source;
						dataBlock.DataSource.SetMemoryData(phase2PartitionTableEntry.DataSource.GetMemoryData(), 0, bytesPerBlock);
						break;
					}
				}
			}
		}

		private void FilterUnAllocatedDataEntries(List<DataBlockEntry> dataBlocks)
		{
			uint bytesPerBlock = _payload.StoreHeader.BytesPerBlock;
			for (int i = 0; i < dataBlocks.Count; i++)
			{
				DataBlockEntry dataBlockEntry = dataBlocks[i];
				if (dataBlockEntry.DataSource.Source == DataBlockSource.DataSource.Disk && !_allocation.BlockIsAllocated(dataBlockEntry.DataSource.StorageOffset))
				{
					dataBlocks.RemoveAt(i--);
				}
			}
		}

		private List<DataBlockEntry> GenerateDataEntriesFromDisk(long diskOffset, long byteCount)
		{
			uint bytesPerBlock = _payload.StoreHeader.BytesPerBlock;
			List<DataBlockEntry> list = new List<DataBlockEntry>();
			uint num = (uint)(byteCount / (long)bytesPerBlock);
			if (byteCount % (long)bytesPerBlock != 0L)
			{
				num++;
			}
			uint num2 = (uint)(diskOffset / (long)bytesPerBlock);
			for (uint num3 = 0u; num3 < num; num3++)
			{
				DataBlockEntry dataBlockEntry = new DataBlockEntry(bytesPerBlock);
				dataBlockEntry.BlockLocationsOnDisk.Add(new DiskLocation(num3 + num2));
				DataBlockSource dataSource = dataBlockEntry.DataSource;
				dataSource.Source = DataBlockSource.DataSource.Disk;
				dataSource.StorageOffset = (ulong)(num2 + num3) * (ulong)bytesPerBlock;
				list.Add(dataBlockEntry);
			}
			return list;
		}
	}
}
