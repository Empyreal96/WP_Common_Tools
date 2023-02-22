using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.WindowsPhone.Imaging
{
	internal class DiskStreamSource : IBlockStreamSource, IDisposable
	{
		private SafeFileHandle _handle;

		private VirtualMemoryPtr _buffer;

		private uint _blockSize;

		private bool _alreadyDisposed;

		public long Length { get; private set; }

		public DiskStreamSource(SafeFileHandle diskHandle, uint blockSize)
		{
			_blockSize = blockSize;
			_handle = diskHandle;
			_buffer = new VirtualMemoryPtr(blockSize);
			ulong sectorCount = NativeImaging.GetSectorCount(IntPtr.Zero, _handle);
			uint sectorSize = NativeImaging.GetSectorSize(IntPtr.Zero, _handle);
			Length = (long)(sectorCount * sectorSize);
		}

		public void ReadBlock(uint blockIndex, byte[] buffer, int bufferIndex)
		{
			uint bytesRead = 0u;
			long newFileLocation = 0L;
			Win32Exports.SetFilePointerEx(_handle, blockIndex * (int)_blockSize, out newFileLocation, Win32Exports.MoveMethod.FILE_BEGIN);
			Win32Exports.ReadFile(_handle, _buffer, _blockSize, out bytesRead);
			Marshal.Copy(_buffer.AllocatedPointer, buffer, bufferIndex, (int)_blockSize);
		}

		~DiskStreamSource()
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
				if (_handle != null)
				{
					_handle = null;
				}
				if (_buffer != null)
				{
					_buffer.Dispose();
					_buffer = null;
				}
			}
			_alreadyDisposed = true;
		}
	}
}
