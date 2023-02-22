using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("b43bbcf9-65a6-42dd-bac0-8c6741e7f5a4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestPackageDependenciesEnumerator
	{
		IAppxManifestPackageDependency GetCurrent();

		bool GetHasCurrent();

		bool MoveNext();
	}
}
