using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization.XML
{
	[XmlRoot(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class DataAssets
	{
		[XmlAttribute]
		public CustomizationDataAssetType Type { get; set; }

		[XmlElement(ElementName = "DataAsset")]
		public List<DataAsset> Items { get; set; }

		public DataAssets()
		{
			Items = new List<DataAsset>();
		}

		public DataAssets(CustomizationDataAssetType type)
			: this()
		{
			Type = type;
		}
	}
}
