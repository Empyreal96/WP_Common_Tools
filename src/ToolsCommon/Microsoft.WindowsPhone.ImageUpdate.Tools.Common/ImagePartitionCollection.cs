using System.Collections.ObjectModel;
using System.Management;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class ImagePartitionCollection : Collection<ImagePartition>
	{
		private const string WMI_GETPARTITIONS_QUERY = "\\\\.\\root\\cimv2:Win32_DiskDrive.DeviceID='{0}'";

		private const string WMI_DISKPARTITION_CLASS = "Win32_DiskPartition";

		private const string STR_NAME = "Name";

		public void PopulateFromPhysicalDeviceId(string deviceId)
		{
			foreach (ManagementObject item in new ManagementObjectSearcher(new RelatedObjectQuery($"\\\\.\\root\\cimv2:Win32_DiskDrive.DeviceID='{deviceId}'", "Win32_DiskPartition")).Get())
			{
				Add(new VHDImagePartition(deviceId, item.GetPropertyValue("Name").ToString()));
			}
		}
	}
}
