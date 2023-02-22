using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.WindowsPhone.Imaging
{
	public sealed class SafeVolumeHandle : SafeHandle
	{
		private ImageStorage _storage;

		private SafeFileHandle _volumeHandle;

		private bool _disposed;

		public SafeFileHandle VolumeHandle => _volumeHandle;

		public override bool IsInvalid => _disposed;

		public SafeVolumeHandle(ImageStorage storage, string partitionName)
			: base(IntPtr.Zero, true)
		{
			_storage = storage;
			_volumeHandle = storage.OpenVolumeHandle(partitionName);
		}

		public static implicit operator IntPtr(SafeVolumeHandle safeVolumeHandle)
		{
			return safeVolumeHandle._volumeHandle.DangerousGetHandle();
		}

		protected override bool ReleaseHandle()
		{
			if (!_disposed)
			{
				_disposed = true;
				GC.SuppressFinalize(this);
				if (_volumeHandle != null)
				{
					_volumeHandle.Close();
					_volumeHandle = null;
				}
				_storage = null;
			}
			return true;
		}
	}
}
