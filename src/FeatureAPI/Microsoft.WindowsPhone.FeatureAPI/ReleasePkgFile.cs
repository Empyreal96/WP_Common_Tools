using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class ReleasePkgFile : PkgFile
	{
		[XmlAttribute("ReleaseType")]
		public string ReleaseType;

		[XmlIgnore]
		public override string GroupValue
		{
			get
			{
				return ReleaseType;
			}
			set
			{
				ReleaseType = value;
			}
		}

		public ReleasePkgFile()
			: base(FeatureManifest.PackageGroups.RELEASE)
		{
		}

		public new void CopyPkgFile(PkgFile srcPkgFile)
		{
			base.CopyPkgFile(srcPkgFile);
			ReleasePkgFile releasePkgFile = srcPkgFile as ReleasePkgFile;
			ReleaseType = releasePkgFile.ReleaseType;
		}
	}
}
