using System;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents
{
	public class BinaryPartitionBuilder : PkgObjectBuilder<BinaryPartitionPkgObject, BinaryPartitionBuilder>
	{
		public BinaryPartitionBuilder(string imageSource)
		{
			if (string.IsNullOrEmpty(imageSource))
			{
				throw new ArgumentException("imageSource must not be null or empty.");
			}
			pkgObject.ImageSource = imageSource;
		}
	}
}
