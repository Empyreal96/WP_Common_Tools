using System.Xml.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public enum RegValueType
	{
		[XmlEnum(Name = "REG_SZ")]
		String,
		[XmlEnum(Name = "REG_EXPAND_SZ")]
		ExpandString,
		[XmlEnum(Name = "REG_BINARY")]
		Binary,
		[XmlEnum(Name = "REG_DWORD")]
		DWord,
		[XmlEnum(Name = "REG_MULTI_SZ")]
		MultiString,
		[XmlEnum(Name = "REG_QWORD")]
		QWord,
		[XmlEnum(Name = "REG_HEX")]
		Hex
	}
}
