using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "OSComponent", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class OSComponentPkgObject : PkgObject
	{
		[XmlElement("Files")]
		public List<FileGroup> FileGroups { get; }

		[XmlElement("RegKeys")]
		public List<RegGroup> KeyGroups { get; }

		[XmlElement("RegImport")]
		public List<RegImport> RegImports { get; }

		public OSComponentPkgObject()
		{
			FileGroups = new List<FileGroup>();
			KeyGroups = new List<RegGroup>();
			RegImports = new List<RegImport>();
		}

		protected override void DoPreprocess(PackageProject proj, IMacroResolver macroResolver)
		{
			FileGroups.ForEach(delegate(FileGroup x)
			{
				x.Preprocess(macroResolver);
			});
			RegImports.ForEach(delegate(RegImport x)
			{
				x.Preprocess(macroResolver);
			});
		}

		protected override void DoBuild(IPackageGenerator pkgGen)
		{
			FileGroups.ForEach(delegate(FileGroup x)
			{
				x.Build(pkgGen);
			});
			if (pkgGen.BuildPass != 0)
			{
				KeyGroups.ForEach(delegate(RegGroup x)
				{
					x.Build(pkgGen);
				});
				RegImports.ForEach(delegate(RegImport x)
				{
					x.Build(pkgGen);
				});
			}
		}
	}
}
