using System.Xml.Linq;
using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Application", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class ApplicationPkgObject : AppResourcePkgObject
	{
		[XmlAnyElement("RequiredCapabilities")]
		public XElement RequiredCapabilities { get; set; }

		[XmlAnyElement("PrivateResources")]
		public XElement PrivateResources { get; set; }
	}
}
