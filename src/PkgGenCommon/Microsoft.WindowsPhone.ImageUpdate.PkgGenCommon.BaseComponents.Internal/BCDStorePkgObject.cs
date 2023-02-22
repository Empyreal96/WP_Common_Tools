using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "BCDStore", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class BCDStorePkgObject : PkgObject
	{
		[XmlAttribute("Source")]
		public string Source { get; set; }

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			pkgGen.AddBCDStore(Source);
		}
	}
}
