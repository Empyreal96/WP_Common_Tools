using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class PrereleasePkgFile : PkgFile
	{
		[XmlAttribute("Type")]
		public string Type;

		[XmlIgnore]
		public override string GroupValue
		{
			get
			{
				return Type;
			}
			set
			{
				Type = value;
			}
		}

		public PrereleasePkgFile()
			: base(FeatureManifest.PackageGroups.PRERELEASE)
		{
		}

		public new void CopyPkgFile(PkgFile srcPkgFile)
		{
			base.CopyPkgFile(srcPkgFile);
			PrereleasePkgFile prereleasePkgFile = srcPkgFile as PrereleasePkgFile;
			Type = prereleasePkgFile.Type;
		}
	}
}
