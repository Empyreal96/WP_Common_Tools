using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class PublishingPackageInfo
	{
		public enum PublishingPackageInfoComparison
		{
			IgnorePaths,
			OnlyUniqueID,
			OnlyUniqueIDAndFeatureID
		}

		public enum UpdateTypes
		{
			PKR,
			Diff,
			Canonical
		}

		public enum SatelliteTypes
		{
			Base,
			Language,
			Resolution
		}

		public static string UserInstallableFeatureIDPrefix = "USERINSTALLABLE_";

		public string ID;

		[DefaultValue(false)]
		public bool IsFeatureIdentifierPackage;

		public string Path;

		[DefaultValue(null)]
		public string PreviousPath;

		[DefaultValue("MainOS")]
		public string Partition = "MainOS";

		public string FeatureID;

		public string FMID;

		public VersionInfo Version = VersionInfo.Empty;

		[DefaultValue(ReleaseType.Production)]
		public ReleaseType ReleaseType = ReleaseType.Production;

		[DefaultValue(OwnerType.Microsoft)]
		public OwnerType OwnerType = OwnerType.Microsoft;

		[DefaultValue(false)]
		public bool UserInstallable;

		[DefaultValue(SatelliteTypes.Base)]
		public SatelliteTypes SatelliteType;

		[DefaultValue(null)]
		public string SatelliteValue;

		[DefaultValue(UpdateTypes.Canonical)]
		public UpdateTypes UpdateType = UpdateTypes.Canonical;

		public string SourceFMFile;

		[XmlIgnore]
		public string FeatureIDWithFMID
		{
			get
			{
				if (!string.IsNullOrEmpty(FMID))
				{
					return FeatureManifest.GetFeatureIDWithFMID(FeatureID, FMID);
				}
				return FeatureID;
			}
		}

		[XmlIgnore]
		public FeatureManifest.PackageGroups FMGroup
		{
			get
			{
				FeatureManifest.PackageGroups result = FeatureManifest.PackageGroups.BASE;
				if (!string.IsNullOrEmpty(FeatureID) && FeatureID.Contains('_'))
				{
					string text = FeatureID.Substring(0, FeatureID.IndexOf('_'));
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
		}

		public PublishingPackageInfo()
		{
		}

		public PublishingPackageInfo(PublishingPackageInfo pkg)
		{
			UserInstallable = pkg.UserInstallable;
			FeatureID = pkg.FeatureID;
			FMID = pkg.FMID;
			ID = pkg.ID;
			IsFeatureIdentifierPackage = pkg.IsFeatureIdentifierPackage;
			OwnerType = pkg.OwnerType;
			Partition = pkg.Partition;
			Version = pkg.Version;
			Path = pkg.Path;
			PreviousPath = pkg.PreviousPath;
			ReleaseType = pkg.ReleaseType;
			SatelliteType = pkg.SatelliteType;
			SatelliteValue = pkg.SatelliteValue;
			SourceFMFile = pkg.SourceFMFile;
			UpdateType = pkg.UpdateType;
		}

		public PublishingPackageInfo(FeatureManifest.FMPkgInfo pkgInfo, FMCollectionItem fmCollectionItem, string msPackageRoot, bool isUserInstallable)
		{
			char[] trimChars = new char[1] { '\\' };
			string packagePath = pkgInfo.PackagePath;
			string text = packagePath;
			int length = packagePath.Length;
			string text2 = packagePath;
			packagePath = text.Substring(0, length - text2.Substring(text2.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)).Length);
			packagePath += ".cab";
			if (!LongPathFile.Exists(pkgInfo.PackagePath) && !LongPathFile.Exists(packagePath))
			{
				throw new FileNotFoundException("ImageCommon!PublishingPackageInfo: The package file '" + pkgInfo.PackagePath + "' could not be found.");
			}
			if (string.IsNullOrEmpty(pkgInfo.ID) || string.IsNullOrEmpty(pkgInfo.Partition) || !pkgInfo.Version.HasValue || pkgInfo.Version == VersionInfo.Empty || (LongPathFile.Exists(packagePath) && string.Compare(packagePath, pkgInfo.PackagePath, StringComparison.OrdinalIgnoreCase) != 0))
			{
				IPkgInfo pkgInfo2 = Package.LoadFromCab(LongPathFile.Exists(packagePath) ? packagePath : pkgInfo.PackagePath);
				ID = pkgInfo2.Name;
				Partition = pkgInfo2.Partition;
				Version = pkgInfo2.Version;
			}
			else
			{
				ID = pkgInfo.ID;
				Partition = pkgInfo.Partition;
				Version = pkgInfo.Version.Value;
			}
			Path = pkgInfo.PackagePath.Replace(msPackageRoot, "", StringComparison.OrdinalIgnoreCase).Trim(trimChars);
			SourceFMFile = System.IO.Path.GetFileName(fmCollectionItem.Path.ToUpper(CultureInfo.InvariantCulture));
			switch (pkgInfo.FMGroup)
			{
			case FeatureManifest.PackageGroups.DEVICELAYOUT:
				FeatureID = pkgInfo.FeatureID.Replace(FeatureManifest.PackageGroups.DEVICELAYOUT.ToString(), FeatureManifest.PackageGroups.SOC.ToString());
				break;
			case FeatureManifest.PackageGroups.OEMDEVICEPLATFORM:
				FeatureID = pkgInfo.FeatureID.Replace(FeatureManifest.PackageGroups.OEMDEVICEPLATFORM.ToString(), FeatureManifest.PackageGroups.DEVICE.ToString(), StringComparison.OrdinalIgnoreCase);
				break;
			default:
				FeatureID = pkgInfo.FeatureID;
				break;
			}
			FMID = fmCollectionItem.ID;
			if (isUserInstallable)
			{
				FeatureID = UserInstallableFeatureIDPrefix + FeatureID;
			}
			OwnerType = fmCollectionItem.ownerType;
			ReleaseType = fmCollectionItem.releaseType;
			UserInstallable = fmCollectionItem.UserInstallable;
			if (!string.IsNullOrEmpty(pkgInfo.Language))
			{
				SatelliteType = SatelliteTypes.Language;
				SatelliteValue = pkgInfo.Language;
			}
			else if (!string.IsNullOrEmpty(pkgInfo.Resolution))
			{
				SatelliteType = SatelliteTypes.Resolution;
				SatelliteValue = pkgInfo.Resolution;
			}
			else
			{
				SatelliteType = SatelliteTypes.Base;
				SatelliteValue = null;
			}
			IsFeatureIdentifierPackage = pkgInfo.FeatureIdentifierPackage;
			UpdateType = UpdateTypes.Canonical;
		}

		public bool Equals(PublishingPackageInfo pkg, PublishingPackageInfoComparison compareType)
		{
			if (!ID.Equals(pkg.ID, StringComparison.OrdinalIgnoreCase) || !Partition.Equals(pkg.Partition, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (compareType == PublishingPackageInfoComparison.OnlyUniqueID)
			{
				return true;
			}
			if (!FeatureID.Equals(pkg.FeatureID, StringComparison.OrdinalIgnoreCase) || !string.Equals(FMID, pkg.FMID, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (compareType == PublishingPackageInfoComparison.OnlyUniqueIDAndFeatureID)
			{
				return true;
			}
			if (UserInstallable != pkg.UserInstallable || OwnerType != pkg.OwnerType || ReleaseType != pkg.ReleaseType || SatelliteType != pkg.SatelliteType || !string.Equals(SatelliteValue, pkg.SatelliteValue, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			if (compareType == PublishingPackageInfoComparison.IgnorePaths)
			{
				return true;
			}
			if (!Path.Equals(pkg.Path, StringComparison.OrdinalIgnoreCase) || !string.Equals(PreviousPath, pkg.PreviousPath, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return true;
		}

		public int GetHashCode(PublishingPackageInfoComparison compareType)
		{
			int hashCode = ID.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
			hashCode ^= Partition.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
			if (compareType != PublishingPackageInfoComparison.OnlyUniqueID)
			{
				hashCode ^= FeatureID.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
				if (!string.IsNullOrEmpty(FMID))
				{
					hashCode ^= FMID.GetHashCode();
				}
			}
			if (compareType != PublishingPackageInfoComparison.OnlyUniqueIDAndFeatureID && compareType != PublishingPackageInfoComparison.OnlyUniqueID)
			{
				hashCode ^= UserInstallable.GetHashCode();
				hashCode ^= OwnerType.GetHashCode();
				hashCode ^= ReleaseType.GetHashCode();
				hashCode ^= SatelliteType.GetHashCode();
				if (!string.IsNullOrEmpty(SatelliteValue))
				{
					hashCode ^= SatelliteValue.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
				}
				if (compareType != 0)
				{
					hashCode ^= Path.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
					if (!string.IsNullOrEmpty(PreviousPath))
					{
						hashCode ^= PreviousPath.ToUpper(CultureInfo.InvariantCulture).GetHashCode();
					}
				}
			}
			return hashCode;
		}

		public PublishingPackageInfo SetPreviousPath(string path)
		{
			PreviousPath = path;
			return this;
		}

		public PublishingPackageInfo SetUpdateType(UpdateTypes type)
		{
			UpdateType = type;
			return this;
		}

		public override string ToString()
		{
			return ID + " (" + FeatureIDWithFMID + ") : " + UpdateType;
		}
	}
}
