using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "DeviceLayout", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class DeviceLayoutValidationInput
	{
		private InputValidationPartition[] _partitions;

		private string[] _excludedScopes;

		[XmlArrayItem(ElementName = "Partition", Type = typeof(InputValidationPartition), IsNullable = false)]
		[XmlArray]
		public InputValidationPartition[] Partitions
		{
			get
			{
				return _partitions;
			}
			set
			{
				_partitions = value;
			}
		}

		public string Scope { get; set; }

		public uint RulesSectorSize { get; set; }

		[XmlArrayItem(ElementName = "Scope", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public string[] ExcludedScopes
		{
			get
			{
				return _excludedScopes;
			}
			set
			{
				_excludedScopes = value;
			}
		}

		public string SectorSize { get; set; }

		public string ChunkSize { get; set; }

		public string DefaultPartitionByteAlignment { get; set; }
	}
}
