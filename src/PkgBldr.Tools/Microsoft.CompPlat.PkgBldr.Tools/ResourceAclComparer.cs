using System;
using System.Collections.Generic;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class ResourceAclComparer : IEqualityComparer<ResourceAcl>
	{
		public bool Equals(ResourceAcl x, ResourceAcl y)
		{
			bool result = false;
			if (x != null && y != null && !string.IsNullOrEmpty(x.Path) && !string.IsNullOrEmpty(y.Path))
			{
				result = x.Path.Equals(y.Path, StringComparison.OrdinalIgnoreCase);
			}
			return result;
		}

		public int GetHashCode(ResourceAcl obj)
		{
			int result = 0;
			if (obj != null && !string.IsNullOrEmpty(obj.Path))
			{
				result = obj.Path.GetHashCode();
			}
			return result;
		}
	}
}
