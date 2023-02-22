using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("f007eeaf-9831-411c-9847-917cdc62d1fe")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxFilesEnumerator
	{
		IAppxFile GetCurrent();

		bool GetHasCurrent();

		bool MoveNext();
	}
}
