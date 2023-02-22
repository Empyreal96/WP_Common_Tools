using System.Collections.Generic;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBPackageInfoComparer : EqualityComparer<CompDBPackageInfo>
	{
		public static EqualityComparer<CompDBPackageInfo> Standard => new EqualityComparerCompDBPackage(CompDBPackageInfo.CompDBPackageInfoComparison.Standard);

		public static EqualityComparer<CompDBPackageInfo> IgnorePackageHash => new EqualityComparerCompDBPackage(CompDBPackageInfo.CompDBPackageInfoComparison.IgnorePayloadHashes);

		public static EqualityComparer<CompDBPackageInfo> IgnorePaths => new EqualityComparerCompDBPackage(CompDBPackageInfo.CompDBPackageInfoComparison.IgnorePayloadPaths);

		public static EqualityComparer<CompDBPackageInfo> UniqueID => new EqualityComparerCompDBPackage(CompDBPackageInfo.CompDBPackageInfoComparison.OnlyUniqueID);

		public static EqualityComparer<CompDBPackageInfo> UniqueIDAndFeatureID => new EqualityComparerCompDBPackage(CompDBPackageInfo.CompDBPackageInfoComparison.OnlyUniqueIDAndFeatureID);

		protected CompDBPackageInfoComparer()
		{
		}

		public override bool Equals(CompDBPackageInfo x, CompDBPackageInfo y)
		{
			if (x == null)
			{
				if (y == null)
				{
					return true;
				}
				return false;
			}
			return x.Equals(y);
		}

		public override int GetHashCode(CompDBPackageInfo pkg)
		{
			return pkg.GetHashCode();
		}
	}
}
