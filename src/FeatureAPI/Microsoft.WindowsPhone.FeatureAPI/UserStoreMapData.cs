using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class UserStoreMapData
	{
		[XmlAttribute("SourceDir")]
		public string SourceDir;

		[XmlAttribute("UserStoreDir")]
		public string UserStoreDir;
	}
}
