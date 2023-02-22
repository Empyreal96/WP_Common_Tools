using System;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class InputStore
	{
		[XmlArrayItem(ElementName = "Partition", Type = typeof(InputPartition), IsNullable = false)]
		[XmlArray]
		public InputPartition[] Partitions;

		public string Id { get; set; }

		public string StoreType { get; set; }

		public string DevicePath { get; set; }

		[CLSCompliant(false)]
		public uint SizeInSectors { get; set; }

		public bool OnlyAllocateDefinedGptEntries { get; set; }

		public InputStore()
		{
		}

		public InputStore(string storeType)
		{
			StoreType = storeType;
		}

		public bool IsMainOSStore()
		{
			return string.CompareOrdinal(StoreType, "MainOSStore") == 0;
		}
	}
}
