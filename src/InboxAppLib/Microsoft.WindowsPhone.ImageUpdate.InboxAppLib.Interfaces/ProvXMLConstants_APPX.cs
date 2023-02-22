using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ProvXMLConstants_APPX
	{
		public const string AttributeValueAppxPath = "APPXPATH";

		public const string AttributeValueAppxManifestPath = "APPXMANIFESTPATH";

		public const string FrameworkSubDir = "Framework";

		public const string AppxColdFrameworkPrefix = "mxipcold_appframework_";

		public const string InRomProvXMLBaseDestinationPath = "$(runtime.commonfiles)\\Provisioning\\";

		public const string InRomFrameworkProvXMLBaseDestinationPath = "$(runtime.coldBootProvxmlMS)\\";

		public const string DataPartitionProvXMLBaseDestinationPath = "$(runtime.data)\\SharedData\\Provisioning\\";

		public const string InRomLicenseBaseDestinationPath = "$(runtime.commonfiles)\\Xaps\\";

		public const string DataPartitionLicenseBaseDestinationPath = "$(runtime.data)\\SharedData\\Provisioning\\";

		public const string ProvXmlCharacteristic_AppxPackage = "AppxPackage";

		public const string ProvXmlCharacteristic_AppxInfused = "AppxInfused";

		public const string ProvXmlCharacteristic_FrameworkPackage = "FrameworkPackage";

		public const string AttributeNameAppxManifestPath = "AppXManifestPath";

		public const string AttributeValueProductID = "PRODUCTID";

		public const string AttributeValueInstanceID = "INSTANCEID";

		public const string AttributeValueLicensePath = "LICENSEPATH";

		public const string AttributeValueOfferID = "OFFERID";

		public const string AttributeValuePayloadID = "PAYLOADID";

		public const string AttributeValueIsBundle = "IsBundle";

		public const string AttributeValueDependencyPkgs = "DependencyPackages";
	}
}
