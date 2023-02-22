using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ProvXMLConstants
	{
		public const string FileNameBeginsWith = "MPAP_";

		public const string FileNameEndsWith = "_Infused";

		public const char FileNameDelimiter = '_';

		public const string Extension = ".provxml";

		public const string SrcExtension = ".src";

		public const string ElementNameCharacteristic = "characteristic";

		public const string ElementNameParm = "parm";

		public const string CharacteristicValueAppInstall = "AppInstall";

		public const string AttributeNameType = "type";

		public const string AttributeNameName = "name";

		public const string AttributeNameValue = "value";
	}
}
