using System;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public class BCDStoreBuilder : PkgObjectBuilder<BCDStorePkgObject, BCDStoreBuilder>
	{
		public BCDStoreBuilder(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				throw new ArgumentException("source must not be null or empty.");
			}
			pkgObject.Source = source;
		}
	}
}
