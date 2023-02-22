using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class PayloadReader
	{
		private class PayloadOffset
		{
			public StorePayload Payload { get; set; }

			public long Offset { get; set; }
		}

		private List<PayloadOffset> _payloadOffsets;

		private FileStream _payloadStream;

		public ReadOnlyCollection<StorePayload> Payloads
		{
			get
			{
				List<StorePayload> list = new List<StorePayload>();
				foreach (PayloadOffset payloadOffset in _payloadOffsets)
				{
					list.Add(payloadOffset.Payload);
				}
				return list.AsReadOnly();
			}
		}

		public PayloadReader(FileStream payloadStream)
		{
			_payloadStream = payloadStream;
			_payloadOffsets = new List<PayloadOffset>();
			int num = 1;
			for (int i = 1; i <= num; i++)
			{
				StorePayload storePayload = new StorePayload();
				storePayload.ReadMetadataFromStream(payloadStream);
				long num2 = storePayload.StoreHeader.BytesPerBlock - _payloadStream.Position % (long)storePayload.StoreHeader.BytesPerBlock;
				_payloadStream.Position += num2;
				_payloadOffsets.Add(new PayloadOffset
				{
					Payload = storePayload
				});
				if (storePayload.StoreHeader.MajorVersion >= 2)
				{
					num = storePayload.StoreHeader.NumberOfStores;
				}
			}
			long num3 = _payloadStream.Position;
			for (int j = 0; j < num; j++)
			{
				PayloadOffset payloadOffset = _payloadOffsets[j];
				payloadOffset.Offset = num3;
				ImageStoreHeader storeHeader = payloadOffset.Payload.StoreHeader;
				num3 += storeHeader.BytesPerBlock * storeHeader.StoreDataEntryCount;
			}
		}

		public void WriteToDisk(SafeFileHandle diskHandle, StorePayload payload)
		{
			uint bytesPerBlock = payload.StoreHeader.BytesPerBlock;
			long newFileLocation = 0L;
			uint bytesWritten = 0u;
			uint bytesRead = 0u;
			ulong sectorCount = NativeImaging.GetSectorCount(IntPtr.Zero, diskHandle);
			uint sectorSize = NativeImaging.GetSectorSize(IntPtr.Zero, diskHandle);
			long num = (long)(sectorCount * sectorSize);
			PayloadOffset payloadOffset = FindPayloadOffset(payload);
			if (payloadOffset == null)
			{
				throw new ImageStorageException("Unable to find store payload.");
			}
			_payloadStream.Position = payloadOffset.Offset;
			SafeFileHandle safeFileHandle = _payloadStream.SafeFileHandle;
			using (VirtualMemoryPtr virtualMemoryPtr = new VirtualMemoryPtr(bytesPerBlock))
			{
				for (StorePayload.BlockPhase blockPhase = StorePayload.BlockPhase.Phase1; blockPhase != StorePayload.BlockPhase.Invalid; blockPhase++)
				{
					foreach (DataBlockEntry phaseEntry in payload.GetPhaseEntries(blockPhase))
					{
						Win32Exports.ReadFile(safeFileHandle, virtualMemoryPtr, bytesPerBlock, out bytesRead);
						for (int i = 0; i < phaseEntry.BlockLocationsOnDisk.Count; i++)
						{
							long num2 = (long)phaseEntry.BlockLocationsOnDisk[i].BlockIndex * (long)bytesPerBlock;
							if (phaseEntry.BlockLocationsOnDisk[i].AccessMethod == DiskLocation.DiskAccessMethod.DiskEnd)
							{
								num2 = num - num2 - bytesPerBlock;
							}
							Win32Exports.SetFilePointerEx(diskHandle, num2, out newFileLocation, Win32Exports.MoveMethod.FILE_BEGIN);
							Win32Exports.WriteFile(diskHandle, virtualMemoryPtr, bytesPerBlock, out bytesWritten);
						}
					}
				}
			}
		}

		public DataBlockStream GetDataBlockStream(StorePayload payload, int sectorSize, long totalByteCount)
		{
			PayloadOffset payloadOffset = FindPayloadOffset(payload);
			if (payloadOffset == null)
			{
				throw new ImageStorageException("Unable to find store payload.");
			}
			return new DataBlockStream(new ImagePayloadSource(_payloadStream, payload, payloadOffset.Offset, totalByteCount), payload.StoreHeader.BytesPerBlock);
		}

		public void ValidatePayloadPartitions(int sectorSize, long totalByteCount, StorePayload payload, uint partitionStyle, bool isMainOSStore, IULogger logger)
		{
			DataBlockStream dataBlockStream = GetDataBlockStream(payload, sectorSize, totalByteCount);
			if (partitionStyle == ImageConstants.PartitionTypeGpt)
			{
				GuidPartitionTable guidPartitionTable = new GuidPartitionTable(sectorSize, logger);
				guidPartitionTable.ReadFromStream(dataBlockStream, true);
				if (isMainOSStore && guidPartitionTable.GetEntry(ImageConstants.MAINOS_PARTITION_NAME) == null)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The given FFU does not contain the partition '{ImageConstants.MAINOS_PARTITION_NAME}'.");
				}
				return;
			}
			if (partitionStyle == ImageConstants.PartitionTypeMbr)
			{
				MasterBootRecord masterBootRecord = new MasterBootRecord(logger, sectorSize);
				masterBootRecord.ReadFromStream(dataBlockStream, MasterBootRecord.MbrParseType.Normal);
				if (masterBootRecord.FindPartitionByType(ImageConstants.MBR_METADATA_PARTITION_TYPE) == null)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The given FFU does not contain the partition '{ImageConstants.MBR_METADATA_PARTITION_NAME}'.");
				}
				if (masterBootRecord.FindPartitionByName(ImageConstants.MAINOS_PARTITION_NAME) == null)
				{
					throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The given FFU does not contain the partition '{ImageConstants.MAINOS_PARTITION_NAME}'.");
				}
				return;
			}
			throw new ImageStorageException("The payload contains an invalid partition style.");
		}

		public void LogPayload(IULogger logger, bool logStoreHeader, bool logDataEntries)
		{
			foreach (PayloadOffset payloadOffset in _payloadOffsets)
			{
				payloadOffset.Payload.LogInfo(logger, logStoreHeader, logDataEntries);
			}
		}

		private PayloadOffset FindPayloadOffset(StorePayload payload)
		{
			for (int i = 0; i < _payloadOffsets.Count; i++)
			{
				PayloadOffset payloadOffset = _payloadOffsets[i];
				if (payloadOffset.Payload == payload)
				{
					return payloadOffset;
				}
			}
			return null;
		}
	}
}
