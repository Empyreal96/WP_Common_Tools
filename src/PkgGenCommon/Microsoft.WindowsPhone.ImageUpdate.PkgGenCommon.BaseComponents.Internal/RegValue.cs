using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "RegValue", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class RegValue
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("Type")]
		public RegValueType RegValType { get; set; }

		[XmlAttribute("Value")]
		public string Value { get; set; }
	}
}
