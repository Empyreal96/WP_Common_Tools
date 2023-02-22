using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "AppResource", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class AppResourcePkgObject : OSComponentPkgObject
	{
		[XmlAttribute("Name")]
		public string Name { get; set; }

		[XmlAttribute("Suite")]
		public string Suite { get; set; }
	}
}
