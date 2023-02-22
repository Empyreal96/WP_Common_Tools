using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("e4946b59-733e-43f0-a724-3bde4c1285a0")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestPackageDependency
	{
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetName();

		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetPublisher();

		ulong GetMinVersion();
	}
}
