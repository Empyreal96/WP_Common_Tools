using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class SupportedLangs
	{
		[XmlArrayItem(ElementName = "Language", Type = typeof(string), IsNullable = false)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> UserInterface;

		[XmlArrayItem(ElementName = "Language", Type = typeof(string), IsNullable = true)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> Keyboard;

		[XmlArrayItem(ElementName = "Language", Type = typeof(string), IsNullable = true)]
		[XmlArray]
		[DefaultValue(null)]
		public List<string> Speech;
	}
}
