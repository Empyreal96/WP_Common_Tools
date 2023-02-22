using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ProvXMLConstants_XAP
	{
		public const string InRomProvXMLBaseDestinationPath = "$(runtime.commonfiles)\\Provisioning\\";

		public const string DataPartitionProvXMLBaseDestinationPath = "$(runtime.data)\\SharedData\\Provisioning\\";

		public const string InRomLicenseBaseDestinationPath = "$(runtime.commonfiles)\\Provisioning\\";

		public const string DataPartitionLicenseBaseDestinationPath = "$(runtime.data)\\SharedData\\Provisioning\\";

		public const string ProvXmlCharacteristic_XapPackage = "XapPackage";

		public const string ProvXmlCharacteristic_XapInfused = "XapInfused";

		public const string XapLegacyParmNameINSTALLINFO = "INSTALLINFO";

		public const string AttributeNameXAPManifestPath = "XapManifestPath";

		public const string AttributeValueXAPMANIFESTPATH = "XAPMANIFESTPATH";

		public const string AttributeValueXAPPATH = "XAPPATH";

		public const string AttributeValuePRODUCTID = "PRODUCTID";

		public const string AttributeValueProductID = "ProductID";

		public const string AttributeValueLICENSEPATH = "LICENSEPATH";

		public const string AttributeValueLicensePath = "LicensePath";

		public const string AttributeValueINSTANCEID = "INSTANCEID";

		public const string AttributeValueInstanceID = "InstanceID";

		public const string AttributeValueOFFERID = "OFFERID";

		public const string AttributeValueOfferID = "OfferID";

		public const string AttributeValueUninstallDisabled = "UninstallDisabled";
	}
}
