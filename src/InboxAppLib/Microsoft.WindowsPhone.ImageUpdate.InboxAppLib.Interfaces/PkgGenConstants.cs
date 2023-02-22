using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct PkgGenConstants
	{
		public const string XmlElementUniqueXPath = "@Source";

		public const string XmlSchemaPath = "Microsoft.WindowsPhone.ImageUpdate.InboxApp.InboxApp.Resources.Schema.xsd";

		public const string AttrSource = "Source";

		public const string AttrLicense = "License";

		public const string AttrProvXML = "ProvXML";

		public const string AttrInfuseIntoDataPartition = "InfuseIntoDataPartition";

		public static readonly ReadOnlyCollection<string> ValidAttrInfuseIntoDataPartitionValues = new ReadOnlyCollection<string>(new string[2] { "true", "false" });

		public const string AttrUpdate = "Update";

		public static readonly ReadOnlyCollection<string> ValidAttrUpdateValues = new ReadOnlyCollection<string>(new string[2] { "early", "normal" });

		public const string VariablePROVXMLTYPE = "PROVXMLTYPE";

		public static readonly ReadOnlyCollection<string> ValidVariablePROVXMLTYPEValues = new ReadOnlyCollection<string>(new string[3] { "Microsoft", "OEM", "Test" });
	}
}
