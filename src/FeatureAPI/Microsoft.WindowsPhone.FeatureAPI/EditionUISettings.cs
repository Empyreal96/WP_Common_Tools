using System.Collections.Generic;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class EditionUISettings
	{
		[XmlAttribute]
		public UIDisplayType DisplayType;

		[XmlArrayItem(ElementName = "Lookup", Type = typeof(EditionLookup), IsNullable = false)]
		[XmlArray]
		public List<EditionLookup> Lookups;
	}
}
