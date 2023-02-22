using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class InputRules
	{
		[XmlArrayItem(ElementName = "IntegerRule", Type = typeof(InputIntegerRule), IsNullable = false)]
		[XmlArray]
		public InputIntegerRule[] IntegerRules;

		[XmlArrayItem(ElementName = "StringRule", Type = typeof(InputStringRule), IsNullable = false)]
		[XmlArray]
		public InputStringRule[] StringRules;
	}
}
