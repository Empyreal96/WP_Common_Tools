using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class GuidPartitionTableEntry
	{
		private byte[] _partitionName;

		private IULogger _logger;

		public Guid PartitionType { get; set; }

		public Guid PartitionId { get; set; }

		public ulong StartingSector { get; set; }

		public ulong LastSector { get; set; }

		public ulong Attributes { get; set; }

		public string PartitionName => Encoding.Unicode.GetString(_partitionName).Split(default(char))[0];

		public GuidPartitionTableEntry(IULogger logger)
		{
			_logger = logger;
		}

		public void WriteToStream(Stream stream, int bytesPerEntry)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			long position = stream.Position;
			byte[] array = PartitionType.ToByteArray();
			binaryWriter.Write(array, 0, array.Length);
			array = PartitionId.ToByteArray();
			binaryWriter.Write(array, 0, array.Length);
			binaryWriter.Write(StartingSector);
			binaryWriter.Write(LastSector);
			binaryWriter.Write(Attributes);
			binaryWriter.Write(_partitionName, 0, _partitionName.Length);
			if (stream.Position - position < bytesPerEntry)
			{
				int num = (int)(bytesPerEntry - (stream.Position - position));
				for (int i = 0; i < num; i++)
				{
					stream.WriteByte(0);
				}
			}
		}

		public void ReadFromStream(Stream stream, int entryByteCount)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			long position = stream.Position;
			PartitionType = new Guid(binaryReader.ReadBytes(16));
			PartitionId = new Guid(binaryReader.ReadBytes(16));
			StartingSector = binaryReader.ReadUInt64();
			LastSector = binaryReader.ReadUInt64();
			Attributes = binaryReader.ReadUInt64();
			_partitionName = binaryReader.ReadBytes(72);
			if (stream.Position - position < entryByteCount)
			{
				stream.Position += entryByteCount - (stream.Position - position);
			}
		}

		public void LogInfo(ushort indentLevel = 0)
		{
			if (!(PartitionType == Guid.Empty) && !string.IsNullOrEmpty(PartitionName) && StartingSector != 0L)
			{
				string text = new StringBuilder().Append(' ', indentLevel).ToString();
				_logger.LogInfo(text + "Partition Name : {0}", PartitionName);
				_logger.LogInfo(text + "Partition Type : {{{0}}}", PartitionType);
				_logger.LogInfo(text + "Partition Id   : {{{0}}}", PartitionId);
				_logger.LogInfo(text + "Starting Sector: 0x{0:x}", StartingSector);
				_logger.LogInfo(text + "Last Sector    : 0x{0:x}", LastSector);
				_logger.LogInfo(text + "Attributes     : 0x{0:x}", Attributes);
				_logger.LogInfo("");
			}
		}

		public void Clean()
		{
			PartitionType = Guid.Empty;
			PartitionId = Guid.Empty;
			StartingSector = 0uL;
			LastSector = 0uL;
			Attributes = 0uL;
			for (int i = 0; i < 72; i++)
			{
				_partitionName[i] = 0;
			}
		}

		public override string ToString()
		{
			return PartitionName;
		}
	}
}
