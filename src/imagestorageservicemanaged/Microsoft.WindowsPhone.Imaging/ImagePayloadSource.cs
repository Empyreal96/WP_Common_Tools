using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class ImagePayloadSource : IBlockStreamSource
	{
		private StorePayload _payload;

		private Stream _stream;

		private long _firstDataByte;

		private uint _blockSize;

		public long Length { get; private set; }

		public ImagePayloadSource(Stream imageStream, StorePayload payload, long firstDataBlockIndex, long diskSizeInBytes)
		{
			_payload = payload;
			_blockSize = payload.StoreHeader.BytesPerBlock;
			_stream = imageStream;
			Length = diskSizeInBytes;
			_firstDataByte = firstDataBlockIndex;
		}

		private long GetLocationDiskOffset(DiskLocation location)
		{
			if (location.AccessMethod == DiskLocation.DiskAccessMethod.DiskBegin)
			{
				return (long)location.BlockIndex * (long)_blockSize;
			}
			return Length - (location.BlockIndex + 1) * _blockSize;
		}

		private long GetDataBlockOffsetForDiskOffset(long diskOffset)
		{
			long num = -1L;
			uint num2 = _payload.StoreHeader.StoreDataEntryCount - 1;
			StorePayload.BlockPhase blockPhase = StorePayload.BlockPhase.Invalid;
			do
			{
				blockPhase--;
				List<DataBlockEntry> phaseEntries = _payload.GetPhaseEntries(blockPhase);
				for (int i = 0; i < phaseEntries.Count; i++)
				{
					DataBlockEntry dataBlockEntry = phaseEntries[phaseEntries.Count - (i + 1)];
					for (int j = 0; j < dataBlockEntry.BlockLocationsOnDisk.Count; j++)
					{
						if (diskOffset == GetLocationDiskOffset(dataBlockEntry.BlockLocationsOnDisk[j]))
						{
							num = (long)_blockSize * (long)num2;
							break;
						}
					}
					if (num > 0)
					{
						break;
					}
					num2--;
				}
			}
			while (num < 0 && blockPhase != 0);
			return num;
		}

		public void ReadBlock(uint blockIndex, byte[] buffer, int bufferIndex)
		{
			long num = (long)blockIndex * (long)_blockSize;
			if (num > Length)
			{
				throw new ImageStorageException("Attempting to read beyond the end of the disk.");
			}
			long dataBlockOffsetForDiskOffset = GetDataBlockOffsetForDiskOffset(num);
			if (dataBlockOffsetForDiskOffset == -1)
			{
				Array.Clear(buffer, bufferIndex, (int)_blockSize);
				return;
			}
			_stream.Position = _firstDataByte + dataBlockOffsetForDiskOffset;
			_stream.Read(buffer, bufferIndex, (int)_blockSize);
		}
	}
}
