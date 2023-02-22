using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("54CD06C1-268F-40BB-8ED2-757A9EBAEC8D")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxBundleManifestPackageInfo
	{
		APPX_BUNDLE_PAYLOAD_PACKAGE_TYPE GetPackageType();

		IAppxManifestPackageId GetPackageId();

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetFileName();

		[return: MarshalAs(UnmanagedType.LPWStr)]
		ulong GetOffset();

		ulong GetSize();

		IAppxManifestQualifiedResourcesEnumerator GetResources();
	}
}
