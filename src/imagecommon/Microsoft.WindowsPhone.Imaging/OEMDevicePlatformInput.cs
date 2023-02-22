using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "OEMDevicePlatform", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class OEMDevicePlatformInput
	{
		[XmlElement("DevicePlatformID")]
		public string DevicePlatformID;

		private string[] _idArray;

		[XmlArrayItem(ElementName = "Name")]
		[XmlArray("CompressedPartitions")]
		public string[] CompressedPartitions;

		[XmlArrayItem(ElementName = "Name")]
		[XmlArray("UncompressedPartitions")]
		public string[] UncompressedPartitions;

		[CLSCompliant(false)]
		public uint MinSectorCount;

		[CLSCompliant(false)]
		public uint AdditionalMainOSFreeSectorsRequest;

		[CLSCompliant(false)]
		public uint MMOSPartitionTotalSectorsOverride;

		[CLSCompliant(false)]
		[XmlElement("MainOSRTCDataReservedSectors")]
		public uint MainOSRTCDataReservedSectors;

		[XmlElement("Rules")]
		public InputRules Rules;

		[XmlArrayItem(ElementName = "ID")]
		[XmlArray("DevicePlatformIDs")]
		public string[] DevicePlatformIDs
		{
			get
			{
				if (DevicePlatformID != null && _idArray != null)
				{
					throw new ImageCommonException("Please specify either a DevicePlatformID or a group of DevicePlatformIDs in the device platform package, but not both.");
				}
				if (DevicePlatformID == null && _idArray == null)
				{
					throw new ImageCommonException("Please specify either a DevicePlatformID or a group of DevicePlatformIDs in the device platform package. No platform ID is currently present.");
				}
				if (DevicePlatformID != null)
				{
					return new string[1] { DevicePlatformID };
				}
				return _idArray;
			}
			set
			{
				_idArray = value;
			}
		}
	}
}
