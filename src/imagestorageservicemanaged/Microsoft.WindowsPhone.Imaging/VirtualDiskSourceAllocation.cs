using System;
using System.Reflection;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class VirtualDiskSourceAllocation : ISourceAllocation, IDisposable
	{
		private string _virtualDiskPath;

		private DynamicHardDisk _virtualDisk;

		private uint _sectorsPerVirtualBlock;

		private bool _alreadyDisposed;

		public VirtualDiskSourceAllocation(string virtualDiskPath, uint alignmentSize)
		{
			_virtualDiskPath = virtualDiskPath;
			_virtualDisk = new DynamicHardDisk(virtualDiskPath);
			_sectorsPerVirtualBlock = _virtualDisk.BlockSize / _virtualDisk.SectorSize;
			if (_virtualDisk.BlockSize % alignmentSize != 0)
			{
				throw new ImageStorageException($"{MethodBase.GetCurrentMethod().Name}: The virtual disk allocation size (0x{_virtualDisk.BlockSize:x}) is not a multiple of the given alignment size (0x{alignmentSize:x}).");
			}
		}

		public bool BlockIsAllocated(ulong diskByteOffset)
		{
			uint index = (uint)(diskByteOffset / _virtualDisk.SectorSize / _sectorsPerVirtualBlock);
			return _virtualDisk.AllocationTable[index] != uint.MaxValue;
		}

		public uint GetAllocationSize()
		{
			return _virtualDisk.BlockSize;
		}

		~VirtualDiskSourceAllocation()
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
			if (_alreadyDisposed)
			{
				return;
			}
			if (isDisposing)
			{
				_virtualDiskPath = null;
				if (_virtualDisk != null)
				{
					_virtualDisk.Dispose();
					_virtualDisk = null;
				}
			}
			_alreadyDisposed = true;
		}
	}
}
