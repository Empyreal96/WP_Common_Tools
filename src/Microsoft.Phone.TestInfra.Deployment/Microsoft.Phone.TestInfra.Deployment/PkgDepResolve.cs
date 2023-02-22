using System;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public class PkgDepResolve : IEquatable<PkgDepResolve>
	{
		public PackageInfo PkgInfo { get; set; }

		public bool IsProcessed { get; set; }

		public new bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals(obj as PkgDepResolve);
		}

		public bool Equals(PkgDepResolve other)
		{
			return string.Compare(PkgInfo.PackageName, other.PkgInfo.PackageName, true) == 0;
		}

		public override int GetHashCode()
		{
			return PkgInfo.PackageName.GetHashCode();
		}
	}
}
