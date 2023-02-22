using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(IncludeInSchema = false)]
	public enum ValueTypeChoice
	{
		StringValue,
		BooleanValue,
		ObjectValue,
		ObjectListValue,
		IntegerValue,
		IntegerListValue,
		DeviceValue
	}
}
