using System;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class BuildPaths
	{
		public static string OEMKitFMSchema => "OEMKitFM.xsd";

		public static string PropsProjectSchema => "PropsProject.xsd";

		public static string PropsGuidMappingsSchema => "PropsGuidMappings.xsd";

		public static string PublishingPackageInfoSchema => "PublishingPackageInfo.xsd";

		public static string FMCollectionSchema => "FMCollection.xsd";

		public static string BuildCompDBSchema => "BuildCompDB.xsd";

		public static string UpdateCompDBSchema => "UpdateCompDB.xsd";

		public static string BSPCompDBSchema => "BSPCompDB.xsd";

		public static string DeviceCompDBSchema => "DeviceCompDB.xsd";

		public static string CompDBChunkMappingSchema => "CompDBChunkMapping.xsd";

		public static string CompDBPublishingInfoSchema => "CompDBPublishingInfo.xsd";

		public static string GetImagingTempPath(string defaultPath)
		{
			string environmentVariable = Environment.GetEnvironmentVariable("BUILD_PRODUCT");
			string text = Environment.GetEnvironmentVariable("OBJECT_ROOT");
			if ((!string.IsNullOrEmpty(environmentVariable) && environmentVariable.Equals("nt", StringComparison.OrdinalIgnoreCase)) || string.IsNullOrEmpty(text))
			{
				text = Environment.GetEnvironmentVariable("TEMP");
				if (string.IsNullOrEmpty(text))
				{
					text = Environment.GetEnvironmentVariable("TMP");
					if (string.IsNullOrEmpty(text))
					{
						text = defaultPath;
					}
				}
			}
			return FileUtils.GetTempFile(text);
		}
	}
}
