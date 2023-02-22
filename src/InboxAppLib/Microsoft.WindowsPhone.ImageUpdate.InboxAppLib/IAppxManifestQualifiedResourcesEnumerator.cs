using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[CLSCompliant(false)]
	[Guid("8ef6adfe-3762-4a8f-9373-2fc5d444c8d2")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestQualifiedResourcesEnumerator
	{
		IAppxManifestQualifiedResource GetCurrent();

		bool GetHasCurrent();

		bool MoveNext();
	}
}
