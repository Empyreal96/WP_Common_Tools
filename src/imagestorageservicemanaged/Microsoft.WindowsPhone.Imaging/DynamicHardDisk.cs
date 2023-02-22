using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class DynamicHardDisk : IVirtualHardDisk, IDisposable
	{
		private FileStream _fileStream;

		private VhdFooter _footer;

		private VhdHeader _header;

		private BlockAllocationTable _blockAllocationTable;

		private ulong _tableOffset;

		private ulong _fileSize;

		private ulong _footerOffset;

		public static byte[] emptySectorBuffer = new byte[VhdCommon.VHDSectorSize];

		private bool _alreadyDisposed;

		public uint SectorSize => VhdCommon.VHDSectorSize;

		public ulong SectorCount { get; private set; }

		public BlockAllocationTable AllocationTable => _blockAllocationTable;

		public uint BlockSize => _header.BlockSize;

		public uint BlockBitmapSectorCount
		{
			get
			{
				uint num = BlockSize / SectorSize;
				uint num2 = num / 8u;
				if (num % 8u != 0)
				{
					num2++;
				}
				uint num3 = num2 / SectorSize;
				if (num2 % SectorSize != 0)
				{
					num3++;
				}
				return num3;
			}
		}

		private uint SectorsPerBlock => VhdCommon.DynamicVHDBlockSize / SectorSize;

		public DynamicHardDisk(string fileName, ulong sectorCount)
		{
			ulong num = 0uL;
			_fileSize = sectorCount * SectorSize;
			_fileStream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
			SectorCount = sectorCount;
			_footer = new VhdFooter(_fileSize, VhdType.Dynamic, (ulong)Marshal.SizeOf(typeof(VhdFooter)));
			WriteVHDFooter(num);
			num += (ulong)Marshal.SizeOf(typeof(VhdFooter));
			_header = new VhdHeader(_fileSize);
			WriteVHDHeader(num);
			num = (_tableOffset = num + (ulong)Marshal.SizeOf(typeof(VhdHeader)));
			_blockAllocationTable = new BlockAllocationTable(_header.MaxTableEntries);
			WriteBlockAllocationTable(num);
			num = (_footerOffset = num + _blockAllocationTable.SizeInBytes);
			WriteVHDFooter(num);
			num += (ulong)Marshal.SizeOf(typeof(VhdFooter));
		}

		public DynamicHardDisk(string existingFile, bool addWriteAccess = false)
		{
			FileAccess access = FileAccess.Read;
			if (addWriteAccess)
			{
				access = FileAccess.ReadWrite;
			}
			_fileStream = new FileStream(existingFile, FileMode.Open, access, FileShare.ReadWrite);
			_footer = VhdFooter.Read(_fileStream);
			_fileSize = _footer.CurrentSize;
			SectorCount = _fileSize / VhdCommon.VHDSectorSize;
			_fileStream.Position = (long)_footer.DataOffset;
			_header = VhdHeader.Read(_fileStream);
			_tableOffset = _header.TableOffset;
			_fileStream.Position = (long)_header.TableOffset;
			_blockAllocationTable = new BlockAllocationTable(_header.MaxTableEntries);
			_blockAllocationTable.Read(_fileStream);
		}

		public void Close()
		{
			if (_fileStream != null)
			{
				_fileStream.Close();
				_fileStream = null;
			}
		}

		public void FlushFile()
		{
			WriteVHDFooter(_footerOffset);
			_fileStream.Flush();
		}

		public bool SectorIsAllocated(ulong sectorIndex)
		{
			uint index = (uint)(sectorIndex / SectorsPerBlock);
			if (-1 == (int)_blockAllocationTable[index])
			{
				return false;
			}
			return true;
		}

		public void ReadSector(ulong sector, byte[] buffer, uint offset)
		{
			if (sector >= SectorCount)
			{
				throw new ArgumentException("Sector is out of bound", "sector");
			}
			if (buffer.Length - offset < SectorSize)
			{
				throw new ArgumentException("The buffer, from the given offset, is smaller than the sector size.", "offset");
			}
			uint index = (uint)(sector / SectorsPerBlock);
			uint num = (uint)(sector % SectorsPerBlock);
			if (-1 == (int)_blockAllocationTable[index])
			{
				Array.Copy(emptySectorBuffer, 0L, buffer, offset, SectorSize);
				return;
			}
			byte[] buffer2 = new byte[SectorSize];
			_fileStream.Seek((int)(_blockAllocationTable[index] * SectorSize), SeekOrigin.Begin);
			_fileStream.Read(buffer2, 0, (int)SectorSize);
			uint num2 = num / 8u;
			_fileStream.Seek((_blockAllocationTable[index] + num + BlockBitmapSectorCount) * SectorSize, SeekOrigin.Begin);
			_fileStream.Read(buffer, (int)offset, (int)SectorSize);
		}

		public void WriteSector(ulong sector, byte[] buffer, uint offset)
		{
			if (sector >= SectorCount)
			{
				throw new ArgumentException("Sector is out of bound", "sector");
			}
			if (buffer.Length - offset < SectorSize)
			{
				throw new ArgumentException("The buffer, from the given offset, is smaller than the sector size.", "offset");
			}
			uint index = (uint)(sector / SectorsPerBlock);
			uint num = (uint)(sector % SectorsPerBlock);
			if (-1 == (int)_blockAllocationTable[index])
			{
				_blockAllocationTable[index] = (uint)(_footerOffset / SectorSize);
				WriteBlockAllocationTable(_tableOffset);
				_fileStream.Seek((long)_footerOffset, SeekOrigin.Begin);
				_fileStream.Write(emptySectorBuffer, 0, (int)SectorSize);
				_footerOffset += SectorSize + VhdCommon.DynamicVHDBlockSize;
			}
			byte[] array = new byte[SectorSize];
			_fileStream.Seek((int)(_blockAllocationTable[index] * SectorSize), SeekOrigin.Begin);
			_fileStream.Read(array, 0, (int)SectorSize);
			uint num2 = num / 8u;
			byte b = (byte)(num % 8u);
			array[num2] = (byte)(array[num2] | (1 << (int)b));
			_fileStream.Seek((int)(_blockAllocationTable[index] * SectorSize), SeekOrigin.Begin);
			_fileStream.Write(array, 0, (int)SectorSize);
			_fileStream.Seek((long)(_blockAllocationTable[index] + num + 1) * (long)SectorSize, SeekOrigin.Begin);
			_fileStream.Write(buffer, (int)offset, (int)SectorSize);
		}

		~DynamicHardDisk()
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
			if (!_alreadyDisposed)
			{
				if (_fileStream != null)
				{
					_fileStream.Close();
					_fileStream = null;
				}
				_alreadyDisposed = true;
			}
		}

		private void WriteVHDFooter(ulong offset)
		{
			_fileStream.Seek((long)offset, SeekOrigin.Begin);
			_footer.Write(_fileStream);
		}

		private void WriteVHDHeader(ulong offset)
		{
			_fileStream.Seek((long)offset, SeekOrigin.Begin);
			_header.Write(_fileStream);
		}

		private void WriteBlockAllocationTable(ulong offset)
		{
			_fileStream.Seek((long)offset, SeekOrigin.Begin);
			_blockAllocationTable.Write(_fileStream);
		}
	}
}
