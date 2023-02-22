using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "BinaryPartition", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public sealed class BinaryPartitionPkgObject : PkgObject
	{
		[XmlAttribute("ImageSource")]
		public string ImageSource { get; set; }

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			pkgGen.AddBinaryPartition(ImageSource);
		}
	}
}
