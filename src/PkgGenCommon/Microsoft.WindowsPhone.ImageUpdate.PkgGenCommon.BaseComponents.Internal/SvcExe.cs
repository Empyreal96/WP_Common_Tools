using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot("Executable", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class SvcExe : SvcEntry
	{
		[XmlAttribute("ImagePath")]
		public string ImagePath;

		[XmlAttribute("BinaryInOneCorePkg")]
		public bool BinaryInOneCorePkg;

		[XmlIgnore]
		public string ImagePathExpandString
		{
			get
			{
				if (string.IsNullOrEmpty(ImagePath))
				{
					string path = ((Name == null) ? Path.GetFileName(SourcePath) : Name);
					return Path.Combine(DestinationDir.Replace("$(runtime.", "$(env."), path);
				}
				return ImagePath;
			}
		}

		public override void Build(IPackageGenerator pkgGen)
		{
			if (!BinaryInOneCorePkg)
			{
				base.Build(pkgGen);
			}
			if (pkgGen.BuildPass != 0)
			{
				pkgGen.AddRegValue("$(hklm.service)", "ImagePath", RegValueType.ExpandString, ImagePathExpandString);
			}
		}
	}
}
