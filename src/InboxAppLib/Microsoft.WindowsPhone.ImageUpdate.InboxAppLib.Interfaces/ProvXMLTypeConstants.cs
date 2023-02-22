using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ProvXMLTypeConstants
	{
		public const string Microsoft = "Microsoft";

		public const string Test = "Test";

		public const string OEM = "OEM";
	}
}
