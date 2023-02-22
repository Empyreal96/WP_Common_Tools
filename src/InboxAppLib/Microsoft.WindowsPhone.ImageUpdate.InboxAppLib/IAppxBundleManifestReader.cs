using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("CF0EBBC1-CC99-4106-91EB-E67462E04FB0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxBundleManifestReader
	{
		IAppxManifestPackageId GetPackageId();

		IAppxBundleManifestPackageInfoEnumerator GetPackageInfoItems();

		IStream GetStream();
	}
}
