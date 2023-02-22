using System.Collections.Generic;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public interface IDiffPkg : IPkgInfo
	{
		IEnumerable<IDiffEntry> DiffFiles { get; }
	}
}
