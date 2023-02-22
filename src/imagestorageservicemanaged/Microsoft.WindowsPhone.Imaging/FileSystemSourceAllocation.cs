using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class FileSystemSourceAllocation : ISourceAllocation
	{
		private ImageStorage _storage;

		private string _partitionName;

		private uint _blockSize;

		private byte[] _blockAllocationBitmap;

		private uint _blockCount;

		private ulong _partitionOffset;

		private bool _isDestkopImage;

		internal static Guid OriginalSystemPartition = Guid.Empty;

		public bool this[uint blockIndex]
		{
			get
			{
				byte b = (byte)(1 << (int)(blockIndex & 7));
				return (_blockAllocationBitmap[blockIndex / 8u] & b) != 0;
			}
		}

		public FileSystemSourceAllocation(ImageStorage imageService, string partitionName, ulong partitionOffset, uint blockSize)
			: this(imageService, partitionName, partitionOffset, blockSize, false)
		{
		}

		public FileSystemSourceAllocation(ImageStorage imageService, string partitionName, ulong partitionOffset, uint blockSize, bool isDesktopImage)
		{
			_isDestkopImage = isDesktopImage;
			_storage = imageService;
			_blockSize = blockSize;
			_partitionOffset = partitionOffset;
			_partitionName = partitionName;
			ulong partitionSize = NativeImaging.GetPartitionSize(_storage.ServiceHandle, _storage.StoreId, partitionName);
			uint sectorSize = NativeImaging.GetSectorSize(_storage.ServiceHandle, _storage.StoreId);
			ulong num = partitionSize * sectorSize;
			if (num / sectorSize != partitionSize)
			{
				throw new ImageStorageException($"Volume {partitionName} is too large to be byte-addressed with a 64-bit value.");
			}
			_blockCount = (uint)((num + blockSize - 1) / blockSize);
			if ((ulong)((long)_blockCount * (long)blockSize) < num)
			{
				throw new ImageStorageException($"Volume {partitionName} is too large to access with a 32-bit block count.");
			}
			_blockAllocationBitmap = new byte[(_blockCount + 7) / 8u];
			NativeImaging.GetBlockAllocationBitmap(_storage.ServiceHandle, _storage.StoreId, partitionName, _blockSize, _blockAllocationBitmap);
		}

		public uint GetAllocationSize()
		{
			return _blockSize;
		}

		public bool BlockIsAllocated(ulong diskByteOffset)
		{
			uint blockIndex = (uint)((diskByteOffset - _partitionOffset) / _blockSize);
			return this[blockIndex];
		}

		public List<DataBlockEntry> GenerateDataEntries()
		{
			List<DataBlockEntry> list = new List<DataBlockEntry>();
			uint num = (uint)(_partitionOffset / _blockSize);
			for (uint num2 = 0u; num2 < _blockCount; num2++)
			{
				if (this[num2])
				{
					DataBlockEntry dataBlockEntry = new DataBlockEntry(_blockSize);
					dataBlockEntry.BlockLocationsOnDisk.Add(new DiskLocation(num2 + num));
					DataBlockSource dataSource = dataBlockEntry.DataSource;
					dataSource.Source = DataBlockSource.DataSource.Disk;
					dataSource.StorageOffset = (ulong)(num + num2) * (ulong)_blockSize;
					list.Add(dataBlockEntry);
				}
			}
			return list;
		}

		[Conditional("DEBUG")]
		public void ValidateDataEntries(List<DataBlockEntry> entries)
		{
			byte[] array = new byte[(_blockCount + 7) / 8u];
			Array.Clear(array, 0, (int)(_blockCount & 7) / 8);
			uint num = (uint)(_partitionOffset / _blockSize);
			foreach (DataBlockEntry entry in entries)
			{
				uint num2 = entry.BlockLocationsOnDisk[0].BlockIndex - num;
				array[num2 / 8u] = (byte)(array[num2 / 8u] | (1 << (int)(num2 % 8u)));
			}
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] != _blockAllocationBitmap[i])
				{
					throw new ImageStorageException($"The block bitmap generated from the volume doesn't match the bitmap generated from the data entries at offset {i}");
				}
			}
		}
	}
}
