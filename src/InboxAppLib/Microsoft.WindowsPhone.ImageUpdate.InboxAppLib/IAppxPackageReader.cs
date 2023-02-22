using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("b5c49650-99bc-481c-9a34-3d53a4106708")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxPackageReader
	{
		IAppxBlockMapReader GetBlockMap();

		IAppxFile GetFootprintFile([In] APPX_FOOTPRINT_FILE_TYPE type);

		IAppxFile GetPayloadFile([In][MarshalAs(UnmanagedType.LPWStr)] string fileName);

		IAppxFilesEnumerator GetPayloadFiles();

		IAppxManifestReader GetManifest();
	}
}
