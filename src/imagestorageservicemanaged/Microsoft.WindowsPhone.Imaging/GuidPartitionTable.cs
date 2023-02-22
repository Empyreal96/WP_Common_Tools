using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class GuidPartitionTable
	{
		private const int MIN_GPT_PARTITION_ARRAY_SIZE = 16384;

		public int BytesPerSector { get; set; }

		public MasterBootRecord ProtectiveMbr { get; private set; }

		public GuidPartitionTableHeader Header { get; private set; }

		public List<GuidPartitionTableEntry> Entries { get; private set; }

		private IULogger Logger { get; set; }

		public GuidPartitionTable(int bytesPerSector, IULogger logger)
		{
			Logger = logger;
			BytesPerSector = bytesPerSector;
		}

		public void ReadFromStream(Stream stream, bool readPrimaryTable)
		{
			ReadFromStream(stream, readPrimaryTable, false);
		}

		public void ReadFromStream(Stream stream, bool readPrimaryTable, bool isDesktopImage)
		{
			long position = BytesPerSector;
			if (BytesPerSector == 0)
			{
				throw new ImageStorageException("BytesPerSector must be initialized before calling GuidPartitionTable.ReadFromStream.");
			}
			if (readPrimaryTable)
			{
				ProtectiveMbr = new MasterBootRecord(Logger, BytesPerSector, isDesktopImage);
				ProtectiveMbr.ReadFromStream(stream, MasterBootRecord.MbrParseType.Normal);
			}
			else
			{
				position = stream.Length - BytesPerSector;
			}
			stream.Position = position;
			Header = new GuidPartitionTableHeader(Logger);
			Header.ReadFromStream(stream, BytesPerSector);
			stream.Position = (long)Header.PartitionEntryStartSector * (long)BytesPerSector;
			int num = (int)Math.Max(Header.PartitionEntryCount, 16384u / Header.PartitionEntrySizeInBytes);
			Entries = new List<GuidPartitionTableEntry>(num);
			for (int i = 0; i < num; i++)
			{
				GuidPartitionTableEntry guidPartitionTableEntry = new GuidPartitionTableEntry(Logger);
				guidPartitionTableEntry.ReadFromStream(stream, (int)Header.PartitionEntrySizeInBytes);
				Entries.Add(guidPartitionTableEntry);
			}
		}

		public void WriteToStream(Stream stream, bool fPrimaryTable, bool onlyAllocateDefinedGptEntries)
		{
			long position = BytesPerSector;
			if (fPrimaryTable)
			{
				if (ProtectiveMbr == null)
				{
					throw new ImageStorageException("The GuidPartitionTable protective MBR is null.");
				}
				ProtectiveMbr.WriteToStream(stream, false);
			}
			else
			{
				position = stream.Length - BytesPerSector;
			}
			if (Header == null)
			{
				throw new ImageStorageException("The GuidPartitionTable header is null.");
			}
			stream.Position = position;
			if (onlyAllocateDefinedGptEntries)
			{
				Header.PartitionEntryCount = 4096u / Header.PartitionEntrySizeInBytes;
			}
			else
			{
				Header.PartitionEntryCount = Math.Max(Header.PartitionEntryCount, 16384u / Header.PartitionEntrySizeInBytes);
			}
			Header.PartitionEntryArrayCrc32 = ComputePartitionEntryCrc(Header.PartitionEntryCount);
			Header.FixHeaderCrc(BytesPerSector);
			Header.WriteToStream(stream, BytesPerSector);
			stream.Position = (long)Header.PartitionEntryStartSector * (long)BytesPerSector;
			foreach (GuidPartitionTableEntry entry in Entries)
			{
				entry.WriteToStream(stream, (int)Header.PartitionEntrySizeInBytes);
			}
		}

		public void LogInfo(ushort indentLevel = 0)
		{
			ProtectiveMbr.LogInfo(Logger, indentLevel);
			Header.LogInfo(indentLevel);
			Logger.LogInfo("");
			Logger.LogInfo("Partition Entry Array");
			foreach (GuidPartitionTableEntry entry in Entries)
			{
				entry.LogInfo((ushort)(indentLevel + 2));
			}
		}

		public Guid SetEntryId(string partitionName, Guid partitionId)
		{
			Guid empty = Guid.Empty;
			foreach (GuidPartitionTableEntry entry in Entries)
			{
				if (string.CompareOrdinal(entry.PartitionName.Split(default(char))[0], partitionName) == 0)
				{
					empty = entry.PartitionId;
					entry.PartitionId = partitionId;
					return empty;
				}
			}
			return empty;
		}

		public void RemoveEntry(string partitionName)
		{
			foreach (GuidPartitionTableEntry entry in Entries)
			{
				if (string.CompareOrdinal(entry.PartitionName.Split(default(char))[0], partitionName) == 0)
				{
					entry.Clean();
					break;
				}
			}
		}

		public uint ComputePartitionEntryCrc()
		{
			return ComputePartitionEntryCrc(Header.PartitionEntryCount);
		}

		public uint ComputePartitionEntryCrc(uint partitionEntryCount)
		{
			CRC32 cRC = new CRC32();
			MemoryStream memoryStream = new MemoryStream();
			uint num = 0u;
			foreach (GuidPartitionTableEntry entry in Entries)
			{
				if (++num > partitionEntryCount)
				{
					break;
				}
				entry.WriteToStream(memoryStream, (int)Header.PartitionEntrySizeInBytes);
			}
			byte[] array = cRC.ComputeHash(memoryStream.GetBuffer());
			return (uint)((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
		}

		public void ValidatePartitionEntryCrc()
		{
			uint num = ComputePartitionEntryCrc();
			if (Header.PartitionEntryArrayCrc32 != num)
			{
				throw new ImageStorageException($"The partition entry array CRC is invalid.  Actual: {Header.PartitionEntryArrayCrc32:x} Expected: {num:x}.");
			}
		}

		public void NormalizeGptIds(out Guid originalSystemPartitionId)
		{
			Header.DiskId = ImageConstants.SYSTEM_STORE_GUID;
			SetEntryId(ImageConstants.MAINOS_PARTITION_NAME, ImageConstants.MAINOS_PARTITION_ID);
			SetEntryId(ImageConstants.MMOS_PARTITION_NAME, ImageConstants.MMOS_PARTITION_ID);
			originalSystemPartitionId = SetEntryId(ImageConstants.SYSTEM_PARTITION_NAME, ImageConstants.SYSTEM_PARTITION_ID);
		}

		public void RandomizeGptIds()
		{
			Header.DiskId = Guid.NewGuid();
			SetEntryId(ImageConstants.MAINOS_PARTITION_NAME, Guid.NewGuid());
			SetEntryId(ImageConstants.MMOS_PARTITION_NAME, Guid.NewGuid());
			SetEntryId(ImageConstants.SYSTEM_PARTITION_NAME, Guid.NewGuid());
		}

		public void FixCrcs()
		{
			Header.PartitionEntryArrayCrc32 = ComputePartitionEntryCrc();
			Header.FixHeaderCrc(BytesPerSector);
		}

		public void ValidateCrcs()
		{
			ValidatePartitionEntryCrc();
			Header.ValidateHeaderCrc(BytesPerSector);
		}

		public GuidPartitionTableEntry GetEntry(string partitionName)
		{
			GuidPartitionTableEntry result = null;
			for (int i = 0; i < Entries.Count; i++)
			{
				if (string.Compare(Entries[i].PartitionName, partitionName, true, CultureInfo.InvariantCulture) == 0)
				{
					result = Entries[i];
					break;
				}
			}
			return result;
		}

		public static bool IsGuidPartitionStyle(List<DataBlockEntry> blockEntries, int bytesPerSector, int bytesPerBlock)
		{
			int num = bytesPerBlock / bytesPerSector;
			DataBlockEntry dataBlockEntry = null;
			foreach (DataBlockEntry blockEntry in blockEntries)
			{
				for (int i = 0; i < blockEntry.BlockLocationsOnDisk.Count; i++)
				{
					if (blockEntry.BlockLocationsOnDisk[i].AccessMethod == DiskLocation.DiskAccessMethod.DiskBegin && blockEntry.BlockLocationsOnDisk[i].BlockIndex == 0)
					{
						dataBlockEntry = blockEntry;
						break;
					}
				}
			}
			return false;
		}
	}
}
