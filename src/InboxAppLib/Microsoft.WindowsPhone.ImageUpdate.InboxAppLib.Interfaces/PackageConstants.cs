using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PackageConstants
	{
		public const string PackageContentsSubDir = "Content";

		public const string RuntimeDataMacro = "$(runtime.data)";
	}
}
