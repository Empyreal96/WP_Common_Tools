using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib.Interfaces
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ManifestConstants_APPX
	{
		public const string ManifestFileName = "AppxManifest.xml";

		public const string BundleManifestFileName = "AppxBundleManifest.xml";

		public const string AttributeValueID = "ID";

		public const string AttributeValueProcessorArch = "PROCESSORARCHITECTURE";

		public const string AttributeValueResourceID = "RESOURCEID";

		public const string AttributeValueDepMinVersion = "MINVERSION";

		public const string AttributeValuePackageType = "Type";

		public const string AttributeValuePackageVersion = "Version";

		public const string AttributeValuePackageArch = "Architecture";

		public const string AttributeValuePackageFileName = "FileName";

		public const string AttributeValuePackageResourceId = "ResourceId";

		public const string ElementNameResourcePackage = "ResourcePackage";

		public const string ElementNameFramework = "Framework";

		public const string IdentityProcessorArchNeutral = "neutral";

		public const string PhoneProductID = "PHONEPRODUCTID";

		public const string ResourceTypeLanguage = "Language";

		public const string ResourceTypeScale = "Scale";

		public const string ResourceTypeDXFeatureLevel = "DXFeatureLevel";

		public const string ProcessorTypeX86 = "X86";

		public const string ProcessorTypeARM = "ARM";
	}
}
