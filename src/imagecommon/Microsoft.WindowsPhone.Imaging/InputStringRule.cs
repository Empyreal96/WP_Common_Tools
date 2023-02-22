using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class InputStringRule : InputRule
	{
		[XmlArrayItem(ElementName = "Value", Type = typeof(string), IsNullable = false)]
		[XmlArray("List")]
		public string[] Values;
	}
}
