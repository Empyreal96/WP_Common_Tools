using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "Files", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class FileGroup : FilterGroup
	{
		[XmlElement("File")]
		public List<PkgFile> Files;

		public FileGroup()
		{
			Files = new List<PkgFile>();
		}

		public void Preprocess(IMacroResolver macroResolver)
		{
			Files.ForEach(delegate(PkgFile x)
			{
				x.Preprocess(macroResolver);
			});
		}

		public override void Build(IPackageGenerator pkgGen, SatelliteId satelliteId)
		{
			Files.ForEach(delegate(PkgFile x)
			{
				x.Build(pkgGen, satelliteId);
			});
		}
	}
}
