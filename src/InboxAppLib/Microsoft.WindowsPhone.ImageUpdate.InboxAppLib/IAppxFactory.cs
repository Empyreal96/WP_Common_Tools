using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("beb94909-e451-438b-b5a7-d79e767b75d8")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxFactory
	{
		IAppxPackageWriter CreatePackageWriter([In] IStream outputStream, [In] APPX_PACKAGE_SETTINGS settings);

		IAppxPackageReader CreatePackageReader([In] IStream inputStream);

		IAppxManifestReader CreateManifestReader([In] IStream inputStream);

		IAppxBlockMapReader CreateBlockMapReader([In] IStream inputStream);

		IAppxBlockMapReader CreateValidatedBlockMapReader([In] IStream blockMapStream, [In] string signatureFileName);
	}
}
