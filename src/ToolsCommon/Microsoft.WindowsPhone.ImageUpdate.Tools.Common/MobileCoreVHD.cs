using System;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class MobileCoreVHD : MobileCoreImage
	{
		private IntPtr _hndlVirtDisk = IntPtr.Zero;

		private static readonly object _lockObj = new object();

		private const int SLEEP_1000 = 1000;

		private const int MAX_RETRY = 3;

		internal MobileCoreVHD(string path)
			: base(path)
		{
		}

		private void MountWithRetry(bool readOnly)
		{
			if (base.IsMounted)
			{
				return;
			}
			int num = 0;
			int num2 = 3;
			bool flag = false;
			do
			{
				flag = false;
				try
				{
					MountVHD(readOnly);
				}
				catch (Exception)
				{
					num++;
					flag = num < num2;
					if (!flag)
					{
						throw;
					}
					Thread.Sleep(1000);
				}
			}
			while (flag);
			base.IsMounted = true;
		}

		public override void MountReadOnly()
		{
			bool readOnly = true;
			MountWithRetry(readOnly);
		}

		public override void Mount()
		{
			bool readOnly = false;
			MountWithRetry(readOnly);
		}

		private void MountVHD(bool readOnly)
		{
			lock (_lockObj)
			{
				m_partitions.Clear();
				try
				{
					_hndlVirtDisk = CommonUtils.MountVHD(m_mobileCoreImagePath, readOnly);
					int DiskPathSizeInBytes = 1024;
					StringBuilder stringBuilder = new StringBuilder(DiskPathSizeInBytes);
					int virtualDiskPhysicalPath = VirtualDiskLib.GetVirtualDiskPhysicalPath(_hndlVirtDisk, ref DiskPathSizeInBytes, stringBuilder);
					if (0 < virtualDiskPhysicalPath)
					{
						throw new Win32Exception(virtualDiskPhysicalPath);
					}
					m_partitions.PopulateFromPhysicalDeviceId(stringBuilder.ToString());
					if (m_partitions.Count == 0)
					{
						throw new IUException("Could not retrieve logical drive information for {0}", stringBuilder);
					}
				}
				catch (Exception)
				{
					Unmount();
					throw;
				}
			}
		}

		public override void Unmount()
		{
			lock (_lockObj)
			{
				CommonUtils.DismountVHD(_hndlVirtDisk);
				_hndlVirtDisk = IntPtr.Zero;
				m_partitions.Clear();
				base.IsMounted = false;
			}
		}
	}
}
