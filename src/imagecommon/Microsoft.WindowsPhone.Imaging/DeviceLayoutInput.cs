using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "DeviceLayout", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class DeviceLayoutInput
	{
		[XmlArrayItem(ElementName = "Partition", Type = typeof(InputPartition), IsNullable = false)]
		[XmlArray]
		public InputPartition[] Partitions;

		private uint _sectorSize;

		private uint _chunkSize = 256u;

		[XmlElement("SectorSize")]
		[CLSCompliant(false)]
		public uint SectorSize
		{
			get
			{
				return _sectorSize;
			}
			set
			{
				_sectorSize = value;
			}
		}

		[XmlElement("ChunkSize")]
		[CLSCompliant(false)]
		public uint ChunkSize
		{
			get
			{
				return _chunkSize;
			}
			set
			{
				_chunkSize = value;
			}
		}

		[CLSCompliant(false)]
		[XmlIgnore]
		public uint DefaultPartitionByteAlignment { get; set; }

		[XmlElement("VersionTag")]
		public string VersionTag { get; set; }

		[XmlElement("DefaultPartitionByteAlignment")]
		public string DefaultPartitionByteAlignmentAsString
		{
			get
			{
				return DefaultPartitionByteAlignment.ToString(CultureInfo.InvariantCulture);
			}
			set
			{
				uint value2 = 0u;
				if (!InputHelpers.StringToUint(value, out value2))
				{
					throw new ImageCommonException($"The default byte alignment cannot be parsed: {value}");
				}
				DefaultPartitionByteAlignment = value2;
			}
		}
	}
}
