using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.Imaging
{
	public sealed class NativeServiceHandle : SafeHandle
	{
		private readonly IntPtr _serviceHandle;

		private bool _disposed;

		public IntPtr ServiceHandle => _serviceHandle;

		public override bool IsInvalid => _disposed;

		public NativeServiceHandle(LogFunction logError)
			: base(IntPtr.Zero, true)
		{
			_serviceHandle = NativeImaging.CreateImageStorageService(logError);
			if (_serviceHandle == IntPtr.Zero)
			{
				throw new ImageStorageException("Unable to create the image storage service.");
			}
		}

		public static implicit operator IntPtr(NativeServiceHandle virtualServiceHandle)
		{
			return virtualServiceHandle._serviceHandle;
		}

		protected override bool ReleaseHandle()
		{
			if (!_disposed)
			{
				_disposed = true;
				GC.SuppressFinalize(this);
				if (_serviceHandle != IntPtr.Zero)
				{
					NativeImaging.CloseImageStorageService(_serviceHandle);
				}
			}
			return true;
		}
	}
}
