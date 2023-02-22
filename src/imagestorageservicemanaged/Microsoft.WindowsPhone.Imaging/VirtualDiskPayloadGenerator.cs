using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class VirtualDiskPayloadGenerator : IDisposable
	{
		private ImageStorage _storage;

		private StorePayload _payload;

		private VirtualDiskSourceAllocation _virtualDiskAllocator;

		private SafeFileHandle _sourceHandle;

		private uint _blockSize;

		private IULogger _logger;

		private ushort _storeHeaderVersion;

		private ushort _numOfStores;

		private ushort _storeIndex;

		private bool _alreadyDisposed;

		internal long TotalSize
		{
			get
			{
				long num = _payload.GetMetadataSize();
				num += GetPaddingSizeInBytes(num);
				return num + GetBlockDataSize();
			}
		}

		public VirtualDiskPayloadGenerator(IULogger logger, uint bytesPerBlock, ImageStorage storage, ushort storeHeaderVersion, ushort numOfStores, ushort storeIndex)
		{
			_sourceHandle = storage.SafeStoreHandle;
			_blockSize = bytesPerBlock;
			_storage = storage;
			if (storage.VirtualHardDiskSectorSize == ImageConstants.DefaultVirtualHardDiskSectorSize)
			{
				_virtualDiskAllocator = new VirtualDiskSourceAllocation(storage.VirtualDiskFilePath, bytesPerBlock);
			}
			else
			{
				_virtualDiskAllocator = null;
			}
			_logger = logger;
			_storeHeaderVersion = storeHeaderVersion;
			_numOfStores = numOfStores;
			_storeIndex = storeIndex;
			_payload = new StorePayload();
		}

		public void GenerateStorePayload(ImageStorage storage)
		{
			if (_storeHeaderVersion == 2)
			{
				_payload.StoreHeader.Initialize2(FullFlashUpdateType.FullUpdate, _blockSize, storage.Image, _numOfStores, _storeIndex, storage.Store.DevicePath);
			}
			else
			{
				_payload.StoreHeader.Initialize(FullFlashUpdateType.FullUpdate, _blockSize, storage.Image);
			}
			GenerateDataEntries(_sourceHandle, storage, storage.VirtualHardDiskSectorSize, _virtualDiskAllocator);
			_payload.StoreHeader.StoreDataEntryCount = (uint)(_payload.Phase1DataEntries.Count + _payload.Phase2DataEntries.Count + _payload.Phase3DataEntries.Count);
			if (_storeHeaderVersion == 2)
			{
				_payload.StoreHeader.StorePayloadSize = (ulong)GetBlockDataSize();
			}
		}

		public void WriteMetadata(IPayloadWrapper payloadWrapper)
		{
			byte[] metadata = _payload.GetMetadata(_blockSize);
			payloadWrapper.Write(metadata);
		}

		public void WriteStorePayload(IPayloadWrapper payloadWrapper)
		{
			byte[] array2 = new byte[_blockSize];
			uint bytesRead = 0u;
			using (VirtualMemoryPtr virtualMemoryPtr = new VirtualMemoryPtr(_payload.StoreHeader.BytesPerBlock))
			{
				for (StorePayload.BlockPhase blockPhase = StorePayload.BlockPhase.Phase1; blockPhase != StorePayload.BlockPhase.Invalid; blockPhase++)
				{
					foreach (DataBlockEntry phaseEntry in _payload.GetPhaseEntries(blockPhase))
					{
						byte[] array = new byte[_blockSize];
						if (phaseEntry.DataSource.Source == DataBlockSource.DataSource.Disk)
						{
							long newFileLocation = 0L;
							Win32Exports.SetFilePointerEx(_sourceHandle, (long)phaseEntry.DataSource.StorageOffset, out newFileLocation, Win32Exports.MoveMethod.FILE_BEGIN);
							Win32Exports.ReadFile(_sourceHandle, virtualMemoryPtr.AllocatedPointer, _payload.StoreHeader.BytesPerBlock, out bytesRead);
							Marshal.Copy(virtualMemoryPtr.AllocatedPointer, array, 0, (int)_payload.StoreHeader.BytesPerBlock);
						}
						else if (phaseEntry.DataSource.Source == DataBlockSource.DataSource.Memory)
						{
							array = phaseEntry.DataSource.GetMemoryData();
						}
						ReplaceGptDiskId(array, FileSystemSourceAllocation.OriginalSystemPartition, ImageConstants.SYSTEM_PARTITION_ID);
						payloadWrapper.Write(array);
					}
				}
			}
		}

		public void Finalize(IPayloadWrapper payloadWrapper)
		{
			payloadWrapper.FinalizeWrapper();
		}

		public uint GetPaddingSizeInBytes(long currentSize)
		{
			return _blockSize - (uint)(int)(currentSize % (long)_blockSize);
		}

		private void ReplaceGptDiskId(byte[] data, Guid originalPartitionId, Guid newPartitionId)
		{
			if (!(originalPartitionId != Guid.Empty) || !(newPartitionId != Guid.Empty))
			{
				return;
			}
			byte[] array = originalPartitionId.ToByteArray();
			byte[] array2 = newPartitionId.ToByteArray();
			for (int i = 0; i < data.Length - array.Length; i++)
			{
				bool flag = true;
				for (int j = 0; j < array.Length; j++)
				{
					if (data[i + j] != array[j])
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					for (int k = 0; k < array2.Length; k++)
					{
						data[i + k] = array2[k];
					}
					i += array2.Length - 1;
				}
			}
		}

		private void GenerateDataEntries(SafeFileHandle sourceHandle, ImageStorage storage, uint sourceSectorSize, ISourceAllocation sourceAllocation)
		{
			IEntryGenerator entryGenerator = null;
			using (DataBlockStream stream = new DataBlockStream(new DiskStreamSource(sourceHandle, _blockSize), _blockSize))
			{
				MasterBootRecord masterBootRecord = new MasterBootRecord(_logger, (int)sourceSectorSize);
				masterBootRecord.ReadFromStream(stream, MasterBootRecord.MbrParseType.Normal);
				entryGenerator = ((!masterBootRecord.IsValidProtectiveMbr()) ? ((IEntryGenerator)new MbrDataEntryGenerator(_logger, _payload, sourceAllocation, sourceHandle, sourceSectorSize, storage.Image)) : ((IEntryGenerator)new GptDataEntryGenerator(storage, _payload, sourceAllocation, sourceHandle, sourceSectorSize)));
				entryGenerator.GenerateEntries(storage.Store.OnlyAllocateDefinedGptEntries);
			}
		}

		~VirtualDiskPayloadGenerator()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (_alreadyDisposed)
			{
				return;
			}
			if (isDisposing)
			{
				_payload = null;
				_sourceHandle = null;
				if (_virtualDiskAllocator != null)
				{
					_virtualDiskAllocator.Dispose();
					_virtualDiskAllocator = null;
				}
			}
			_alreadyDisposed = true;
		}

		private long GetBlockDataSize()
		{
			long num = 0L;
			for (StorePayload.BlockPhase blockPhase = StorePayload.BlockPhase.Phase1; blockPhase != StorePayload.BlockPhase.Invalid; blockPhase++)
			{
				List<DataBlockEntry> phaseEntries = _payload.GetPhaseEntries(blockPhase);
				num += _payload.StoreHeader.BytesPerBlock * phaseEntries.Count;
			}
			return num;
		}
	}
}
