using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("11D22258-F470-42C1-B291-8361C5437E41")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestCapabilitiesEnumerator
	{
		APPX_CAPABILITIES GetCurrent();

		bool GetHasCurrent();

		bool MoveNext();
	}
}
