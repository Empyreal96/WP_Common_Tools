using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class SVPkgFile : PkgFile
	{
		[XmlAttribute("SV")]
		public string SV;

		[XmlIgnore]
		public override string GroupValue
		{
			get
			{
				return SV;
			}
			set
			{
				SV = value;
			}
		}

		public SVPkgFile()
			: base(FeatureManifest.PackageGroups.SV)
		{
		}

		public new void CopyPkgFile(PkgFile srcPkgFile)
		{
			base.CopyPkgFile(srcPkgFile);
			SVPkgFile sVPkgFile = srcPkgFile as SVPkgFile;
			SV = sVPkgFile.SV;
		}
	}
}
