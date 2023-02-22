using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class EditionPackage
	{
		[XmlAttribute]
		public string RelativePath;

		[XmlAttribute]
		public string PackageName;

		[XmlAttribute]
		public string FMDeviceDir;

		[XmlAttribute]
		public string FMDeviceName;

		[XmlAttribute]
		public string AKName;

		private FeatureManifest _fm;

		[XmlIgnore]
		public string FMDevicePath => Path.Combine(FMDeviceDir, FMDeviceName);

		[XmlIgnore]
		public FeatureManifest FM => _fm;

		public override string ToString()
		{
			return PackageName;
		}

		public string GetPackagePath(string msPackageRoot, string cpuType, string buildType)
		{
			string rawPackagePath = GetRawPackagePath(msPackageRoot);
			rawPackagePath = ProcessEnvs(rawPackagePath, cpuType, buildType);
			if (!File.Exists(rawPackagePath))
			{
				string text = ((!Path.GetExtension(rawPackagePath).Equals(PkgConstants.c_strCBSPackageExtension, StringComparison.OrdinalIgnoreCase)) ? Path.ChangeExtension(rawPackagePath, PkgConstants.c_strCBSPackageExtension) : Path.ChangeExtension(rawPackagePath, PkgConstants.c_strPackageExtension));
				if (File.Exists(text))
				{
					rawPackagePath = text;
				}
			}
			return rawPackagePath;
		}

		public string GetRawPackagePath(string msPackageRoot)
		{
			return Path.Combine(msPackageRoot, RelativePath, PackageName);
		}

		public FeatureManifest LoadFM(string tempDirectory, string msPackageRoot, string cpuType, string buildType)
		{
			string tempDirectory2 = FileUtils.GetTempDirectory();
			try
			{
				string packagePath = GetPackagePath(msPackageRoot, cpuType, buildType);
				string text = Path.Combine(tempDirectory2, Path.GetFileName(packagePath));
				LongPathFile.Copy(packagePath, text);
				IPkgInfo pkgInfo = Package.LoadFromCab(text);
				bool overwriteExistingFiles = true;
				string text2 = Path.Combine(tempDirectory2, Path.GetFileName(FMDevicePath));
				pkgInfo.ExtractFile(FMDevicePath, text2, overwriteExistingFiles);
				FeatureManifest.ValidateAndLoad(ref _fm, text2, new IULogger());
			}
			finally
			{
				FileUtils.DeleteTree(tempDirectory2);
			}
			return _fm;
		}

		private string ProcessEnvs(string source, string cpuType, string buildType)
		{
			string text = source;
			if (!string.IsNullOrWhiteSpace(cpuType))
			{
				text = text.Replace("$(cputype)", cpuType, StringComparison.OrdinalIgnoreCase);
			}
			if (!string.IsNullOrWhiteSpace(buildType))
			{
				text = text.Replace("$(buildtype)", buildType, StringComparison.OrdinalIgnoreCase);
			}
			return Environment.ExpandEnvironmentVariables(text);
		}

		public bool ExistsUnder(string msPackageRoot, string cpuType, string buildType)
		{
			return LongPathFile.Exists(GetPackagePath(msPackageRoot, cpuType, buildType));
		}
	}
}
