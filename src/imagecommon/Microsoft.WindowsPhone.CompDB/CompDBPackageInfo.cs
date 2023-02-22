using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	public class CompDBPackageInfo
	{
		public enum CompDBPackageInfoComparison
		{
			Standard,
			IgnorePayloadHashes,
			IgnorePayloadPaths,
			OnlyUniqueID,
			OnlyUniqueIDAndFeatureID
		}

		public enum SatelliteTypes
		{
			Base,
			Language,
			Resolution,
			LangModel
		}

		private BuildCompDB _parentDB;

		[XmlAttribute]
		public string ID;

		[XmlAttribute]
		[DefaultValue("MainOS")]
		public string Partition = "MainOS";

		[XmlAttribute]
		[DefaultValue(ReleaseType.Production)]
		public ReleaseType ReleaseType = ReleaseType.Production;

		[XmlAttribute]
		[DefaultValue(OwnerType.Microsoft)]
		public OwnerType OwnerType = OwnerType.Microsoft;

		[XmlAttribute]
		[DefaultValue(SatelliteTypes.Base)]
		public SatelliteTypes SatelliteType;

		[XmlAttribute]
		[DefaultValue(null)]
		public string SatelliteValue;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool Encrypted;

		[XmlAttribute]
		public string PublicKeyToken;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool BinaryPartition;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool SkipForPublishing;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool SkipForPRSSigning;

		[XmlAttribute]
		[DefaultValue(false)]
		public bool UserInstallable;

		[XmlAttribute]
		[DefaultValue(null)]
		public string BuildArchOverride;

		[XmlArrayItem(ElementName = "PayloadItem", Type = typeof(CompDBPayloadInfo), IsNullable = false)]
		[XmlArray]
		public List<CompDBPayloadInfo> Payload = new List<CompDBPayloadInfo>();

		[XmlIgnore]
		public BuildCompDB ParentDB => _parentDB;

		[XmlIgnore]
		public string CBSAssemblyIdentity => string.Concat(BuildArchOverride + "~", ID, "~");

		[XmlIgnore]
		public string KeyForm
		{
			get
			{
				string text = ((SatelliteType == SatelliteTypes.Language || SatelliteType == SatelliteTypes.LangModel) ? SatelliteValue : "");
				return $"{ID}~{PublicKeyToken}~{BuildArch}~{text}~{VersionStr}";
			}
		}

		[XmlIgnore]
		public string BuildArch
		{
			get
			{
				if (!string.IsNullOrEmpty(BuildArchOverride))
				{
					return BuildArchOverride;
				}
				if (_parentDB != null)
				{
					return _parentDB.BuildArch;
				}
				return "";
			}
		}

		[XmlAttribute("Version")]
		public string VersionStr
		{
			get
			{
				return Version.ToString();
			}
			set
			{
				VersionInfo versionInfo;
				if (!VersionInfo.TryParse(value, out versionInfo))
				{
					throw new ImageCommonException($"ImageCommon::CompDBPackageInfo!VersionStr: Package {ID}'s version '{value}' cannot be parsed.");
				}
				Version = versionInfo;
			}
		}

		[XmlIgnore]
		public VersionInfo Version { get; set; }

		[XmlIgnore]
		public CompDBPayloadInfo FirstPayloadItem
		{
			get
			{
				if (Payload.Count > 0)
				{
					return Payload[0];
				}
				return null;
			}
		}

		[XmlIgnore]
		public string BuildInfo
		{
			get
			{
				if (_parentDB == null)
				{
					return "";
				}
				return _parentDB.BuildInfo;
			}
		}

		public CompDBPackageInfo()
		{
		}

		public CompDBPackageInfo(CompDBPackageInfo pkg)
		{
			UserInstallable = pkg.UserInstallable;
			ID = pkg.ID;
			OwnerType = pkg.OwnerType;
			Partition = pkg.Partition;
			Version = pkg.Version;
			ReleaseType = pkg.ReleaseType;
			SatelliteType = pkg.SatelliteType;
			SatelliteValue = pkg.SatelliteValue;
			PublicKeyToken = pkg.PublicKeyToken;
			BinaryPartition = pkg.BinaryPartition;
			BuildArchOverride = pkg.BuildArchOverride;
			Encrypted = pkg.Encrypted;
			SkipForPRSSigning = pkg.SkipForPRSSigning;
			SkipForPublishing = pkg.SkipForPublishing;
			UserInstallable = pkg.UserInstallable;
			foreach (CompDBPayloadInfo item in pkg.Payload)
			{
				Payload.Add(new CompDBPayloadInfo(item));
			}
		}

		public CompDBPackageInfo(IPkgInfo pkgInfo, string packagePath, string msPackageRoot, string sourceFMFile, BuildCompDB parentDB, bool generateHash, bool isUserInstallable)
		{
			SetValues(pkgInfo, packagePath, msPackageRoot, sourceFMFile, parentDB, generateHash, isUserInstallable);
		}

		public CompDBPackageInfo(FeatureManifest.FMPkgInfo pkgInfo, FeatureManifest fm, string fmFilename, string msPackageRoot, BuildCompDB parentDB, bool generateHash, bool isUserInstallable)
		{
			SetValues(pkgInfo, fm.OwnerType, fm.ReleaseType, fmFilename, msPackageRoot, parentDB, generateHash, isUserInstallable);
		}

		public CompDBPackageInfo(FeatureManifest.FMPkgInfo pkgInfo, FMCollectionItem fmCollectionItem, string msPackageRoot, BuildCompDB parentDB, bool generateHash, bool isUserInstallable)
		{
			SetValues(pkgInfo, fmCollectionItem.ownerType, fmCollectionItem.releaseType, Path.GetFileName(fmCollectionItem.Path), msPackageRoot, parentDB, generateHash, isUserInstallable);
		}

		public CompDBPackageInfo SetParentDB(BuildCompDB parentDB)
		{
			_parentDB = parentDB;
			Payload = Payload.Select((CompDBPayloadInfo pay) => pay.SetParentPkg(this)).ToList();
			return this;
		}

		public void SetPackageHash(string packageFile)
		{
			if (Payload.Count != 1)
			{
				throw new ImageCommonException($"ImageCommon::CompDBPackageInfo!SetPackageHash: The Package payload must have one entry to call this function. PackageFile '{packageFile}'.");
			}
			FirstPayloadItem.SetPayloadHash(packageFile);
		}

		public List<CompDBPublishingPackageInfo> GetPublishingPackages()
		{
			List<CompDBPublishingPackageInfo> list = new List<CompDBPublishingPackageInfo>();
			foreach (CompDBPayloadInfo item in Payload)
			{
				list.Add(new CompDBPublishingPackageInfo(item));
			}
			return list;
		}

		public static string GetPackageHash(string packageFile)
		{
			if (!LongPathFile.Exists(packageFile))
			{
				throw new ImageCommonException($"ImageCommon::CompDBPackageInfo!GetPackageHash: Package {packageFile} does not exist");
			}
			return Convert.ToBase64String(PackageTools.CalculateFileHash(packageFile));
		}

		public static long GetPackageSize(string packageFile)
		{
			long num = 0L;
			try
			{
				return new FileInfo(packageFile).Length;
			}
			catch
			{
				return LongPathFile.ReadAllBytes(packageFile).Length;
			}
		}

		public CompDBPayloadInfo FindPayload(string path)
		{
			return Payload.FirstOrDefault((CompDBPayloadInfo pay) => pay.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
		}

		public static FeatureManifest.PackageGroups GetFMGroupFromFeatureID(string featureID)
		{
			FeatureManifest.PackageGroups result = FeatureManifest.PackageGroups.BASE;
			if (!string.IsNullOrEmpty(featureID) && featureID.Contains("_"))
			{
				string text = featureID.Substring(0, featureID.IndexOf('_'));
				if (!Enum.TryParse<FeatureManifest.PackageGroups>(text, out result))
				{
					if (text.Equals("MS", StringComparison.OrdinalIgnoreCase))
					{
						result = FeatureManifest.PackageGroups.MSFEATURE;
					}
					else if (text.Equals("OEM", StringComparison.OrdinalIgnoreCase))
					{
						result = FeatureManifest.PackageGroups.OEMFEATURE;
					}
				}
			}
			return result;
		}

		public static string GetFMGroupValueFromFeatureID(string featureID)
		{
			FeatureManifest.PackageGroups result = FeatureManifest.PackageGroups.BASE;
			string result2 = "";
			if (!string.IsNullOrEmpty(featureID) && featureID.Contains("_"))
			{
				string text = featureID.Substring(0, featureID.IndexOf('_'));
				if (!Enum.TryParse<FeatureManifest.PackageGroups>(text, out result))
				{
					if (text.Equals("MS", StringComparison.OrdinalIgnoreCase))
					{
						result = FeatureManifest.PackageGroups.MSFEATURE;
						result2 = featureID.Substring("MS_".Length);
					}
					else if (text.Equals("OEM", StringComparison.OrdinalIgnoreCase))
					{
						result = FeatureManifest.PackageGroups.OEMFEATURE;
						result2 = featureID.Substring("OEM_".Length);
					}
				}
				else
				{
					result2 = featureID.Substring(text.Length + 1);
				}
			}
			return result2;
		}

		public bool Equals(CompDBPackageInfo pkg, CompDBPackageInfoComparison compareType)
		{
			if (!ID.Equals(pkg.ID, StringComparison.OrdinalIgnoreCase) || !Partition.Equals(pkg.Partition, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (compareType == CompDBPackageInfoComparison.OnlyUniqueID)
			{
				return true;
			}
			if (UserInstallable != pkg.UserInstallable || OwnerType != pkg.OwnerType || ReleaseType != pkg.ReleaseType || SatelliteType != pkg.SatelliteType || !string.Equals(SatelliteValue, pkg.SatelliteValue, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (compareType == CompDBPackageInfoComparison.IgnorePayloadPaths)
			{
				return true;
			}
			if (Payload.Count != pkg.Payload.Count)
			{
				return false;
			}
			foreach (CompDBPayloadInfo item in Payload)
			{
				bool flag = false;
				foreach (CompDBPayloadInfo item2 in pkg.Payload)
				{
					if (string.Equals(item.Path, item2.Path, StringComparison.OrdinalIgnoreCase))
					{
						flag = true;
						if (compareType != CompDBPackageInfoComparison.IgnorePayloadHashes && !string.Equals(item.PayloadHash, item2.PayloadHash))
						{
							return false;
						}
						break;
					}
				}
				if (!flag)
				{
					return false;
				}
			}
			return true;
		}

		public int GetHashCode(CompDBPackageInfoComparison compareType)
		{
			int hashCode = ID.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
			hashCode ^= Partition.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
			if (compareType != CompDBPackageInfoComparison.OnlyUniqueIDAndFeatureID && compareType != CompDBPackageInfoComparison.OnlyUniqueID)
			{
				hashCode ^= UserInstallable.GetHashCode();
				hashCode ^= OwnerType.GetHashCode();
				hashCode ^= ReleaseType.GetHashCode();
				hashCode ^= SatelliteType.GetHashCode();
				if (!string.IsNullOrEmpty(SatelliteValue))
				{
					hashCode ^= SatelliteValue.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
				}
				if (compareType != CompDBPackageInfoComparison.IgnorePayloadPaths)
				{
					foreach (CompDBPayloadInfo item in Payload)
					{
						hashCode ^= item.Path.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
					}
				}
				if (compareType != CompDBPackageInfoComparison.IgnorePayloadHashes)
				{
					foreach (CompDBPayloadInfo item2 in Payload)
					{
						if (!string.IsNullOrEmpty(item2.PayloadHash))
						{
							hashCode ^= item2.PayloadHash.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
						}
					}
					return hashCode;
				}
			}
			return hashCode;
		}

		public CompDBPackageInfo ClearPackageHashes()
		{
			foreach (CompDBPayloadInfo item in Payload)
			{
				item.ClearPayloadHash();
			}
			return this;
		}

		public CompDBPackageInfo SetPath(string path)
		{
			if (Payload.Count != 1)
			{
				throw new ImageCommonException($"ImageCommon::CompDBPackageInfo!SetPath: The Package payload must have one entry to call this function. Path '{path}'.");
			}
			FirstPayloadItem.SetPath(path);
			return this;
		}

		public CompDBPackageInfo SetPreviousPath(string path)
		{
			if (Payload.Count != 1)
			{
				throw new ImageCommonException($"ImageCommon::CompDBPackageInfo!SetPreviousPath: The Package payload must have one entry to call this function. Path '{path}'.");
			}
			FirstPayloadItem.SetPreviousPath(path);
			return this;
		}

		public override string ToString()
		{
			return ID;
		}

		private void SetValues(IPkgInfo pkgInfo, string packagePath, string msPackageRoot, string sourceFMFile, BuildCompDB parentDB, bool generateHash = false, bool isUserInstallable = false)
		{
			SatelliteTypes pkgSatelliteType = SatelliteTypes.Base;
			string pkgSatelliteValue = null;
			if (!string.IsNullOrEmpty(pkgInfo.Culture))
			{
				pkgSatelliteType = SatelliteTypes.Language;
				pkgSatelliteValue = pkgInfo.Culture;
			}
			else if (!string.IsNullOrEmpty(pkgInfo.Resolution))
			{
				pkgSatelliteType = SatelliteTypes.Resolution;
				pkgSatelliteValue = pkgInfo.Resolution;
			}
			string buildArch = CpuString(pkgInfo.ComplexCpuType);
			SetValues(packagePath, pkgInfo.Name, pkgInfo.Partition, buildArch, pkgInfo.Version, pkgInfo.PublicKey, pkgInfo.IsBinaryPartition, pkgSatelliteType, pkgSatelliteValue, pkgInfo.OwnerType, pkgInfo.ReleaseType, sourceFMFile, msPackageRoot, parentDB, generateHash, isUserInstallable);
		}

		public static string CpuString(CpuId cpuId)
		{
			switch (cpuId)
			{
			case CpuId.AMD64_X86:
				return "wow64";
			case CpuId.ARM64_ARM:
				return "arm64.arm";
			case CpuId.ARM64_X86:
				return "arm64.x86";
			default:
				return cpuId.ToString().ToLower(CultureInfo.InvariantCulture);
			}
		}

		public static SatelliteTypes GetSatelliteTypeFromFMPkgInfo(FeatureManifest.FMPkgInfo fmPkgInfo, IPkgInfo pkgInfo)
		{
			SatelliteTypes result = SatelliteTypes.Base;
			if (!string.IsNullOrEmpty(fmPkgInfo.Language))
			{
				result = SatelliteTypes.Language;
			}
			else if (!string.IsNullOrEmpty(fmPkgInfo.Resolution))
			{
				result = SatelliteTypes.Resolution;
			}
			else if (fmPkgInfo.FMGroup == FeatureManifest.PackageGroups.KEYBOARD || fmPkgInfo.FMGroup == FeatureManifest.PackageGroups.SPEECH)
			{
				result = SatelliteTypes.LangModel;
			}
			else if (!string.IsNullOrEmpty(pkgInfo.Culture) && pkgInfo.OwnerType == OwnerType.Microsoft && (fmPkgInfo.FeatureID.StartsWith("MS_SPEECHSYSTEM_", StringComparison.OrdinalIgnoreCase) || fmPkgInfo.FeatureID.StartsWith("MS_SPEECHDATA_", StringComparison.OrdinalIgnoreCase)))
			{
				result = SatelliteTypes.LangModel;
			}
			return result;
		}

		private void SetValues(FeatureManifest.FMPkgInfo pkgInfo, OwnerType fmOwnerType, ReleaseType fmReleaseType, string fmFilename, string msPackageRoot, BuildCompDB parentDB, bool generateHash = false, bool isUserInstallable = false)
		{
			string text = Path.ChangeExtension(pkgInfo.PackagePath, PkgConstants.c_strCBSPackageExtension);
			IPkgInfo pkgInfo2;
			if (LongPathFile.Exists(text))
			{
				pkgInfo2 = Package.LoadFromCab(text);
			}
			else
			{
				if (!LongPathFile.Exists(pkgInfo.PackagePath))
				{
					throw new ImageCommonException($"ImageCommon::CompDBPackageInfo!SetValues: Package file '{pkgInfo.PackagePath}' could not be found.");
				}
				pkgInfo2 = Package.LoadFromCab(pkgInfo.PackagePath);
			}
			SatelliteTypes satelliteTypeFromFMPkgInfo = GetSatelliteTypeFromFMPkgInfo(pkgInfo, pkgInfo2);
			string pkgSatelliteValue = null;
			switch (satelliteTypeFromFMPkgInfo)
			{
			case SatelliteTypes.Language:
				pkgSatelliteValue = pkgInfo.Language;
				break;
			case SatelliteTypes.Resolution:
				pkgSatelliteValue = pkgInfo.Resolution;
				break;
			case SatelliteTypes.LangModel:
				pkgSatelliteValue = pkgInfo2.Culture;
				break;
			}
			string buildArch = CpuString(pkgInfo2.ComplexCpuType);
			SetValues(pkgInfo.PackagePath, pkgInfo2.Name, pkgInfo2.Partition, buildArch, pkgInfo2.Version, pkgInfo2.PublicKey, pkgInfo2.IsBinaryPartition, satelliteTypeFromFMPkgInfo, pkgSatelliteValue, pkgInfo2.OwnerType, pkgInfo2.ReleaseType, fmFilename, msPackageRoot, parentDB, generateHash, isUserInstallable);
		}

		private void SetValues(string pkgPath, string pkgName, string pkgPartition, string buildArch, VersionInfo? pkgVersion, string pkgPublicKey, bool binaryPartition, SatelliteTypes pkgSatelliteType, string pkgSatelliteValue, OwnerType fmOwnerType, ReleaseType fmReleaseType, string fmFilename, string msPackageRoot, BuildCompDB parentDB, bool generateHash = false, bool isUserInstallable = false)
		{
			(new char[1])[0] = '\\';
			ID = pkgName;
			Partition = pkgPartition;
			if (pkgVersion.HasValue)
			{
				Version = pkgVersion.Value;
			}
			PublicKeyToken = pkgPublicKey;
			BinaryPartition = binaryPartition;
			CompDBPayloadInfo item = new CompDBPayloadInfo(pkgPath, msPackageRoot, this, generateHash);
			Payload.Add(item);
			OwnerType = fmOwnerType;
			ReleaseType = fmReleaseType;
			UserInstallable = isUserInstallable;
			SatelliteType = pkgSatelliteType;
			SatelliteValue = pkgSatelliteValue;
			if (!parentDB.BuildArch.Equals(buildArch, StringComparison.OrdinalIgnoreCase))
			{
				BuildArchOverride = buildArch;
			}
			_parentDB = parentDB;
		}

		public CompDBPackageInfo ClearSkipForPublishing()
		{
			SkipForPublishing = false;
			return this;
		}

		public CompDBPackageInfo SetSkipForPublishing()
		{
			SkipForPublishing = true;
			return this;
		}

		public CompDBPackageInfo ClearSkipForPRSSigning()
		{
			SkipForPRSSigning = false;
			return this;
		}

		public CompDBPackageInfo SetSkipForPRSSigning()
		{
			if (ReleaseType == ReleaseType.Production)
			{
				SkipForPRSSigning = true;
			}
			return this;
		}

		public CompDBPackageInfo SetPayloadType(CompDBPayloadInfo.PayloadTypes payloadType)
		{
			foreach (CompDBPayloadInfo item in Payload)
			{
				item.PayloadType = payloadType;
			}
			return this;
		}
	}
}
