using System.Collections.Generic;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	public class Customizations
	{
		public const string PROVISIONING_PKG_EXT = ".ppkg";

		public const string CUSTOMIZATION_XML_EXT = ".xml";

		public string CustomizationXMLFilePath { get; set; }

		public string CustomizationPPKGFilePath { get; set; }

		public string OutputDirectory { get; set; }

		public string ImageDeviceName { get; set; }

		public CpuId ImageCpuType { get; set; }

		public BuildType ImageBuildType { get; set; }

		public VersionInfo ImageVersion { get; set; }

		public List<IPkgInfo> ImagePackages { get; set; }

		public List<string> ImageLanguages { get; set; }

		public static bool StrictSettingPolicies { get; set; }
	}
}
