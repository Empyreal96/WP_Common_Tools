using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(IncludeInSchema = false)]
	public enum DeviceTypeChoice
	{
		GPTDevice,
		MBRDevice,
		RamdiskDevice
	}
}
