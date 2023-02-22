using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	public class InputIntegerRule : InputRule
	{
		[XmlArrayItem(ElementName = "Value", Type = typeof(ulong), IsNullable = false)]
		[XmlArray("List")]
		public ulong[] Values;

		public ulong? Max { get; set; }

		public ulong? Min { get; set; }
	}
}
