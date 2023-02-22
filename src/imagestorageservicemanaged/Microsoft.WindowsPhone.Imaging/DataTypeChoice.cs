using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(IncludeInSchema = false)]
	public enum DataTypeChoice
	{
		WellKnownType,
		RawType
	}
}
