using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("DD75B8C0-BA76-43B0-AE0F-68656A1DC5C8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxBundleReader
	{
		IAppxFile GetFootprintFile([In] APPX_BUNDLE_FOOTPRINT_FILE_TYPE fileType);

		IAppxBlockMapReader GetBlockMap();

		IAppxBundleManifestReader GetManifest();

		IAppxFilesEnumerator GetPayloadPackages();

		IAppxFile GetPayloadPackage([In][MarshalAs(UnmanagedType.LPWStr)] string fileName);
	}
}
