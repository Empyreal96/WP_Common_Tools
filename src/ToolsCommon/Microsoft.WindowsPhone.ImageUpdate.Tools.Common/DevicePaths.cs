using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class DevicePaths
	{
		private static string _imageUpdatePath = "Windows\\ImageUpdate";

		private static string _updateFilesPath = "SharedData\\DuShared";

		private static string _registryHivePath = "Windows\\System32\\Config";

		private static string _BiosBCDHivePath = "boot";

		private static string _UefiBCDHivePath = "efi\\Microsoft\\boot";

		private static string _dsmPath = _imageUpdatePath;

		private static string _UpdateOSPath = "PROGRAMS\\UpdateOS\\";

		private static string _FMFilesDirectory = "FeatureManifest";

		private static string _OEMInputPath = "OEMInput";

		private static string _OEMInputFile = "OEMInput.xml";

		private static string _deviceLayoutFileName = "DeviceLayout.xml";

		private static string _oemDevicePlatformFileName = "OEMDevicePlatform.xml";

		private static string _updateOutputFile = "UpdateOutput.xml";

		private static string _updateHistoryFile = "UpdateHistory.xml";

		private static string _updateOSWIMName = "UpdateOS.wim";

		private static string _mmosWIMName = "MMOS.wim";

		public const string MAINOS_PARTITION_NAME = "MainOS";

		public const string MMOS_PARTITION_NAME = "MMOS";

		public static string ImageUpdatePath => _imageUpdatePath;

		public static string DeviceLayoutFileName => _deviceLayoutFileName;

		public static string DeviceLayoutFilePath => Path.Combine(ImageUpdatePath, DeviceLayoutFileName);

		public static string OemDevicePlatformFileName => _oemDevicePlatformFileName;

		public static string OemDevicePlatformFilePath => Path.Combine(ImageUpdatePath, OemDevicePlatformFileName);

		public static string UpdateOutputFile => _updateOutputFile;

		public static string UpdateOutputFilePath => Path.Combine(_updateFilesPath, _updateOutputFile);

		public static string UpdateHistoryFile => _updateHistoryFile;

		public static string UpdateHistoryFilePath => Path.Combine(_imageUpdatePath, _updateHistoryFile);

		public static string UpdateOSWIMName => _updateOSWIMName;

		public static string UpdateOSWIMFilePath => Path.Combine(_UpdateOSPath, UpdateOSWIMName);

		public static string MMOSWIMName => _mmosWIMName;

		public static string MMOSWIMFilePath => MMOSWIMName;

		public static string RegistryHivePath => _registryHivePath;

		public static string DeviceLayoutSchema => "DeviceLayout.xsd";

		public static string DeviceLayoutSchema2 => "DeviceLayoutv2.xsd";

		public static string UpdateOSInputSchema => "UpdateOSInput.xsd";

		public static string OEMInputSchema => "OEMInput.xsd";

		public static string FeatureManifestSchema => "FeatureManifest.xsd";

		public static string UpdateOSOutputSchema => "UpdateOSOutput.xsd";

		public static string UpdateHistorySchema => "UpdateHistory.xsd";

		public static string OEMDevicePlatformSchema => "OEMDevicePlatform.xsd";

		public static string MSFMPath => Path.Combine(ImageUpdatePath, _FMFilesDirectory, "Microsoft");

		public static string MSFMPathOld => ImageUpdatePath;

		public static string OEMFMPath => Path.Combine(ImageUpdatePath, _FMFilesDirectory, "OEM");

		public static string OEMInputPath => Path.Combine(ImageUpdatePath, _OEMInputPath);

		public static string OEMInputFile => Path.Combine(OEMInputPath, _OEMInputFile);

		public static string GetBCDHivePath(bool isUefiBoot)
		{
			if (!isUefiBoot)
			{
				return _BiosBCDHivePath;
			}
			return _UefiBCDHivePath;
		}

		public static string GetRegistryHiveFilePath(SystemRegistryHiveFiles hiveType)
		{
			return GetRegistryHiveFilePath(hiveType, true);
		}

		public static string GetRegistryHiveFilePath(SystemRegistryHiveFiles hiveType, bool isUefiBoot)
		{
			string result = "";
			switch (hiveType)
			{
			case SystemRegistryHiveFiles.DEFAULT:
				result = Path.Combine(RegistryHivePath, "DEFAULT");
				break;
			case SystemRegistryHiveFiles.DRIVERS:
				result = Path.Combine(RegistryHivePath, "DRIVERS");
				break;
			case SystemRegistryHiveFiles.SAM:
				result = Path.Combine(RegistryHivePath, "SAM");
				break;
			case SystemRegistryHiveFiles.SECURITY:
				result = Path.Combine(RegistryHivePath, "SECURITY");
				break;
			case SystemRegistryHiveFiles.SOFTWARE:
				result = Path.Combine(RegistryHivePath, "SOFTWARE");
				break;
			case SystemRegistryHiveFiles.SYSTEM:
				result = Path.Combine(RegistryHivePath, "SYSTEM");
				break;
			case SystemRegistryHiveFiles.BCD:
				result = Path.Combine(GetBCDHivePath(isUefiBoot), "BCD");
				break;
			}
			return result;
		}
	}
}
