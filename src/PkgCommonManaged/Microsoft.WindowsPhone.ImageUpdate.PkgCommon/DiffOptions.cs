using System;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	[Flags]
	public enum DiffOptions
	{
		None = 0,
		PrsSignCatalog = 1,
		DeltaThresholdMB = 2
	}
}
