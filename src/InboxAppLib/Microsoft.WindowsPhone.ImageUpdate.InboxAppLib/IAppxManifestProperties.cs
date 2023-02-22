using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("03faf64d-f26f-4b2c-aaf7-8fe7789b8bca")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestProperties
	{
		bool GetBoolValue(string name);

		void GetStringValue(string name, [MarshalAs(UnmanagedType.LPWStr)] out string value);
	}
}
