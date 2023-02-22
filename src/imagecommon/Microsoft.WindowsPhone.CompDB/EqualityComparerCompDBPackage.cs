using System.Collections.Generic;

namespace Microsoft.WindowsPhone.CompDB
{
	public class EqualityComparerCompDBPackage : EqualityComparer<CompDBPackageInfo>
	{
		private CompDBPackageInfo.CompDBPackageInfoComparison _compareType;

		public EqualityComparerCompDBPackage(CompDBPackageInfo.CompDBPackageInfoComparison compareType)
		{
			_compareType = compareType;
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
			return x.Equals(y, _compareType);
		}

		public override int GetHashCode(CompDBPackageInfo pkg)
		{
			return pkg.GetHashCode(_compareType);
		}
	}
}
