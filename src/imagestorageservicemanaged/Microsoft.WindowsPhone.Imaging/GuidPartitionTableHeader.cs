using System;
using System.IO;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class GuidPartitionTableHeader
	{
		public const ulong HeaderSignature = 6075990659671082565uL;

		private IULogger _logger;

		public ulong Signature { get; set; }

		public uint Revision { get; set; }

		public uint HeaderSize { get; set; }

		public uint HeaderCrc32 { get; set; }

		public uint Reserved { get; set; }

		public ulong HeaderSector { get; set; }

		public ulong AlternateHeaderSector { get; set; }

		public ulong FirstUsableSector { get; set; }

		public ulong LastUsableSector { get; set; }

		public Guid DiskId { get; set; }

		public ulong PartitionEntryStartSector { get; set; }

		public uint PartitionEntryCount { get; set; }

		public uint PartitionEntrySizeInBytes { get; set; }

		public uint PartitionEntryArrayCrc32 { get; set; }

		public GuidPartitionTableHeader(IULogger logger)
		{
			_logger = logger;
		}

		public void WriteToStream(Stream stream, int bytesPerSector)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			binaryWriter.Write(Signature);
			binaryWriter.Write(Revision);
			binaryWriter.Write(HeaderSize);
			binaryWriter.Write(HeaderCrc32);
			binaryWriter.Write(Reserved);
			binaryWriter.Write(HeaderSector);
			binaryWriter.Write(AlternateHeaderSector);
			binaryWriter.Write(FirstUsableSector);
			binaryWriter.Write(LastUsableSector);
			byte[] array = DiskId.ToByteArray();
			binaryWriter.Write(array, 0, array.Length);
			binaryWriter.Write(PartitionEntryStartSector);
			binaryWriter.Write(PartitionEntryCount);
			binaryWriter.Write(PartitionEntrySizeInBytes);
			binaryWriter.Write(PartitionEntryArrayCrc32);
			if (stream.Position % bytesPerSector != 0L)
			{
				stream.Position += bytesPerSector - stream.Position % bytesPerSector;
			}
		}

		public void ReadFromStream(Stream stream, int bytesPerSector)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			Signature = binaryReader.ReadUInt64();
			if (Signature != 6075990659671082565L)
			{
				throw new ImageStorageException("The EFI header signature is invalid.");
			}
			Revision = binaryReader.ReadUInt32();
			if (Revision != 65536)
			{
				throw new ImageStorageException("The EFI header revision is an unsupported version.");
			}
			HeaderSize = binaryReader.ReadUInt32();
			HeaderCrc32 = binaryReader.ReadUInt32();
			Reserved = binaryReader.ReadUInt32();
			if (Reserved != 0)
			{
				throw new ImageStorageException("The reserved field in the EFI header is not zero.");
			}
			HeaderSector = binaryReader.ReadUInt64();
			AlternateHeaderSector = binaryReader.ReadUInt64();
			FirstUsableSector = binaryReader.ReadUInt64();
			LastUsableSector = binaryReader.ReadUInt64();
			DiskId = new Guid(binaryReader.ReadBytes(16));
			PartitionEntryStartSector = binaryReader.ReadUInt64();
			PartitionEntryCount = binaryReader.ReadUInt32();
			PartitionEntrySizeInBytes = binaryReader.ReadUInt32();
			PartitionEntryArrayCrc32 = binaryReader.ReadUInt32();
			if (stream.Position % bytesPerSector != 0L)
			{
				stream.Position += bytesPerSector - stream.Position % bytesPerSector;
			}
		}

		public void LogInfo(ushort indentLevel = 0)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			_logger.LogInfo(text + "GUID Partition Table Header");
			indentLevel = (ushort)(indentLevel + 2);
			text = new StringBuilder().Append(' ', indentLevel).ToString();
			_logger.LogInfo(text + "Revision                     : 0x{0:x}", Revision);
			_logger.LogInfo(text + "Header Size                  : 0x{0:x}", HeaderSize);
			_logger.LogInfo(text + "Header Sector                : 0x{0:x}", HeaderSector);
			_logger.LogInfo(text + "Alternate Header Sector      : 0x{0:x}", AlternateHeaderSector);
			_logger.LogInfo(text + "First Usable Sector          : 0x{0:x}", FirstUsableSector);
			_logger.LogInfo(text + "Last Usable Sector           : 0x{0:x}", LastUsableSector);
			_logger.LogInfo(text + "Disk Id                      : {{{0}}}", DiskId);
			_logger.LogInfo(text + "Partition Entry Start Sector : 0x{0:x}", PartitionEntryStartSector);
			_logger.LogInfo(text + "Partition Entry Size In Bytes: 0x{0:x}", PartitionEntrySizeInBytes);
			_logger.LogInfo(text + "Partition Entry Array CRC    : 0x{0:x}", PartitionEntryArrayCrc32);
		}

		public bool IsValid(ulong headerSectorIndex, byte[] partitionEntryArray)
		{
			return false;
		}

		private uint ComputeHeaderCrc(int bytesPerSector)
		{
			MemoryStream memoryStream = new MemoryStream();
			CRC32 cRC = new CRC32();
			uint headerCrc = HeaderCrc32;
			HeaderCrc32 = 0u;
			WriteToStream(memoryStream, bytesPerSector);
			byte[] array = cRC.ComputeHash(memoryStream.GetBuffer(), 0, (int)HeaderSize);
			int result = (array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3];
			HeaderCrc32 = headerCrc;
			return (uint)result;
		}

		public void ValidateHeaderCrc(int bytesPerSector)
		{
			uint num = ComputeHeaderCrc(bytesPerSector);
			if (HeaderCrc32 != num)
			{
				throw new ImageStorageException($"The GPT header CRC is invalid.  Actual: {HeaderCrc32:x} Expected {num:x}.");
			}
		}

		public void FixHeaderCrc(int bytesPerSector)
		{
			HeaderCrc32 = ComputeHeaderCrc(bytesPerSector);
		}
	}
}
