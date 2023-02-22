using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct ATTACH_VIRTUAL_DISK_PARAMETERS
	{
		public ATTACH_VIRTUAL_DISK_VERSION Version;

		public int Reserved;
	}
}
