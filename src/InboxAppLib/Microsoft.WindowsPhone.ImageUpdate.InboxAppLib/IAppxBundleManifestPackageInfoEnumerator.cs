using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("F9B856EE-49A6-4E19-B2B0-6A2406D63A32")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxBundleManifestPackageInfoEnumerator
	{
		IAppxBundleManifestPackageInfo GetCurrent();

		bool GetHasCurrent();

		bool MoveNext();
	}
}
