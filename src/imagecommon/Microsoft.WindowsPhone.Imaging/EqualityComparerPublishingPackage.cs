using System.Collections.Generic;

namespace Microsoft.WindowsPhone.Imaging
{
	public class EqualityComparerPublishingPackage : EqualityComparer<PublishingPackageInfo>
	{
		private PublishingPackageInfo.PublishingPackageInfoComparison _compareType;

		public EqualityComparerPublishingPackage(PublishingPackageInfo.PublishingPackageInfoComparison compareType)
		{
			_compareType = compareType;
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
			return x.Equals(y, _compareType);
		}

		public override int GetHashCode(PublishingPackageInfo pkg)
		{
			return pkg.GetHashCode(_compareType);
		}
	}
}
