using System;
using System.Linq;
using Microsoft.Tools.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal static class PackageValidator
	{
		public static void Validate(PkgManifest package, string depoymentRoot)
		{
			ValidatePackage(package);
			ValidateFilePaths(package, depoymentRoot);
		}

		public static void ValidatePackage(PkgManifest package)
		{
			if (package.ReleaseType != ReleaseType.Test)
			{
				Log.Warning("Test package release type must be 'Test' [{0}]", package.Name);
			}
			if (package.OwnerType != OwnerType.Microsoft)
			{
				Log.Warning("Test package owner type must be 'Microsoft' [{0}]", package.Name);
			}
		}

		public static void ValidateFilePaths(PkgManifest package, string depoymentRoot)
		{
			string text = LongPathPath.Combine(depoymentRoot, "bin");
			string value = LongPathPath.Combine(depoymentRoot, "metadata");
			FileEntry[] files = package.Files;
			foreach (FileEntry obj in files)
			{
				string fileName = LongPathPath.GetFileName(obj.DevicePath);
				string fileNameWithoutExtension = LongPathPath.GetFileNameWithoutExtension(obj.DevicePath);
				string fileExtension = GetFileExtension(obj.DevicePath);
				string directoryName = LongPathPath.GetDirectoryName(obj.DevicePath);
				switch (fileExtension.ToLowerInvariant())
				{
				case ".dll":
				case ".exe":
					if (!directoryName.Equals(text, StringComparison.OrdinalIgnoreCase))
					{
						Log.Warning("Test Binary {0} should be deployed under $(TEST_DEPLOY_BIN) [{1}] instead of {2}.", fileName, text, directoryName);
					}
					continue;
				case ".meta.xml":
				{
					string nameWithoutExtension = LongPathPath.GetFileNameWithoutExtension(fileNameWithoutExtension);
					if (nameWithoutExtension.Equals(package.Name + ".spkg", StringComparison.OrdinalIgnoreCase))
					{
						if (!directoryName.Equals(value, StringComparison.OrdinalIgnoreCase))
						{
							Log.Warning("File {0} cannot be placed under $(TEST_DEPLOY_META) [{1}].", fileName, directoryName);
							Log.Warning("\tIf metadata file please check the file name of the metadata file or corresponding binary or package.");
						}
						continue;
					}
					FileEntry fileEntry = package.Files.FirstOrDefault((FileEntry fe) => LongPathPath.GetFileName(fe.DevicePath)?.Equals(nameWithoutExtension, StringComparison.OrdinalIgnoreCase) ?? false);
					if (fileEntry != null)
					{
						if (!directoryName.Equals(value, StringComparison.OrdinalIgnoreCase))
						{
							Log.Warning("Metadata File {0} cannot be placed under {1}.", fileName, directoryName);
							Log.Warning("\tIf metadata file please check the file name of the metadata file or corresponding binary or package and deploy under $(TEST_DEPLOY_META).");
						}
						continue;
					}
					break;
				}
				case ".config":
					continue;
				}
				if (directoryName.StartsWith(value, StringComparison.OrdinalIgnoreCase))
				{
					Log.Warning("Data File {0} cannot be placed under {1}.", fileName, directoryName);
					Log.Warning("\tIf metadata file please check the file name of the metadata file or corresponding binary or package and deploy under $(TEST_DEPLOY_META).");
				}
			}
		}

		private static string GetFileExtension(string fileName)
		{
			string result = LongPathPath.GetExtension(fileName);
			string extension = LongPathPath.GetExtension(LongPathPath.GetFileNameWithoutExtension(fileName));
			if (!string.IsNullOrEmpty(extension) && ".meta".Equals(extension, StringComparison.OrdinalIgnoreCase))
			{
				result = ".meta.xml";
			}
			return result;
		}
	}
}
