using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class MasterBootRecord
	{
		public enum MbrParseType
		{
			Normal,
			TruncateAllExtendedRecords,
			TruncateInvalidExtendedRecords
		}

		public const ushort Signature = 43605;

		public const int FirstEntryByteOffset = 446;

		public const ushort CodeAreaSize = 440;

		private readonly byte[] CodeData = new byte[440]
		{
			51, 192, 142, 208, 188, 0, 124, 142, 192, 142,
			216, 190, 0, 124, 191, 0, 6, 185, 0, 2,
			252, 243, 164, 80, 104, 28, 6, 203, 251, 185,
			4, 0, 189, 190, 7, 128, 126, 0, 0, 124,
			11, 15, 133, 14, 1, 131, 197, 16, 226, 241,
			205, 24, 136, 86, 0, 85, 198, 70, 17, 5,
			198, 70, 16, 0, 180, 65, 187, 170, 85, 205,
			19, 93, 114, 15, 129, 251, 85, 170, 117, 9,
			247, 193, 1, 0, 116, 3, 254, 70, 16, 102,
			96, 128, 126, 16, 0, 116, 38, 102, 104, 0,
			0, 0, 0, 102, 255, 118, 8, 104, 0, 0,
			104, 0, 124, 104, 1, 0, 104, 16, 0, 180,
			66, 138, 86, 0, 139, 244, 205, 19, 159, 131,
			196, 16, 158, 235, 20, 184, 1, 2, 187, 0,
			124, 138, 86, 0, 138, 118, 1, 138, 78, 2,
			138, 110, 3, 205, 19, 102, 97, 115, 28, 254,
			78, 17, 117, 12, 128, 126, 0, 128, 15, 132,
			138, 0, 178, 128, 235, 132, 85, 50, 228, 138,
			86, 0, 205, 19, 93, 235, 158, 129, 62, 254,
			125, 85, 170, 117, 110, 255, 118, 0, 232, 141,
			0, 117, 23, 250, 176, 209, 230, 100, 232, 131,
			0, 176, 223, 230, 96, 232, 124, 0, 176, 255,
			230, 100, 232, 117, 0, 251, 184, 0, 187, 205,
			26, 102, 35, 192, 117, 59, 102, 129, 251, 84,
			67, 80, 65, 117, 50, 129, 249, 2, 1, 114,
			44, 102, 104, 7, 187, 0, 0, 102, 104, 0,
			2, 0, 0, 102, 104, 8, 0, 0, 0, 102,
			83, 102, 83, 102, 85, 102, 104, 0, 0, 0,
			0, 102, 104, 0, 124, 0, 0, 102, 97, 104,
			0, 0, 7, 205, 26, 90, 50, 246, 234, 0,
			124, 0, 0, 205, 24, 160, 183, 7, 235, 8,
			160, 182, 7, 235, 3, 160, 181, 7, 50, 228,
			5, 0, 7, 139, 240, 172, 60, 0, 116, 9,
			187, 7, 0, 180, 14, 205, 16, 235, 242, 244,
			235, 253, 43, 201, 228, 100, 235, 0, 36, 2,
			224, 248, 36, 2, 195, 73, 110, 118, 97, 108,
			105, 100, 32, 112, 97, 114, 116, 105, 116, 105,
			111, 110, 32, 116, 97, 98, 108, 101, 0, 69,
			114, 114, 111, 114, 32, 108, 111, 97, 100, 105,
			110, 103, 32, 111, 112, 101, 114, 97, 116, 105,
			110, 103, 32, 115, 121, 115, 116, 101, 109, 0,
			77, 105, 115, 115, 105, 110, 103, 32, 111, 112,
			101, 114, 97, 116, 105, 110, 103, 32, 115, 121,
			115, 116, 101, 109, 0, 0, 0, 99, 123, 154
		};

		private IULogger _logger;

		private int _bytesPerSector;

		private uint _sectorIndex;

		private MasterBootRecord _primaryRecord;

		private MasterBootRecord _extendedRecord;

		private List<MbrPartitionEntry> _entries = new List<MbrPartitionEntry>();

		private MasterBootRecordMetadataPartition _metadataPartition;

		private byte[] _codeData = new byte[440];

		private bool _isDesktopImage;

		private MbrPartitionEntry _extendedEntry;

		public uint DiskSignature { get; set; }

		public uint DiskSectorCount { get; set; }

		public List<MbrPartitionEntry> PartitionEntries => _entries;

		public MasterBootRecord ExtendedRecord => _extendedRecord;

		private MasterBootRecord()
		{
		}

		private MasterBootRecord(MasterBootRecord primaryRecord)
		{
			_primaryRecord = primaryRecord;
			_bytesPerSector = _primaryRecord._bytesPerSector;
			_logger = _primaryRecord._logger;
		}

		public MasterBootRecord(IULogger logger, int bytesPerSector)
			: this(logger, bytesPerSector, false)
		{
		}

		public MasterBootRecord(IULogger logger, int bytesPerSector, bool isDesktopImage)
		{
			_isDesktopImage = isDesktopImage;
			_logger = logger;
			_bytesPerSector = bytesPerSector;
			_metadataPartition = new MasterBootRecordMetadataPartition(logger);
		}

		public bool ReadFromStream(Stream stream, MbrParseType parseType)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			bool flag = true;
			_sectorIndex = (uint)(stream.Position / _bytesPerSector);
			stream.Read(_codeData, 0, _codeData.Length);
			DiskSignature = binaryReader.ReadUInt32();
			stream.Position += 2L;
			for (int i = 0; i < 4; i++)
			{
				MbrPartitionEntry mbrPartitionEntry = new MbrPartitionEntry();
				mbrPartitionEntry.ReadFromStream(binaryReader);
				if (IsExtendedBootRecord())
				{
					mbrPartitionEntry.StartingSectorOffset = _sectorIndex;
				}
				if (mbrPartitionEntry.TypeIsContainer && parseType == MbrParseType.TruncateAllExtendedRecords)
				{
					mbrPartitionEntry.ZeroData();
				}
				if (mbrPartitionEntry.TypeIsContainer)
				{
					if (_extendedEntry != null)
					{
						_logger.LogWarning("{0}: The extended boot record at sector 0x{1:x} contains multiple extended boot records.", MethodBase.GetCurrentMethod().Name, _sectorIndex);
						if (IsExtendedBootRecord() && parseType == MbrParseType.TruncateInvalidExtendedRecords)
						{
							flag = false;
							break;
						}
						throw new ImageStorageException("There are multiple extended partition entries.");
					}
					long num = 0L;
					num = (IsExtendedBootRecord() ? ((mbrPartitionEntry.StartingSector + _primaryRecord._extendedEntry.StartingSector) * _bytesPerSector) : (mbrPartitionEntry.StartingSector * _bytesPerSector));
					if (mbrPartitionEntry.SectorCount == 0 || mbrPartitionEntry.StartingSector == 0)
					{
						_logger.LogWarning("{0}: The boot record at sector 0x{1:x} has an entry with a extended partition type, but the start sector or size is 0.", MethodBase.GetCurrentMethod().Name, _sectorIndex);
						mbrPartitionEntry.PartitionType = 0;
					}
					else if (num > stream.Length)
					{
						if (parseType != MbrParseType.TruncateInvalidExtendedRecords)
						{
							throw new ImageStorageException("There are multiple extended partition entries.");
						}
						_logger.LogDebug("{0}: The extended boot entry at sector 0x{1:x} points beyond the end of the stream.", MethodBase.GetCurrentMethod().Name, _sectorIndex);
						if (IsExtendedBootRecord())
						{
							flag = false;
							break;
						}
						mbrPartitionEntry.ZeroData();
					}
					else
					{
						_extendedEntry = mbrPartitionEntry;
					}
				}
				_entries.Add(mbrPartitionEntry);
			}
			if (!_isDesktopImage && binaryReader.ReadUInt16() != 43605)
			{
				if (!IsExtendedBootRecord() || parseType != MbrParseType.TruncateInvalidExtendedRecords)
				{
					throw new ImageStorageException("The MBR disk signature is invalid.");
				}
				_logger.LogDebug("{0}: The extended boot record at sector 0x{1:x} has an invalid MBR signature.", MethodBase.GetCurrentMethod().Name, _sectorIndex);
				flag = false;
			}
			if (stream.Position % _bytesPerSector != 0L)
			{
				stream.Position += _bytesPerSector - stream.Position % _bytesPerSector;
			}
			if (flag && !ReadExtendedPartitions(stream, parseType))
			{
				_extendedRecord = null;
				_extendedEntry.ZeroData();
			}
			ReadMetadataPartition(stream);
			return flag;
		}

		private bool ReadExtendedPartitions(Stream stream, MbrParseType parseType)
		{
			long position = stream.Position;
			bool result = true;
			if (_extendedEntry != null)
			{
				if (!IsExtendedBootRecord())
				{
					stream.Position = _extendedEntry.StartingSector * _bytesPerSector;
				}
				else
				{
					stream.Position = (_extendedEntry.StartingSector + _primaryRecord._extendedEntry.StartingSector) * _bytesPerSector;
				}
				_extendedRecord = new MasterBootRecord((_primaryRecord == null) ? this : _primaryRecord);
				result = _extendedRecord.ReadFromStream(stream, parseType);
			}
			return result;
		}

		public void WriteToStream(Stream stream, bool addCodeData)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			stream.Position = _sectorIndex * _bytesPerSector;
			if (!addCodeData)
			{
				stream.Write(_codeData, 0, 440);
			}
			else
			{
				stream.Write(CodeData, 0, CodeData.Length);
			}
			binaryWriter.Write(DiskSignature);
			stream.WriteByte(0);
			stream.WriteByte(0);
			foreach (MbrPartitionEntry entry in _entries)
			{
				entry.WriteToStream(binaryWriter);
			}
			binaryWriter.Write((ushort)43605);
			if (stream.Position % _bytesPerSector != 0L)
			{
				stream.Position += _bytesPerSector - stream.Position % _bytesPerSector;
			}
			if (_extendedRecord != null)
			{
				_extendedRecord.WriteToStream(stream, false);
			}
		}

		public void LogInfo(IULogger logger, ushort indentLevel = 0)
		{
			string text = new StringBuilder().Append(' ', indentLevel).ToString();
			if (IsValidProtectiveMbr())
			{
				logger.LogInfo(text + "Protective Master Boot Record");
			}
			else if (_primaryRecord == null)
			{
				logger.LogInfo(text + "Master Boot Record");
				logger.LogInfo(text + "  Disk Signature: 0x{0:x}", DiskSignature);
			}
			else
			{
				logger.LogInfo(text + "Extended Boot Record");
			}
			logger.LogInfo("");
			foreach (MbrPartitionEntry entry in _entries)
			{
				entry.LogInfo(logger, this, (ushort)(indentLevel + 2));
			}
			if (_extendedRecord != null)
			{
				if (!IsExtendedBootRecord())
				{
					indentLevel = (ushort)(indentLevel + 2);
				}
				_extendedRecord.LogInfo(logger, indentLevel);
			}
		}

		public bool IsValidProtectiveMbr()
		{
			for (int i = 0; i < _entries.Count; i++)
			{
				MbrPartitionEntry mbrPartitionEntry = _entries[i];
				if (i == 0)
				{
					if (mbrPartitionEntry.StartingSector != 1)
					{
						return false;
					}
					if (mbrPartitionEntry.SectorCount == 0)
					{
						return false;
					}
					if (mbrPartitionEntry.PartitionType != 238)
					{
						return false;
					}
				}
				else
				{
					if (mbrPartitionEntry.SectorCount != 0)
					{
						return false;
					}
					if (mbrPartitionEntry.StartingSector != 0)
					{
						return false;
					}
					if (mbrPartitionEntry.PartitionType != 0)
					{
						return false;
					}
				}
				if (mbrPartitionEntry.Bootable)
				{
					return false;
				}
			}
			return true;
		}

		public bool IsExtendedBootRecord()
		{
			return _primaryRecord != null;
		}

		public MbrPartitionEntry FindPartitionByType(byte partitionType)
		{
			foreach (MbrPartitionEntry entry in _entries)
			{
				if (entry.PartitionType == partitionType)
				{
					return entry;
				}
			}
			if (_extendedRecord != null)
			{
				return _extendedRecord.FindPartitionByType(partitionType);
			}
			return null;
		}

		public ulong FindPartitionOffset(string partitionName)
		{
			ulong result = 0uL;
			if (_metadataPartition != null)
			{
				foreach (MetadataPartitionEntry entry in _metadataPartition.Entries)
				{
					if (string.Compare(entry.Name, partitionName, true, CultureInfo.InvariantCulture) == 0)
					{
						return entry.DiskOffset;
					}
				}
				return result;
			}
			return result;
		}

		public MbrPartitionEntry FindPartitionByName(string partitionName)
		{
			ulong num = FindPartitionOffset(partitionName);
			if (num != 0)
			{
				return FindPartitionByName(partitionName, num);
			}
			return null;
		}

		private MbrPartitionEntry FindPartitionByName(string partitionName, ulong diskOffset)
		{
			foreach (MbrPartitionEntry entry in _entries)
			{
				if (entry.AbsoluteStartingSector * _bytesPerSector == (long)diskOffset)
				{
					return entry;
				}
			}
			if (_extendedRecord != null)
			{
				return _extendedRecord.FindPartitionByName(partitionName, diskOffset);
			}
			return null;
		}

		public string GetPartitionName(MbrPartitionEntry entry)
		{
			ulong num = (ulong)(entry.AbsoluteStartingSector * _bytesPerSector);
			MasterBootRecordMetadataPartition metadataPartition = _metadataPartition;
			if (IsExtendedBootRecord())
			{
				metadataPartition = _primaryRecord._metadataPartition;
			}
			if (metadataPartition != null)
			{
				foreach (MetadataPartitionEntry entry2 in metadataPartition.Entries)
				{
					if (entry2.DiskOffset == num)
					{
						return entry2.Name;
					}
				}
			}
			return string.Empty;
		}

		public long GetMetadataPartitionOffset()
		{
			long result = 0L;
			MbrPartitionEntry mbrPartitionEntry = FindPartitionByType(MasterBootRecordMetadataPartition.PartitonType);
			if (mbrPartitionEntry != null)
			{
				result = mbrPartitionEntry.AbsoluteStartingSector * _bytesPerSector;
			}
			return result;
		}

		public void ReadMetadataPartition(Stream stream)
		{
			if (_primaryRecord == null && !IsValidProtectiveMbr())
			{
				long metadataPartitionOffset = GetMetadataPartitionOffset();
				long position = stream.Position;
				if (metadataPartitionOffset > 0)
				{
					stream.Position = metadataPartitionOffset;
					_metadataPartition = new MasterBootRecordMetadataPartition(_logger);
					_metadataPartition.ReadFromStream(stream);
				}
				stream.Position = position;
			}
		}

		public byte[] GetCodeData()
		{
			return _codeData;
		}

		public void RemovePartition(string partitionName)
		{
			ulong num = FindPartitionOffset(partitionName);
			if (num == 0L)
			{
				throw new ImageStorageException($"Partition {partitionName} was not found in the MBR metadata partition.");
			}
			RemovePartition(partitionName, num);
		}

		private void RemovePartition(string partitionName, ulong partitionOffset)
		{
			bool flag = true;
			foreach (MbrPartitionEntry entry in _entries)
			{
				if (entry.AbsoluteStartingSector * _bytesPerSector == (long)partitionOffset)
				{
					entry.ZeroData();
					flag = false;
					break;
				}
			}
			if (flag)
			{
				if (_extendedRecord == null)
				{
					throw new ImageStorageException($"Partition {partitionName} was in the MBR metadata partition, but the boot record is not found.");
				}
				_extendedRecord.RemovePartition(partitionName, partitionOffset);
			}
		}
	}
}
