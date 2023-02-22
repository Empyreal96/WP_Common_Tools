using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class BlockAllocationTable
	{
		private uint[] _blockAllocationTable;

		public uint this[uint index]
		{
			get
			{
				return _blockAllocationTable[index];
			}
			set
			{
				_blockAllocationTable[index] = value;
			}
		}

		public ulong SizeInBytes => (ulong)_blockAllocationTable.Length * (ulong)Marshal.SizeOf(typeof(uint));

		public uint EntryCount => (uint)_blockAllocationTable.Length;

		public BlockAllocationTable(uint batSize)
		{
			uint num = VhdCommon.Round(batSize * (uint)Marshal.SizeOf(typeof(uint)), VhdCommon.VHDSectorSize) / (uint)Marshal.SizeOf(typeof(uint));
			_blockAllocationTable = new uint[num];
			for (int i = 0; i < batSize; i++)
			{
				_blockAllocationTable[i] = uint.MaxValue;
			}
		}

		public void Write(FileStream writer)
		{
			uint[] blockAllocationTable = _blockAllocationTable;
			for (int i = 0; i < blockAllocationTable.Length; i++)
			{
				uint structure = VhdCommon.Swap32(blockAllocationTable[i]);
				writer.WriteStruct(ref structure);
			}
		}

		public void Read(FileStream reader)
		{
			for (int i = 0; i < _blockAllocationTable.Length; i++)
			{
				uint data = reader.ReadStruct<uint>();
				_blockAllocationTable[i] = VhdCommon.Swap32(data);
			}
		}
	}
}
