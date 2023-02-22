using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class OEMInputFeatures
	{
		[XmlArrayItem(ElementName = "Feature", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> Microsoft;

		[XmlArrayItem(ElementName = "Feature", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		public List<string> OEM;
	}
}
