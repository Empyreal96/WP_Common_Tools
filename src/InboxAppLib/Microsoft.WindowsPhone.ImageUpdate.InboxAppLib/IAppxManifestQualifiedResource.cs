using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("3b53a497-3c5c-48d1-9ea3-bb7eac8cd7d4")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestQualifiedResource
	{
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetLanguage();

		uint GetScale();

		DX_FEATURE_LEVEL GetDXFeatureLevel();
	}
}
