using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	[Guid("de4dfbbd-881a-48bb-858c-d6f2baeae6ed")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAppxManifestResourcesEnumerator
	{
		[return: MarshalAs(UnmanagedType.LPWStr)]
		string GetCurrent();

		bool GetHasCurrent();

		bool MoveNext();
	}
}
