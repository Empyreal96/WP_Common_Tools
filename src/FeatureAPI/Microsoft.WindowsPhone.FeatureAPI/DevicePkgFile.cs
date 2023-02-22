using System.Xml.Serialization;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class DevicePkgFile : PkgFile
	{
		[XmlAttribute("Device")]
		public string Device;

		[XmlIgnore]
		public override string GroupValue
		{
			get
			{
				return Device;
			}
			set
			{
				Device = value;
			}
		}

		public DevicePkgFile()
			: base(FeatureManifest.PackageGroups.DEVICE)
		{
		}

		public DevicePkgFile(FeatureManifest.PackageGroups fmGroup)
			: base(fmGroup)
		{
		}

		public new void CopyPkgFile(PkgFile srcPkgFile)
		{
			base.CopyPkgFile(srcPkgFile);
			DevicePkgFile devicePkgFile = srcPkgFile as DevicePkgFile;
			Device = devicePkgFile.Device;
		}
	}
}
