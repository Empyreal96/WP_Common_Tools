using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Security", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class Security
	{
		[XmlAttribute("InfSectionName")]
		public string InfSectionName { get; set; }

		internal Security()
		{
		}

		internal Security(string infSectionName)
		{
			InfSectionName = infSectionName;
		}
	}
}
