using System;
using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public static class PkgConstants
	{
		public static readonly string c_strCBSPackageExtension = ".cab";

		public static readonly string c_strPackageExtension = ".spkg";

		public static readonly string c_strPackageSearchPattern = "*" + c_strPackageExtension;

		public static readonly string c_strRemovalPkgExtension = ".spkr";

		public static readonly string c_strRemovalPkgSearchPattern = "*" + c_strRemovalPkgExtension;

		public static readonly string c_strRemovalCbsExtension = ".cbsr";

		public static readonly string c_strRemovalCbsSearchPattern = "*" + c_strRemovalCbsExtension;

		public static readonly string c_strDiffPackageExtension = ".spku";

		public static readonly string c_strDiffPackageSearchPattern = "*" + c_strPackageExtension;

		public static readonly string c_strDsmExtension = ".dsm.xml";

		public static readonly string c_strMumExtension = ".mum";

		public static readonly string c_strDsmFile = "man" + c_strDsmExtension;

		public static readonly string c_strMumFile = "update" + c_strMumExtension;

		public static readonly string c_strCatalogFileExtension = ".cat";

		public static readonly string c_strCatalogFile = "content" + c_strCatalogFileExtension;

		public static readonly string c_strCBSCatalogFile = "update" + c_strCatalogFileExtension;

		public static readonly string c_strDsmSearchPattern = "*" + c_strDsmExtension;

		public static readonly string c_strMumSearchPattern = "*" + c_strMumExtension;

		public static readonly string c_strDiffDsmExtension = ".ddsm.xml";

		public static readonly string c_strDiffDsmFile = "dman" + c_strDiffDsmExtension;

		public static readonly string c_strDiffDsmSearchPattern = "*" + c_strDiffDsmExtension;

		public static readonly string c_strRguExtension = ".reg";

		public static readonly string c_strRegAppendExtension = ".rga";

		public static readonly string c_strPolicyExtension = ".policy.xml";

		public static readonly string c_strCustomMetadataExtension = ".meta.xml";

		public static readonly string c_strCIX = "_manifest_.cix.xml";

		public static readonly string c_strCertStoreExtension = ".dat";

		public static readonly string c_strPkgMetadataFolder = "\\Windows\\Packages";

		public static readonly string c_strDsmDeviceFolder = c_strPkgMetadataFolder + "\\DsmFiles";

		public static readonly string c_strMumDeviceFolder = "Windows\\servicing\\Packages";

		public static readonly string c_strRguDeviceFolder = c_strPkgMetadataFolder + "\\RegistryFiles";

		public static readonly string c_strRgaDeviceFolder = c_strRguDeviceFolder;

		public static readonly string c_strPolicyDeviceFolder = c_strPkgMetadataFolder + "\\PolicyFiles";

		public static readonly string c_strCustomMetadataDeviceFolder = c_strPkgMetadataFolder + "\\CustomMetadata";

		private static readonly string c_strBackupMetadataRootDeviceFolder = c_strPkgMetadataFolder + "\\BackupMetadata";

		public static readonly string c_strBackupMetadataDirectoriesDeviceFolder = c_strBackupMetadataRootDeviceFolder + "\\Directories";

		public static readonly string c_strBackupMetadataFilesDeviceFolder = c_strBackupMetadataRootDeviceFolder + "\\Files";

		public static string c_strCertStoreDeviceFolder = c_strPkgMetadataFolder + "\\Certificates";

		public static readonly string c_strCatalogDeviceFolder = "\\Windows\\System32\\catroot\\{F750E6C3-38EE-11D1-85E5-00C04FC295EE}";

		public static readonly string c_strCBSPublicKey = "628844477771337a";

		public static readonly string[] c_strSpecialFolders = new string[5] { c_strDsmDeviceFolder, c_strRguDeviceFolder, c_strRgaDeviceFolder, c_strPolicyDeviceFolder, c_strCertStoreDeviceFolder };

		public static readonly string c_strMainOsPartition = "MainOS";

		public static readonly string c_strUpdateOsPartition = "UpdateOS";

		public static readonly string c_strEfiPartition = "EFIESP";

		public static readonly string c_strDataPartition = "Data";

		public static readonly string c_strPlatPartition = "PLAT";

		public static readonly string c_strCrashDumpPartition = "CrashDump";

		public static readonly string c_strDPPPartition = "DPP";

		public static readonly string c_strDataPartitionRoot;

		public static readonly string[] c_strHivePartitions;

		public static readonly string[] c_strJunctionPaths;

		public static readonly string c_strDefaultPartition;

		public static readonly string c_strDefaultDrive;

		public static readonly string c_strUpdateOSDrive;

		public static readonly int c_iMaxPackageString;

		public static readonly int c_iMaxElementCount;

		public static readonly int c_iMaxPackageName;

		public static readonly string c_strPackageStringPattern;

		public static readonly string c_strCultureStringPattern;

		public static readonly string c_strResolutionStringPattern;

		public static readonly int c_iMaxBuildString;

		public static readonly int c_iMaxDevicePath;

		public static readonly string c_strHashAlgorithm;

		public static readonly int c_iHashSize;

		public static readonly FileAttributes c_validAttributes;

		public static readonly FileAttributes c_defaultAttributes;

		public static int c_iMaxGroupIdString;

		public static int c_iMaxPackagingThreads;

		public static string c_strGroupIdPattern;

		static PkgConstants()
		{
			char directorySeparatorChar = Path.DirectorySeparatorChar;
			string text = directorySeparatorChar.ToString();
			string text2 = c_strDataPartition;
			directorySeparatorChar = Path.DirectorySeparatorChar;
			c_strDataPartitionRoot = text + text2 + directorySeparatorChar;
			c_strHivePartitions = new string[3] { c_strMainOsPartition, c_strUpdateOsPartition, c_strEfiPartition };
			string[] obj = new string[4] { c_strDataPartitionRoot, null, null, null };
			directorySeparatorChar = Path.DirectorySeparatorChar;
			string text3 = directorySeparatorChar.ToString();
			string text4 = c_strEfiPartition;
			directorySeparatorChar = Path.DirectorySeparatorChar;
			obj[1] = text3 + text4 + directorySeparatorChar;
			directorySeparatorChar = Path.DirectorySeparatorChar;
			string text5 = directorySeparatorChar.ToString();
			string text6 = c_strCrashDumpPartition;
			directorySeparatorChar = Path.DirectorySeparatorChar;
			obj[2] = text5 + text6 + directorySeparatorChar;
			directorySeparatorChar = Path.DirectorySeparatorChar;
			string text7 = directorySeparatorChar.ToString();
			string text8 = c_strDPPPartition;
			directorySeparatorChar = Path.DirectorySeparatorChar;
			obj[3] = text7 + text8 + directorySeparatorChar;
			c_strJunctionPaths = obj;
			c_strDefaultPartition = c_strMainOsPartition;
			c_strDefaultDrive = "C:";
			c_strUpdateOSDrive = "X:";
			c_iMaxPackageString = 64;
			c_iMaxElementCount = 8000;
			c_iMaxPackageName = c_iMaxPackageString * 3 + 2;
			c_strPackageStringPattern = "^[0-9a-zA-Z_\\-.]+$";
			c_strCultureStringPattern = "^[a-zA-Z][a-zA-Z0-9_\\-]+$";
			c_strResolutionStringPattern = "^[1-9][0-9]+x|X[1-9][0-9]+$";
			c_iMaxBuildString = 1024;
			c_iMaxDevicePath = 32000;
			c_strHashAlgorithm = "SHA256";
			c_iHashSize = 32;
			c_validAttributes = FileAttributes.ReadOnly | FileAttributes.Hidden | FileAttributes.System | FileAttributes.Archive | FileAttributes.Normal | FileAttributes.Compressed;
			c_defaultAttributes = FileAttributes.Archive | FileAttributes.Compressed;
			c_iMaxGroupIdString = 39;
			c_iMaxPackagingThreads = Environment.ProcessorCount * 2;
			c_strGroupIdPattern = "^[a-zA-Z0-9_.\\\\/\\-{}]+$";
		}
	}
}
