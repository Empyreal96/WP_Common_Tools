using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("283ce2d7-7153-4a91-9649-7a0f7240945f")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestPackageId
	{
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetName();

		APPX_PACKAGE_ARCHITECTURE GetArchitecture();

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetPublisher();

		ulong GetVersion();

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetResourceId();

		bool ComparePublisher([In][MarshalAs(UnmanagedType.LPWStr)] string otherPublisher);

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetPackageFullName();

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetPackageFamilyName();
	}
}
