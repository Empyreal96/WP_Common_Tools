using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PackageConstants_XAP
	{
		public const string Extension = ".xap";

		public const string InRomInstallBaseDestinationPath = "$(runtime.commonfiles)\\InboxApps";

		public const string DataPartitionInstallBaseDestinationPath = "$(runtime.data)\\Programs\\{0}\\install";
	}
}
