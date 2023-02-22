using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class GptDataEntryGenerator : IEntryGenerator
	{
		private ISourceAllocation _storageAllocation;

		private StorePayload _payload;

		private SafeFileHandle _sourceHandle;

		private GuidPartitionTable _table;

		private FullFlashUpdateImage _image;

		private uint _sectorSize;

		private List<DataBlockEntry> _phase2PartitionTableEntries;

		private List<FullFlashUpdateImage.FullFlashUpdatePartition> _finalPartitions;

		private ImageStorage _storage;

		public GptDataEntryGenerator(ImageStorage storage, StorePayload storePayload, ISourceAllocation storageAllocation, SafeFileHandle sourceHandle, uint sourceSectorSize)
		{
			_payload = storePayload;
			_storageAllocation = storageAllocation;
			_sourceHandle = sourceHandle;
			_image = storage.Image;
			_sectorSize = sourceSectorSize;
			_table = new GuidPartitionTable((int)sourceSectorSize, storage.Logger);
			_finalPartitions = new List<FullFlashUpdateImage.FullFlashUpdatePartition>();
			_storage = storage;
			if (storePayload.StoreHeader.BytesPerBlock < _storage.Store.SectorSize)
			{
				throw new ImageStorageException("The data block size is less than the device sector size.");
			}
			if (storePayload.StoreHeader.BytesPerBlock % _storage.Store.SectorSize != 0)
			{
				throw new ImageStorageException("The data block size is not a multiple of the device sector size.");
			}
			if (storageAllocation != null)
			{
				if (storePayload.StoreHeader.BytesPerBlock > storageAllocation.GetAllocationSize())
				{
					throw new ImageStorageException("The payload block size is larger than the allocation size of the temp store.");
				}
				if (storageAllocation.GetAllocationSize() % storePayload.StoreHeader.BytesPerBlock != 0)
				{
					throw new ImageStorageException("The allocation size of the temp store is not a multiple of the payload block size.");
				}
			}
		}

		public void GenerateEntries(bool onlyAllocateDefinedGptEntries)
		{
			_payload.Phase1DataEntries = GeneratePhase1Entries();
			_payload.Phase2DataEntries = GeneratePhase2Entries(onlyAllocateDefinedGptEntries);
			_payload.Phase3DataEntries = GeneratePhase3Entries(onlyAllocateDefinedGptEntries);
		}

		private List<DataBlockEntry> GeneratePhase1Entries()
		{
			List<DataBlockEntry> list = new List<DataBlockEntry>((int)(131072u / _payload.StoreHeader.BytesPerBlock));
			for (int i = 0; i < list.Capacity; i++)
			{
				DataBlockEntry dataBlockEntry = new DataBlockEntry(_payload.StoreHeader.BytesPerBlock);
				dataBlockEntry.DataSource.Source = DataBlockSource.DataSource.Zero;
				dataBlockEntry.BlockLocationsOnDisk.Add(new DiskLocation((uint)i, DiskLocation.DiskAccessMethod.DiskBegin));
				dataBlockEntry.BlockLocationsOnDisk.Add(new DiskLocation((uint)i, DiskLocation.DiskAccessMethod.DiskEnd));
				list.Add(dataBlockEntry);
			}
			_payload.StoreHeader.InitialPartitionTableBlockCount = (uint)list.Count;
			return list;
		}

		private List<DataBlockEntry> GeneratePhase2Entries(bool onlyAllocateDefinedGptEntries)
		{
			DataBlockStream dataBlockStream = new DataBlockStream(new DiskStreamSource(_sourceHandle, _payload.StoreHeader.BytesPerBlock), _payload.StoreHeader.BytesPerBlock);
			_table.ReadFromStream(dataBlockStream, true);
			_table.ValidateCrcs();
			for (int i = 0; i < _storage.Store.PartitionCount; i++)
			{
				FullFlashUpdateImage.FullFlashUpdatePartition fullFlashUpdatePartition = _storage.Store.Partitions[i];
				if (!fullFlashUpdatePartition.RequiredToFlash)
				{
					_finalPartitions.Add(fullFlashUpdatePartition);
					_table.RemoveEntry(fullFlashUpdatePartition.Name);
				}
			}
			if (_storage.IsMainOSStorage)
			{
				_table.NormalizeGptIds(out FileSystemSourceAllocation.OriginalSystemPartition);
			}
			_table.FixCrcs();
			_table.WriteToStream(dataBlockStream, true, onlyAllocateDefinedGptEntries);
			_phase2PartitionTableEntries = dataBlockStream.BlockEntries;
			List<DataBlockEntry> list = new List<DataBlockEntry>();
			foreach (GuidPartitionTableEntry entry in _table.Entries)
			{
				if (entry.StartingSector != 0)
				{
					GenerateDataEntries(list, entry);
				}
			}
			FilterPartitionTablesFromDataBlocks(list);
			_payload.StoreHeader.FlashOnlyPartitionTableBlockCount = (uint)_phase2PartitionTableEntries.Count;
			_payload.StoreHeader.FlashOnlyPartitionTableBlockIndex = _payload.StoreHeader.InitialPartitionTableBlockCount + (uint)list.Count;
			list.AddRange(_phase2PartitionTableEntries);
			return list;
		}

		private List<DataBlockEntry> GeneratePhase3Entries(bool onlyAllocateDefinedGptEntries)
		{
			List<DataBlockEntry> list = new List<DataBlockEntry>();
			DiskStreamSource streamSource = new DiskStreamSource(_sourceHandle, _payload.StoreHeader.BytesPerBlock);
			DataBlockStream dataBlockStream = new DataBlockStream(streamSource, _payload.StoreHeader.BytesPerBlock);
			_table.ReadFromStream(dataBlockStream, true);
			_table.ValidateCrcs();
			while (_finalPartitions.Count > 0)
			{
				foreach (GuidPartitionTableEntry entry2 in _table.Entries)
				{
					if (string.Compare(entry2.PartitionName, _finalPartitions[0].Name, true, CultureInfo.InvariantCulture) == 0)
					{
						GenerateDataEntries(list, entry2);
						_finalPartitions.RemoveAt(0);
						break;
					}
				}
			}
			GuidPartitionTableEntry entry = _table.GetEntry(ImageConstants.MAINOS_PARTITION_NAME);
			if (entry != null)
			{
				entry.Attributes &= ~ImageConstants.GPT_ATTRIBUTE_NO_DRIVE_LETTER;
			}
			if (_storage.IsMainOSStorage)
			{
				_table.NormalizeGptIds(out FileSystemSourceAllocation.OriginalSystemPartition);
			}
			_table.FixCrcs();
			_table.WriteToStream(dataBlockStream, true, onlyAllocateDefinedGptEntries);
			FilterPartitionTablesFromDataBlocks(list);
			_payload.StoreHeader.FinalPartitionTableBlockCount = (uint)dataBlockStream.BlockEntries.Count;
			_payload.StoreHeader.FinalPartitionTableBlockIndex = _payload.StoreHeader.FlashOnlyPartitionTableBlockIndex + _payload.StoreHeader.FlashOnlyPartitionTableBlockCount + (uint)list.Count;
			list.AddRange(dataBlockStream.BlockEntries);
			dataBlockStream = new DataBlockStream(streamSource, _payload.StoreHeader.BytesPerBlock);
			_table.ReadFromStream(dataBlockStream, false);
			List<DataBlockEntry> blockEntries = dataBlockStream.BlockEntries;
			_payload.StoreHeader.FinalPartitionTableBlockCount += (uint)dataBlockStream.BlockEntries.Count;
			ConvertEntriesToUseEndOfDisk(blockEntries, NativeImaging.GetSectorCount(IntPtr.Zero, _sourceHandle));
			list.AddRange(blockEntries);
			return list;
		}

		private void GenerateDataEntries(List<DataBlockEntry> dataEntries, GuidPartitionTableEntry entry)
		{
			List<DataBlockEntry> list = null;
			if (!_storage.PartitionIsMountedRaw(entry.PartitionName))
			{
				ulong partitionOffset = NativeImaging.GetPartitionOffset(_storage.ServiceHandle, _storage.StoreId, entry.PartitionName) * _sectorSize;
				list = new FileSystemSourceAllocation(_storage, entry.PartitionName, partitionOffset, _payload.StoreHeader.BytesPerBlock).GenerateDataEntries();
			}
			else if (!_storage.IsBackingFileVhdx() || _storage.IsPartitionTargeted(entry.PartitionName))
			{
				ulong num = entry.LastSector + 1 - entry.StartingSector;
				num *= _sectorSize;
				list = GenerateDataEntriesFromDisk((long)(entry.StartingSector * _sectorSize), (long)num);
				FilterUnAllocatedDataEntries(list);
			}
			else
			{
				list = new List<DataBlockEntry>();
			}
			dataEntries.AddRange(list);
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
				if (dataBlockEntry.DataSource.Source == DataBlockSource.DataSource.Disk && _storageAllocation != null && !_storageAllocation.BlockIsAllocated(dataBlockEntry.DataSource.StorageOffset))
				{
					dataBlocks.RemoveAt(i--);
				}
			}
		}

		private List<DataBlockEntry> GenerateDataEntriesFromDisk(long diskOffset, long byteCount)
		{
			uint bytesPerBlock = _payload.StoreHeader.BytesPerBlock;
			List<DataBlockEntry> list = new List<DataBlockEntry>();
			if (diskOffset % (long)bytesPerBlock != 0L)
			{
				throw new ImageStorageException("Parameter 'diskOffset' must be a multiple of the block size.");
			}
			uint num = (uint)((byteCount + bytesPerBlock - 1) / (long)bytesPerBlock);
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

		private void ConvertEntriesToUseEndOfDisk(List<DataBlockEntry> entries, ulong totalSectorCount)
		{
			uint num = _payload.StoreHeader.BytesPerBlock / _sectorSize;
			if (totalSectorCount / num > uint.MaxValue)
			{
				throw new ImageStorageException("The image minimum sector count is too large to be addressed with a 32-bit block count.");
			}
			uint num2 = (uint)(totalSectorCount / num);
			foreach (DataBlockEntry entry in entries)
			{
				uint blockIndex = entry.BlockLocationsOnDisk[0].BlockIndex;
				uint blockIndex2 = num2 - blockIndex - 1;
				entry.BlockLocationsOnDisk[0].AccessMethod = DiskLocation.DiskAccessMethod.DiskEnd;
				entry.BlockLocationsOnDisk[0].BlockIndex = blockIndex2;
			}
		}
	}
}
