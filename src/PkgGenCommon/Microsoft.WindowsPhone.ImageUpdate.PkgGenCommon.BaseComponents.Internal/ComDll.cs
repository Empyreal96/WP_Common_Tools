using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Dll", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class ComDll : PkgFile
	{
	}
}
