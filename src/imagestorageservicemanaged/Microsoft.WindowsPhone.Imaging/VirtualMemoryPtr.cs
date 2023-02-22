using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	public sealed class VirtualMemoryPtr : SafeHandle
	{
		private readonly IntPtr _allocatedPointer;

		private readonly UIntPtr _memorySize;

		private bool _disposed;

		public IntPtr AllocatedPointer => _allocatedPointer;

		[CLSCompliant(false)]
		public uint MemorySize => (uint)_memorySize;

		public override bool IsInvalid => _disposed;

		[CLSCompliant(false)]
		public VirtualMemoryPtr(uint memorySize)
			: base(IntPtr.Zero, true)
		{
			_memorySize = (UIntPtr)memorySize;
			try
			{
				_allocatedPointer = Win32Exports.VirtualAlloc(_memorySize, Win32Exports.AllocationType.MEM_COMMIT | Win32Exports.AllocationType.MEM_RESERVE, Win32Exports.MemoryProtection.PAGE_READWRITE);
			}
			catch (Win32ExportException innerException)
			{
				throw new ImageStorageException("Unable to create the virtual memory pointer.", innerException);
			}
		}

		public static implicit operator IntPtr(VirtualMemoryPtr virtualMemoryPointer)
		{
			return virtualMemoryPointer.AllocatedPointer;
		}

		protected override bool ReleaseHandle()
		{
			if (!_disposed)
			{
				_disposed = true;
				GC.SuppressFinalize(this);
				try
				{
					Win32Exports.VirtualFree(AllocatedPointer, Win32Exports.FreeType.MEM_RELEASE);
				}
				catch (Win32ExportException arg)
				{
					throw new ImageStorageException(string.Format("Unable to free the virtual memory pointer.", arg));
				}
			}
			return true;
		}
	}
}
