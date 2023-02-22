using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class AclCollection : HashSet<ResourceAcl>
	{
		public AclCollection()
			: base((IEqualityComparer<ResourceAcl>)ResourceAcl.Comparer)
		{
		}
	}
}
