namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class MSOptionalPkgFile : OptionalPkgFile
	{
		public MSOptionalPkgFile()
			: base(FeatureManifest.PackageGroups.MSFEATURE)
		{
		}

		public MSOptionalPkgFile(OptionalPkgFile srcPkg)
			: base(srcPkg)
		{
			FMGroup = FeatureManifest.PackageGroups.MSFEATURE;
		}
	}
}
