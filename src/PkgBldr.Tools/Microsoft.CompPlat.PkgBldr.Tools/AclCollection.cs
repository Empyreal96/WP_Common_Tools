using System.Collections.Generic;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class AclCollection : HashSet<ResourceAcl>
	{
		public AclCollection()
			: base((IEqualityComparer<ResourceAcl>)ResourceAcl.Comparer)
		{
		}
	}
}
