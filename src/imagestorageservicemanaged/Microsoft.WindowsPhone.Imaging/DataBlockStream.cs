using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class DataBlockStream : Stream
	{
		private int _blockIndex = int.MaxValue;

		private DataBlockEntry _currentEntry;

		private long _position;

		public List<DataBlockEntry> BlockEntries { get; private set; }

		private uint BytesPerBlock { get; set; }

		internal SortedDictionary<int, int> EntryLookupTable { get; set; }

		private IBlockStreamSource Source { get; set; }

		public override bool CanRead => true;

		public override bool CanWrite => true;

		public override bool CanSeek => true;

		public override bool CanTimeout => false;

		public override long Length => Source.Length;

		public override long Position
		{
			get
			{
				return _position;
			}
			set
			{
				if (value > Length)
				{
					throw new ImageStorageException("The given position is beyond the end of the image payload.");
				}
				_position = value;
			}
		}

		private int BlockIndexFromStreamPosition
		{
			get
			{
				if (_position / (long)BytesPerBlock > int.MaxValue)
				{
					throw new ImageStorageException("The stream position is outside the addressable block range.");
				}
				return (int)(_position / (long)BytesPerBlock);
			}
		}

		public DataBlockStream(IBlockStreamSource streamSource, uint bytesPerBlock)
		{
			BytesPerBlock = bytesPerBlock;
			EntryLookupTable = new SortedDictionary<int, int>();
			BlockEntries = new List<DataBlockEntry>();
			Source = streamSource;
		}

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (offset > Length)
			{
				throw new ImageStorageException("The  offset is beyond the end of the image.");
			}
			switch (origin)
			{
			case SeekOrigin.Begin:
				_position = offset;
				return _position;
			case SeekOrigin.Current:
				if (offset == 0L)
				{
					return _position;
				}
				if (offset < 0)
				{
					throw new ImageStorageException("Negative offsets are not implemented.");
				}
				if (_position >= Length)
				{
					throw new ImageStorageException("The offset is beyond the end of the image.");
				}
				if (Length - _position < offset)
				{
					throw new ImageStorageException("The offset is beyond the end of the image.");
				}
				_position = offset;
				return _position;
			case SeekOrigin.End:
				if (offset > 0)
				{
					throw new ImageStorageException("The offset is beyond the end of the image.");
				}
				if (Length + offset < 0)
				{
					throw new ImageStorageException("The offset is invalid.");
				}
				_position = Length + offset;
				return _position;
			default:
				throw new ImageStorageException("The origin parameter is invalid.");
			}
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = 0;
			do
			{
				int num2 = (int)Math.Min(BytesPerBlock - _position % (long)BytesPerBlock, count - num);
				int blockIndexFromStreamPosition = BlockIndexFromStreamPosition;
				if (_blockIndex != blockIndexFromStreamPosition)
				{
					if (!EntryLookupTable.ContainsKey(blockIndexFromStreamPosition))
					{
						DataBlockEntry dataBlockEntry = new DataBlockEntry(BytesPerBlock);
						dataBlockEntry.DataSource.Source = DataBlockSource.DataSource.Memory;
						byte[] newMemoryData = dataBlockEntry.DataSource.GetNewMemoryData(BytesPerBlock);
						Source.ReadBlock((uint)blockIndexFromStreamPosition, newMemoryData, 0);
						dataBlockEntry.BlockLocationsOnDisk.Add(new DiskLocation((uint)blockIndexFromStreamPosition, DiskLocation.DiskAccessMethod.DiskBegin));
						BlockEntries.Add(dataBlockEntry);
						EntryLookupTable.Add(blockIndexFromStreamPosition, BlockEntries.Count - 1);
						_currentEntry = dataBlockEntry;
					}
					else
					{
						_currentEntry = BlockEntries[EntryLookupTable[blockIndexFromStreamPosition]];
					}
					_blockIndex = blockIndexFromStreamPosition;
				}
				int sourceIndex = (int)(_position % (long)BytesPerBlock);
				Array.Copy(_currentEntry.DataSource.GetMemoryData(), sourceIndex, buffer, offset, num2);
				offset += num2;
				num += num2;
				_position += num2;
			}
			while (num < count);
			return num;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			if (offset + Position > Length)
			{
				throw new EndOfStreamException("Cannot write past the end of the stream.");
			}
			int num = 0;
			do
			{
				int num2 = (int)Math.Min(BytesPerBlock - _position % (long)BytesPerBlock, count - num);
				int blockIndexFromStreamPosition = BlockIndexFromStreamPosition;
				if (!EntryLookupTable.ContainsKey(blockIndexFromStreamPosition))
				{
					throw new ImageStorageException("Attempting to write to an unallocated block data stream location.");
				}
				if (blockIndexFromStreamPosition != _blockIndex)
				{
					_currentEntry = BlockEntries[EntryLookupTable[blockIndexFromStreamPosition]];
					_blockIndex = blockIndexFromStreamPosition;
				}
				int destinationIndex = (int)(_position % (long)BytesPerBlock);
				Array.Copy(buffer, offset, _currentEntry.DataSource.GetMemoryData(), destinationIndex, num2);
				offset += num2;
				num += num2;
				_position += num2;
			}
			while (num < count);
		}
	}
}
