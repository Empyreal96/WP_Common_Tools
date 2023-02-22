using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct CommonConstants
	{
		public const string AttributeValueProductID = "PRODUCTID";

		public const string AttributeValueName = "NAME";

		public const string AttributeValueID = "ID";

		public const string AttributeValueTitle = "TITLE";

		public const string AttributeValueVersion = "VERSION";

		public const string AttributeValuePublisher = "PUBLISHER";

		public const string AttributeValueDescription = "DESCRIPTION";

		public const string PkgGenMacroHeader = "$(";

		public const string PkgGenMacroFooter = ")";

		public const string InRomUpdateMicrosoftProvXMLDestinationPathFormat = "$(runtime.updateProvxmlMS)\\mxipupdate{0}";

		public const string InRomUpdateOEMProvXMLDestinationPathFormat = "$(runtime.updateProvxmlOEM)\\mxipupdate{0}";

		public const string InRomUpdateAppFrameworkProvXMLDestinationPathFormat = "$(runtime.updateProvxmlMS)\\appframework{0}";
	}
}
