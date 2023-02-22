using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class SOCPkgFile : PkgFile
	{
		[XmlAttribute("SOC")]
		public string SOC;

		[XmlIgnore]
		public override string GroupValue
		{
			get
			{
				return SOC;
			}
			set
			{
				SOC = value;
			}
		}

		public SOCPkgFile(FeatureManifest.PackageGroups fmGroup)
			: base(fmGroup)
		{
		}

		public SOCPkgFile()
			: base(FeatureManifest.PackageGroups.SOC)
		{
			if (CPUType == null)
			{
				CPUType = FeatureManifest.CPUType_ARM;
			}
		}

		public new void CopyPkgFile(PkgFile srcPkgFile)
		{
			base.CopyPkgFile(srcPkgFile);
			SOCPkgFile sOCPkgFile = srcPkgFile as SOCPkgFile;
			SOC = sOCPkgFile.SOC;
		}
	}
}
