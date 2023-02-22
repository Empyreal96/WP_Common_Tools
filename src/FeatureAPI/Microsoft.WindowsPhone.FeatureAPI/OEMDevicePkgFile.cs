namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class OEMDevicePkgFile : DevicePkgFile
	{
		public OEMDevicePkgFile()
			: base(FeatureManifest.PackageGroups.OEMDEVICEPLATFORM)
		{
		}

		public new void CopyPkgFile(PkgFile srcPkgFile)
		{
			base.CopyPkgFile(srcPkgFile);
		}
	}
}
