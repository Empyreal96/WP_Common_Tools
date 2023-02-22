using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class DataBlockEntry
	{
		private List<DiskLocation> _blockLocations = new List<DiskLocation>();

		private uint BytesPerBlock { get; set; }

		public List<DiskLocation> BlockLocationsOnDisk => _blockLocations;

		public DataBlockSource DataSource { get; set; }

		public int SizeInBytes => 8 + _blockLocations.Count * DiskLocation.SizeInBytes;

		public DataBlockEntry(uint bytesPerBlock)
		{
			BytesPerBlock = bytesPerBlock;
			DataSource = new DataBlockSource();
		}

		public void WriteEntryToStream(Stream outputStream)
		{
			BinaryWriter binaryWriter = new BinaryWriter(outputStream);
			binaryWriter.Write((uint)_blockLocations.Count);
			binaryWriter.Write(1);
			for (int i = 0; i < _blockLocations.Count; i++)
			{
				_blockLocations[i].Write(binaryWriter);
			}
			binaryWriter = null;
		}

		public void ReadEntryFromStream(BinaryReader reader, uint index)
		{
			int num = reader.ReadInt32();
			if (reader.ReadUInt32() != 1)
			{
				throw new ImageStorageException("More than one block per data block entry is not currently supported.");
			}
			for (int i = 0; i < num; i++)
			{
				DiskLocation diskLocation = new DiskLocation();
				diskLocation.Read(reader);
				_blockLocations.Add(diskLocation);
			}
			DataBlockSource dataBlockSource = new DataBlockSource();
			dataBlockSource.Source = DataBlockSource.DataSource.Disk;
			dataBlockSource.StorageOffset = index * BytesPerBlock;
			DataSource = dataBlockSource;
		}

		public void WriteDataToByteArray(Stream sourceStream, byte[] blockData, int index, int byteCount)
		{
			int num = Math.Min(byteCount, (int)BytesPerBlock);
			switch (DataSource.Source)
			{
			case DataBlockSource.DataSource.Zero:
				Array.Clear(blockData, index, num);
				break;
			case DataBlockSource.DataSource.Memory:
				Array.Copy(DataSource.GetMemoryData(), 0, blockData, index, num);
				break;
			case DataBlockSource.DataSource.Disk:
				sourceStream.Position = (long)DataSource.StorageOffset;
				sourceStream.Read(blockData, index, num);
				break;
			}
		}

		public void WriteDataToStream(Stream outputStream, Stream sourceStream, byte[] blockData)
		{
			WriteDataToByteArray(sourceStream, blockData, 0, (int)BytesPerBlock);
			outputStream.Write(blockData, 0, blockData.Length);
		}

		public void LogInfo(IULogger logger, bool logSources, ushort indentLevel = 0)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Block Location Count: {0}", _blockLocations.Count);
			foreach (DiskLocation blockLocation in _blockLocations)
			{
				blockLocation.LogInfo(logger, (ushort)(indentLevel + 2));
			}
			if (logSources)
			{
				DataSource.LogInfo(logger, (ushort)(indentLevel + 2));
			}
		}

		public DataBlockEntry CreateMemoryBlockEntry(Stream sourceStream)
		{
			DataBlockEntry result = new DataBlockEntry(BytesPerBlock)
			{
				_blockLocations = new List<DiskLocation>(_blockLocations)
			};
			byte[] newMemoryData = new DataBlockSource
			{
				Source = DataBlockSource.DataSource.Memory
			}.GetNewMemoryData(BytesPerBlock);
			WriteDataToByteArray(sourceStream, newMemoryData, 0, (int)BytesPerBlock);
			return result;
		}
	}
}
