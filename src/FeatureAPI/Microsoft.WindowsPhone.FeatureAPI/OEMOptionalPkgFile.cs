namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class OEMOptionalPkgFile : OptionalPkgFile
	{
		public OEMOptionalPkgFile()
			: base(FeatureManifest.PackageGroups.OEMFEATURE)
		{
		}

		public OEMOptionalPkgFile(OptionalPkgFile srcPkg)
			: base(srcPkg)
		{
			FMGroup = FeatureManifest.PackageGroups.OEMFEATURE;
		}
	}
}
