using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate/v2")]
	[XmlRoot(ElementName = "DeviceLayout", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate/v2", IsNullable = false)]
	public class DeviceLayoutInputv2
	{
		[XmlArrayItem(ElementName = "Store", Type = typeof(InputStore), IsNullable = false)]
		[XmlArray]
		public InputStore[] Stores;

		private uint _chunkSize = 256u;

		[XmlElement("SectorSize")]
		public uint SectorSize { get; set; }

		[XmlElement("ChunkSize")]
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

		public InputStore MainOSStore => Stores.FirstOrDefault((InputStore x) => x.IsMainOSStore());
	}
}
