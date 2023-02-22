using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.Interfaces;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	[XmlRoot(ElementName = "File", Namespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00")]
	public class PkgFile : PkgElement
	{
		[XmlAttribute("Source")]
		public string SourcePath;

		[XmlAttribute("DestinationDir")]
		public string DestinationDir = "$(runtime.default)";

		[XmlAttribute("Name")]
		public string Name;

		[XmlAttribute("Attributes")]
		public FileAttributes Attributes = PkgConstants.c_defaultAttributes;

		[XmlAttribute("EmbeddedSigningCategory")]
		public string EmbeddedSigningCategory = "None";

		[XmlIgnore]
		public string DevicePath => Path.Combine(DestinationDir, (Name == null) ? Path.GetFileName(SourcePath) : Name);

		public bool ShouldSerializeDestinationDir()
		{
			return !DestinationDir.Equals("$(runtime.default)", StringComparison.InvariantCulture);
		}

		public bool ShouldSerializeAttributes()
		{
			return Attributes != PkgConstants.c_defaultAttributes;
		}

		public void Preprocess(IMacroResolver macroResolver)
		{
			SourcePath = macroResolver.Resolve(SourcePath, MacroResolveOptions.SkipOnUnknownMacro);
		}

		public override void Build(IPackageGenerator pkgGen, SatelliteId satelliteId)
		{
			pkgGen.AddFile(SourcePath, DevicePath, Attributes, satelliteId, EmbeddedSigningCategory);
		}
	}
}
