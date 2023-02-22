using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class DataBlockSource
	{
		public enum DataSource
		{
			Zero,
			Disk,
			Memory
		}

		private byte[] _memoryData;

		public DataSource Source { get; set; }

		public ulong StorageOffset { get; set; }

		public void SetMemoryData(byte[] buffer, int bufferOffset, int blockSize)
		{
			_memoryData = new byte[blockSize];
			Array.Copy(buffer, bufferOffset, _memoryData, 0, blockSize);
		}

		public void SetMemoryData(FileStream stream, int blockSize)
		{
			_memoryData = new byte[blockSize];
			stream.Read(_memoryData, 0, blockSize);
		}

		public void CreateMemoryData(int blockSize)
		{
			_memoryData = new byte[blockSize];
		}

		public byte[] GetMemoryData()
		{
			return _memoryData;
		}

		public byte[] GetNewMemoryData(uint blockSize)
		{
			_memoryData = new byte[blockSize];
			return _memoryData;
		}

		public void LogInfo(IULogger logger, ushort indentLevel = 0)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			logger.LogInfo(text + "Source           : {0}", Source);
			if (Source == DataSource.Disk)
			{
				logger.LogInfo(text + "  : {0}", StorageOffset);
			}
			logger.LogInfo("");
		}
	}
}
