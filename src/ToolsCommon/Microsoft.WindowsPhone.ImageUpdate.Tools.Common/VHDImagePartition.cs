using System;
using System.Globalization;
using System.IO;
using System.Management;
using System.Threading;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class VHDImagePartition : ImagePartition
	{
		private const string WMI_GETPARTITIONS_QUERY = "Select * from Win32_DiskPartition where Name='{0}'";

		private const string WMI_DISKPARTITION_CLASS = "Win32_DiskPartition";

		private const string WMI_LOGICALDISK_CLASS = "Win32_LogicalDisk";

		private const string STR_NAME = "Name";

		private const int MAX_RETRY = 10;

		private const int SLEEP_500 = 500;

		public VHDImagePartition(string deviceId, string partitionId)
		{
			base.PhysicalDeviceId = deviceId;
			base.Name = partitionId;
			string empty = string.Empty;
			int num = 10;
			bool flag = false;
			int num2 = 0;
			do
			{
				flag = false;
				empty = GetLogicalDriveFromWMI(deviceId, partitionId);
				if (string.IsNullOrEmpty(empty))
				{
					Console.WriteLine("  ImagePartition.GetLogicalDriveFromWMI({0}, {1}) not found, sleeping...", deviceId, partitionId);
					num2++;
					flag = num2 < num;
					Thread.Sleep(500);
				}
			}
			while (flag);
			if (string.IsNullOrEmpty(empty))
			{
				throw new IUException("Failed to retrieve logical drive name of partition {0} using WMI", partitionId);
			}
			if (string.Compare(empty, "NONE", true, CultureInfo.InvariantCulture) != 0)
			{
				base.MountedDriveInfo = new DriveInfo(Path.GetPathRoot(empty));
				base.Root = base.MountedDriveInfo.RootDirectory.FullName;
			}
		}

		private string GetLogicalDriveFromWMI(string deviceId, string partitionId)
		{
			string result = string.Empty;
			bool flag = false;
			using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher($"Select * from Win32_DiskPartition where Name='{partitionId}'"))
			{
				foreach (ManagementObject item in managementObjectSearcher.Get())
				{
					Console.WriteLine("  ImagePartition.GetLogicalDriveFromWMI: Path={0}", item.Path.ToString());
					if (string.Compare(item.GetPropertyValue("Type").ToString(), "unknown", true, CultureInfo.InvariantCulture) == 0)
					{
						return "NONE";
					}
					using (ManagementObjectCollection.ManagementObjectEnumerator managementObjectEnumerator2 = new ManagementObjectSearcher(new RelatedObjectQuery(item.Path.ToString(), "Win32_LogicalDisk")).Get().GetEnumerator())
					{
						if (managementObjectEnumerator2.MoveNext())
						{
							result = ((ManagementObject)managementObjectEnumerator2.Current).GetPropertyValue("Name").ToString();
							flag = true;
						}
					}
					if (flag)
					{
						return result;
					}
				}
				return result;
			}
		}
	}
}
