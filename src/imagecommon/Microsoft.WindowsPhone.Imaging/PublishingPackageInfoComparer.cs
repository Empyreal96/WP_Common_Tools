using System.Collections.Generic;

namespace Microsoft.WindowsPhone.Imaging
{
	public class PublishingPackageInfoComparer : EqualityComparer<PublishingPackageInfo>
	{
		public static EqualityComparer<PublishingPackageInfo> IgnorePaths => new EqualityComparerPublishingPackage(PublishingPackageInfo.PublishingPackageInfoComparison.IgnorePaths);

		public static EqualityComparer<PublishingPackageInfo> UniqueID => new EqualityComparerPublishingPackage(PublishingPackageInfo.PublishingPackageInfoComparison.OnlyUniqueID);

		public static EqualityComparer<PublishingPackageInfo> UniqueIDAndFeatureID => new EqualityComparerPublishingPackage(PublishingPackageInfo.PublishingPackageInfoComparison.OnlyUniqueIDAndFeatureID);

		protected PublishingPackageInfoComparer()
		{
		}

		public override bool Equals(PublishingPackageInfo x, PublishingPackageInfo y)
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

		public override int GetHashCode(PublishingPackageInfo pkg)
		{
			return pkg.GetHashCode();
		}
	}
}
