using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("BBA65864-965F-4A5F-855F-F074BDBF3A7B")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxBundleFactory
	{
		IAppxBundleWriter CreateBundleWriter([In] IStream outputStream, [In] ulong bundleVersion);

		IAppxBundleReader CreateBundleReader([In] IStream inputStream);

		IAppxBundleManifestReader CreateBundleManifestReader([In] IStream inputStream);
	}
}
