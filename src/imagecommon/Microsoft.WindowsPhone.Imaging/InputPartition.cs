using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class InputPartition
	{
		[CLSCompliant(false)]
		public uint MinFreeSectors;

		[CLSCompliant(false)]
		public uint GeneratedFileOverheadSectors;

		private string _primaryPartition;

		public string Name { get; set; }

		public string Type { get; set; }

		public string Id { get; set; }

		public bool ReadOnly { get; set; }

		public bool AttachDriveLetter { get; set; }

		public bool Hidden { get; set; }

		public bool Bootable { get; set; }

		[CLSCompliant(false)]
		public uint TotalSectors { get; set; }

		public bool UseAllSpace { get; set; }

		public string FileSystem { get; set; }

		public string UpdateType { get; set; }

		public bool Compressed { get; set; }

		[XmlElement("RequiresCompression")]
		public bool RequiresCompression { get; set; }

		public string PrimaryPartition
		{
			get
			{
				if (string.IsNullOrEmpty(_primaryPartition))
				{
					return Name;
				}
				return _primaryPartition;
			}
			set
			{
				_primaryPartition = value;
			}
		}

		public bool RequiredToFlash { get; set; }

		public bool SingleSectorAlignment { get; set; }

		[CLSCompliant(false)]
		[XmlIgnore]
		public uint ByteAlignment { get; set; }

		[XmlElement("ByteAlignment")]
		public string ByteAlignmentString
		{
			get
			{
				return ByteAlignment.ToString(CultureInfo.InvariantCulture);
			}
			set
			{
				uint value2 = 0u;
				if (!InputHelpers.StringToUint(value, out value2))
				{
					throw new ImageCommonException(string.Format("Partition {0}'s byte alignment cannot be parsed.", string.IsNullOrEmpty(Name) ? "Unknown" : Name));
				}
				ByteAlignment = value2;
			}
		}

		[CLSCompliant(false)]
		[XmlIgnore]
		public uint ClusterSize { get; set; }

		[XmlElement("ClusterSize")]
		public string ClusterSizeString
		{
			get
			{
				return ClusterSize.ToString(CultureInfo.InvariantCulture);
			}
			set
			{
				uint value2 = 0u;
				if (!InputHelpers.StringToUint(value, out value2))
				{
					throw new ImageCommonException(string.Format("Partition {0}'s cluster size cannot be parsed.", string.IsNullOrEmpty(Name) ? "Unknown" : Name));
				}
				ClusterSize = value2;
			}
		}
	}
}
