using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "FeatureManifest", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class FeatureManifest
	{
		public enum PackageGroups
		{
			BASE,
			BOOTUI,
			BOOTLOCALE,
			RELEASE,
			DEVICELAYOUT,
			OEMDEVICEPLATFORM,
			SV,
			SOC,
			DEVICE,
			MSFEATURE,
			OEMFEATURE,
			KEYBOARD,
			SPEECH,
			PRERELEASE
		}

		public class FMPkgInfo
		{
			[Flags]
			public enum CorePackageTypeEnum
			{
				NonCore = 0,
				RetailCore = 1,
				NonRetailCore = 2,
				TestCore = 4
			}

			public const string ReleaseType_Production = "Production";

			public const string ReleaseType_Test = "Test";

			public static char[] separators = new char[1] { ';' };

			public static string[] ProductionCoreOptionalFeatures = new string[1] { "PRODUCTION_CORE" };

			public static string[] TestCoreOptionalFeatures = new string[2] { "MOBLECORE_TEST", "BOOTSEQUENCE_TEST" };

			public PackageGroups FMGroup;

			public string GroupValue;

			public string Language;

			public string Resolution;

			public string Wow;

			public string PackagePath;

			public string RawBasePath;

			public string ID;

			public bool FeatureIdentifierPackage;

			public string Partition = string.Empty;

			public VersionInfo? Version;

			public string PublicKey;

			public bool BinaryPartition;

			public CorePackageTypeEnum CorePackageType
			{
				get
				{
					if (FMGroup.Equals(PackageGroups.BASE) || FMGroup.Equals(PackageGroups.KEYBOARD) || FMGroup.Equals(PackageGroups.SPEECH) || FMGroup.Equals(PackageGroups.BOOTUI) || FMGroup.Equals(PackageGroups.BOOTLOCALE))
					{
						return CorePackageTypeEnum.RetailCore | CorePackageTypeEnum.NonRetailCore | CorePackageTypeEnum.TestCore;
					}
					if (FMGroup == PackageGroups.RELEASE)
					{
						if (string.Equals(GroupValue, "Production", StringComparison.OrdinalIgnoreCase))
						{
							return CorePackageTypeEnum.RetailCore;
						}
						return CorePackageTypeEnum.NonRetailCore | CorePackageTypeEnum.TestCore;
					}
					if (IsOptionalProductionCore())
					{
						return CorePackageTypeEnum.NonRetailCore;
					}
					if (IsOptionalTestCore())
					{
						return CorePackageTypeEnum.TestCore;
					}
					return CorePackageTypeEnum.NonCore;
				}
			}

			public string FeatureID
			{
				get
				{
					switch (FMGroup)
					{
					case PackageGroups.MSFEATURE:
						return "MS_" + GroupValue;
					case PackageGroups.OEMFEATURE:
						return "OEM_" + GroupValue;
					case PackageGroups.BASE:
						return PackageGroups.BASE.ToString();
					default:
						return FMGroup.ToString() + "_" + GroupValue.ToUpper(CultureInfo.InvariantCulture);
					}
				}
			}

			public FMPkgInfo()
			{
			}

			public FMPkgInfo(FMPkgInfo srcPkg)
			{
				FeatureIdentifierPackage = srcPkg.FeatureIdentifierPackage;
				FMGroup = srcPkg.FMGroup;
				GroupValue = srcPkg.GroupValue;
				ID = srcPkg.ID;
				Language = srcPkg.Language;
				Partition = srcPkg.Partition;
				PackagePath = srcPkg.PackagePath;
				RawBasePath = srcPkg.RawBasePath;
				Resolution = srcPkg.Resolution;
				Version = srcPkg.Version;
			}

			public FMPkgInfo(PkgFile pkg, string groupValue)
			{
				PackagePath = pkg.PackagePath;
				RawBasePath = pkg.RawPackagePath;
				ID = pkg.ID;
				FeatureIdentifierPackage = pkg.FeatureIdentifierPackage;
				SetVersion(pkg.Version);
				Partition = pkg.Partition;
				FMGroup = pkg.FMGroup;
				GroupValue = groupValue;
				PublicKey = pkg.PublicKey;
				BinaryPartition = pkg.BinaryPartition;
			}

			public FMPkgInfo(string packagePath, string id, PackageGroups fmGroup, string groupValue, string partition, string language, string resolution, bool featureIdentifierPackage, VersionInfo? version)
			{
				PackagePath = packagePath;
				ID = id;
				FMGroup = fmGroup;
				GroupValue = groupValue;
				Language = language;
				Partition = partition;
				Resolution = resolution;
				FeatureIdentifierPackage = featureIdentifierPackage;
				Version = version;
			}

			public string[] GetGroupValueList()
			{
				string[] result = new string[0];
				if (FMGroup == PackageGroups.MSFEATURE || FMGroup == PackageGroups.OEMFEATURE)
				{
					result = GroupValue.Split(separators, StringSplitOptions.RemoveEmptyEntries);
				}
				return result;
			}

			public bool IsOptionalCore()
			{
				if (!IsOptionalProductionCore())
				{
					return IsOptionalTestCore();
				}
				return true;
			}

			private bool IsOptionalProductionCore()
			{
				if (ProductionCoreOptionalFeatures.Intersect(GetGroupValueList(), IgnoreCase).Count() > 0)
				{
					return true;
				}
				return false;
			}

			private bool IsOptionalTestCore()
			{
				if (TestCoreOptionalFeatures.Intersect(GetGroupValueList(), IgnoreCase).Count() > 0)
				{
					return true;
				}
				return false;
			}

			public void SetVersion(string versionStr)
			{
				if (!string.IsNullOrEmpty(versionStr))
				{
					VersionInfo versionInfo = default(VersionInfo);
					if (VersionInfo.TryParse(versionStr, out versionInfo))
					{
						Version = versionInfo;
					}
					else
					{
						Version = null;
					}
				}
				else
				{
					Version = null;
				}
			}

			public override string ToString()
			{
				return $"{ID} ({FeatureID})";
			}
		}

		public static readonly string CPUType_ARM = CpuId.ARM.ToString();

		public static readonly string CPUType_X86 = CpuId.X86.ToString();

		public static readonly string CPUType_ARM64 = CpuId.ARM64.ToString();

		public static readonly string CPUType_AMD64 = CpuId.AMD64.ToString();

		public const string FMRevisionNumber = "1";

		public const string FMSchemaVersion = "1.2";

		public const string BuildType_FRE = "fre";

		public const string BuildType_CHK = "chk";

		public const string Prerelease_Protected = "protected";

		public const string Prerelease_Protected_Replacement = "replacement";

		public const string MSPackageRootVariable = "$(mspackageroot)";

		private static StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

		[XmlAttribute]
		public string Revision;

		[XmlAttribute]
		public string SchemaVersion;

		[XmlAttribute]
		[DefaultValue(ReleaseType.Invalid)]
		public ReleaseType ReleaseType;

		[XmlAttribute]
		[DefaultValue(OwnerType.Invalid)]
		public OwnerType OwnerType;

		private string _owner;

		[XmlAttribute]
		public string ID;

		[XmlAttribute]
		public string BuildInfo;

		[XmlAttribute]
		public string BuildID;

		private VersionInfo? _osVersion;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(PkgFile), IsNullable = false)]
		[XmlArray]
		public List<PkgFile> BasePackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(PkgFile), IsNullable = false)]
		[XmlArray]
		public List<PkgFile> CPUPackages;

		public BootUIPkgFile BootUILanguagePackageFile;

		public BootLocalePkgFile BootLocalePackageFile;

		public FMFeatures Features;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(ReleasePkgFile), IsNullable = false)]
		[XmlArray]
		public List<ReleasePkgFile> ReleasePackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(PrereleasePkgFile), IsNullable = false)]
		[XmlArray]
		public List<PrereleasePkgFile> PrereleasePackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(OEMDevicePkgFile), IsNullable = false)]
		[XmlArray]
		public List<OEMDevicePkgFile> OEMDevicePlatformPackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(DeviceLayoutPkgFile), IsNullable = false)]
		[XmlArray]
		public List<DeviceLayoutPkgFile> DeviceLayoutPackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(SOCPkgFile), IsNullable = false)]
		[XmlArray]
		public List<SOCPkgFile> SOCPackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(SVPkgFile), IsNullable = false)]
		[XmlArray]
		public List<SVPkgFile> SVPackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(DevicePkgFile), IsNullable = false)]
		[XmlArray]
		public List<DevicePkgFile> DeviceSpecificPackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(SpeechPkgFile), IsNullable = false)]
		[XmlArray]
		public List<SpeechPkgFile> SpeechPackages;

		[XmlArrayItem(ElementName = "PackageFile", Type = typeof(KeyboardPkgFile), IsNullable = false)]
		[XmlArray]
		public List<KeyboardPkgFile> KeyboardPackages;

		private OEMInput _oemInput;

		[XmlIgnore]
		public string SourceFile;

		[XmlAttribute("OwnerName")]
		[DefaultValue(null)]
		public string Owner
		{
			get
			{
				if (OwnerType == OwnerType.Microsoft)
				{
					return OwnerType.Microsoft.ToString();
				}
				return _owner;
			}
			set
			{
				if (OwnerType == OwnerType.Microsoft)
				{
					_owner = null;
				}
				else
				{
					_owner = value;
				}
			}
		}

		[XmlAttribute]
		[DefaultValue(null)]
		public string OSVersion
		{
			get
			{
				if (!_osVersion.HasValue || string.IsNullOrEmpty(_osVersion.ToString()))
				{
					return null;
				}
				return _osVersion.ToString();
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_osVersion = null;
					return;
				}
				string[] array = value.Split('.');
				ushort[] array2 = new ushort[4];
				for (int i = 0; i < Math.Min(array.Count(), 4); i++)
				{
					if (string.IsNullOrEmpty(array[i]))
					{
						array2[i] = 0;
					}
					else
					{
						array2[i] = ushort.Parse(array[i]);
					}
				}
				if (array.Count() != 4)
				{
					_osVersion = null;
				}
				else
				{
					_osVersion = new VersionInfo(array2[0], array2[1], array2[2], array2[3]);
				}
			}
		}

		public IEnumerable<PkgFile> AllPackages => _allPackages;

		private IEnumerable<PkgFile> _allPackages
		{
			get
			{
				List<PkgFile> list = new List<PkgFile>();
				if (BootUILanguagePackageFile != null)
				{
					list.Add(BootUILanguagePackageFile);
				}
				if (BootLocalePackageFile != null)
				{
					list.Add(BootLocalePackageFile);
				}
				if (BasePackages != null)
				{
					list.AddRange(BasePackages);
				}
				if (Features != null)
				{
					if (Features.Microsoft != null)
					{
						list.AddRange(Features.Microsoft);
					}
					if (Features.OEM != null)
					{
						list.AddRange(Features.OEM);
					}
				}
				if (SVPackages != null)
				{
					list.AddRange(SVPackages);
				}
				if (SOCPackages != null)
				{
					list.AddRange(SOCPackages);
				}
				if (OEMDevicePlatformPackages != null)
				{
					list.AddRange(OEMDevicePlatformPackages);
				}
				if (DeviceLayoutPackages != null)
				{
					list.AddRange(DeviceLayoutPackages);
				}
				if (DeviceSpecificPackages != null)
				{
					list.AddRange(DeviceSpecificPackages);
				}
				if (ReleasePackages != null)
				{
					list.AddRange(ReleasePackages);
				}
				if (PrereleasePackages != null)
				{
					list.AddRange(PrereleasePackages);
				}
				if (KeyboardPackages != null)
				{
					list.AddRange(KeyboardPackages);
				}
				if (SpeechPackages != null)
				{
					list.AddRange(SpeechPackages);
				}
				return list;
			}
		}

		[XmlIgnore]
		public OEMInput OemInput
		{
			get
			{
				return _oemInput;
			}
			set
			{
				_oemInput = value;
				foreach (PkgFile allPackage in _allPackages)
				{
					allPackage.OemInput = _oemInput;
				}
			}
		}

		public FeatureManifest()
		{
		}

		public FeatureManifest(FeatureManifest srcFM)
		{
			if (srcFM == null)
			{
				return;
			}
			if (srcFM.Revision != null)
			{
				Revision = srcFM.Revision;
			}
			if (srcFM.SchemaVersion != null)
			{
				SchemaVersion = srcFM.SchemaVersion;
			}
			if (srcFM.BasePackages != null)
			{
				BasePackages = new List<PkgFile>(srcFM.BasePackages);
			}
			if (srcFM.BootLocalePackageFile != null)
			{
				BootLocalePackageFile = srcFM.BootLocalePackageFile;
			}
			if (srcFM.BootUILanguagePackageFile != null)
			{
				BootUILanguagePackageFile = srcFM.BootUILanguagePackageFile;
			}
			if (srcFM.CPUPackages != null)
			{
				CPUPackages = new List<PkgFile>(srcFM.CPUPackages);
			}
			if (srcFM.DeviceLayoutPackages != null)
			{
				DeviceLayoutPackages = new List<DeviceLayoutPkgFile>(srcFM.DeviceLayoutPackages);
			}
			if (srcFM.DeviceSpecificPackages != null)
			{
				DeviceSpecificPackages = new List<DevicePkgFile>(srcFM.DeviceSpecificPackages);
			}
			if (srcFM.Features != null)
			{
				Features = new FMFeatures();
				if (srcFM.Features.MSFeatureGroups != null)
				{
					Features.MSFeatureGroups = new List<FMFeatureGrouping>(srcFM.Features.MSFeatureGroups);
				}
				if (srcFM.Features.Microsoft != null)
				{
					Features.Microsoft = new List<MSOptionalPkgFile>(srcFM.Features.Microsoft);
				}
				if (srcFM.Features.OEM != null)
				{
					Features.OEM = new List<OEMOptionalPkgFile>(srcFM.Features.OEM);
				}
				if (srcFM.Features.OEMFeatureGroups != null)
				{
					Features.OEMFeatureGroups = new List<FMFeatureGrouping>(srcFM.Features.OEMFeatureGroups);
				}
			}
			if (srcFM.KeyboardPackages != null)
			{
				KeyboardPackages = new List<KeyboardPkgFile>(srcFM.KeyboardPackages);
			}
			if (srcFM.OEMDevicePlatformPackages != null)
			{
				OEMDevicePlatformPackages = new List<OEMDevicePkgFile>(srcFM.OEMDevicePlatformPackages);
			}
			if (srcFM.PrereleasePackages != null)
			{
				PrereleasePackages = new List<PrereleasePkgFile>(srcFM.PrereleasePackages);
			}
			if (srcFM.ReleasePackages != null)
			{
				ReleasePackages = new List<ReleasePkgFile>(srcFM.ReleasePackages);
			}
			if (srcFM.SOCPackages != null)
			{
				SOCPackages = new List<SOCPkgFile>(srcFM.SOCPackages);
			}
			if (srcFM.SpeechPackages != null)
			{
				SpeechPackages = new List<SpeechPkgFile>(srcFM.SpeechPackages);
			}
			if (srcFM.SVPackages != null)
			{
				SVPackages = new List<SVPkgFile>(srcFM.SVPackages);
			}
			SourceFile = srcFM.SourceFile;
			OemInput = srcFM.OemInput;
			OSVersion = srcFM.OSVersion;
			ID = srcFM.ID;
			BuildID = srcFM.BuildID;
			BuildInfo = srcFM.BuildInfo;
			ReleaseType = srcFM.ReleaseType;
			OwnerType = srcFM.OwnerType;
			Owner = srcFM.Owner;
		}

		public bool ShouldSerializeCPUPackages()
		{
			return false;
		}

		public bool ShouldSerializeFeatures()
		{
			return Features != null;
		}

		public List<string> GetFeatureIDs(bool fMSFeatures, bool fOEMFeatures)
		{
			List<string> list = new List<string>();
			if (Features != null)
			{
				if (Features.Microsoft != null && fMSFeatures)
				{
					foreach (MSOptionalPkgFile item in Features.Microsoft)
					{
						list.AddRange(item.FeatureIDs);
					}
				}
				if (Features.OEM != null && fOEMFeatures)
				{
					foreach (OEMOptionalPkgFile item2 in Features.OEM)
					{
						list.AddRange(item2.FeatureIDs);
					}
				}
			}
			return list.Distinct(IgnoreCase).ToList();
		}

		private List<FMPkgInfo> GetPackagesFromList(PkgFile pkg, List<string> listValues)
		{
			List<FMPkgInfo> list = new List<FMPkgInfo>();
			foreach (string listValue in listValues)
			{
				FMPkgInfo fMPkgInfo = new FMPkgInfo(pkg, pkg.GroupValue);
				switch (pkg.FMGroup)
				{
				case PackageGroups.BOOTUI:
					fMPkgInfo.PackagePath = pkg.PackagePath.Replace("$(bootuilanguage)", listValue, StringComparison.OrdinalIgnoreCase);
					fMPkgInfo.ID = BootUILanguagePackageFile.ID.Replace("$(bootuilanguage)", listValue, StringComparison.OrdinalIgnoreCase);
					break;
				case PackageGroups.BOOTLOCALE:
					fMPkgInfo.PackagePath = BootLocalePackageFile.PackagePath.Replace("$(bootlocale)", listValue, StringComparison.OrdinalIgnoreCase);
					fMPkgInfo.ID = BootLocalePackageFile.ID.Replace("$(bootlocale)", listValue, StringComparison.OrdinalIgnoreCase);
					break;
				case PackageGroups.KEYBOARD:
				case PackageGroups.SPEECH:
				{
					fMPkgInfo.ID = pkg.ID + PkgFile.DefaultLanguagePattern + listValue;
					string packagePath = pkg.PackagePath;
					string extension = Path.GetExtension(packagePath);
					packagePath = packagePath.Replace(extension, PkgFile.DefaultLanguagePattern + listValue + extension, StringComparison.OrdinalIgnoreCase);
					fMPkgInfo.PackagePath = packagePath;
					break;
				}
				default:
					throw new FeatureAPIException(string.Concat("FeatureAPI!GetPackagesFromList: Called with non supported FMGroup '", pkg.FMGroup, "' for package '", pkg.PackagePath, "'"));
				}
				fMPkgInfo.GroupValue = listValue;
				list.Add(fMPkgInfo);
			}
			return list;
		}

		private List<FMPkgInfo> GetSatellites(PkgFile pkg, List<string> supportedUILanguages, List<string> supportedResolutions, List<CpuId> supportedWowCputypes, string cpuType, string groupValue = null)
		{
			List<FMPkgInfo> list = new List<FMPkgInfo>();
			string groupValue2 = groupValue ?? pkg.GroupValue;
			if (groupValue != null && string.IsNullOrWhiteSpace(groupValue) && pkg.GroupValues.Count() > 1)
			{
				throw new FeatureAPIException(string.Concat("FeatureAPI!GetSatellites: Called with multiple group values '", pkg.GroupValues, "' for package and no override"));
			}
			if (!string.IsNullOrEmpty(pkg.Language) && supportedUILanguages.Count != 0)
			{
				foreach (string supported in PkgFile.GetSupportedList(pkg.Language, supportedUILanguages))
				{
					FMPkgInfo fMPkgInfo = new FMPkgInfo(pkg, groupValue2);
					fMPkgInfo.Language = supported;
					fMPkgInfo.PackagePath = pkg.GetLanguagePackagePath(supported);
					fMPkgInfo.RawBasePath = pkg.RawPackagePath;
					fMPkgInfo.ID = pkg.ID + PkgFile.DefaultLanguagePattern + supported;
					fMPkgInfo.FeatureIdentifierPackage = false;
					fMPkgInfo.PublicKey = pkg.PublicKey;
					list.Add(fMPkgInfo);
				}
			}
			if (!string.IsNullOrEmpty(pkg.Resolution) && supportedResolutions.Count != 0)
			{
				foreach (string supported2 in PkgFile.GetSupportedList(pkg.Resolution, supportedResolutions))
				{
					FMPkgInfo fMPkgInfo2 = new FMPkgInfo(pkg, groupValue2);
					fMPkgInfo2.Resolution = supported2;
					fMPkgInfo2.PackagePath = pkg.GetResolutionPackagePath(supported2);
					fMPkgInfo2.RawBasePath = pkg.RawPackagePath;
					fMPkgInfo2.ID = pkg.ID + PkgFile.DefaultResolutionPattern + supported2;
					fMPkgInfo2.FeatureIdentifierPackage = false;
					list.Add(fMPkgInfo2);
				}
			}
			bool flag = !string.IsNullOrEmpty(pkg.Wow);
			bool flag2 = !string.IsNullOrEmpty(pkg.LangWow);
			bool flag3 = !string.IsNullOrEmpty(pkg.ResWow);
			if ((flag || flag2 || flag3) && supportedWowCputypes.Any())
			{
				List<string> supportedList = PkgFile.GetSupportedList(pkg.Wow, supportedWowCputypes.Select((CpuId cpuid) => cpuid.ToString()).ToList());
				List<FMPkgInfo> list2 = new List<FMPkgInfo>();
				List<string> list3 = ((!flag) ? new List<string>() : (pkg.Wow.Equals("*") ? supportedList : supportedList.Intersect(pkg.Wow.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase).ToList()));
				List<string> list4 = ((!flag2) ? new List<string>() : (pkg.LangWow.Equals("*") ? supportedList : supportedList.Intersect(pkg.LangWow.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase).ToList()));
				List<string> list5 = ((!flag3) ? new List<string>() : (pkg.ResWow.Equals("*") ? supportedList : supportedList.Intersect(pkg.ResWow.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase).ToList()));
				foreach (FMPkgInfo item in list)
				{
					List<string> list6 = new List<string>();
					if (flag2 && !string.IsNullOrEmpty(item.Language))
					{
						list6 = list4;
					}
					else if (flag3 && !string.IsNullOrEmpty(item.Resolution))
					{
						list6 = list5;
					}
					foreach (string item2 in list6)
					{
						string text = (cpuType + "." + item2.ToString()).ToLower(CultureInfo.InvariantCulture);
						FMPkgInfo fMPkgInfo3 = new FMPkgInfo(item);
						fMPkgInfo3.Wow = text;
						string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fMPkgInfo3.PackagePath);
						fMPkgInfo3.PackagePath = fMPkgInfo3.PackagePath.Replace("$(mspackageroot)", "$(mspackageroot)\\Wow\\" + text, StringComparison.OrdinalIgnoreCase);
						fMPkgInfo3.PackagePath = fMPkgInfo3.PackagePath.Replace(fileNameWithoutExtension, fileNameWithoutExtension + PkgFile.DefaultWowPattern + text, StringComparison.OrdinalIgnoreCase);
						fMPkgInfo3.ID = fMPkgInfo3.ID + PkgFile.DefaultWowPattern + text;
						list2.Add(fMPkgInfo3);
					}
				}
				list.AddRange(list2);
				if (flag)
				{
					foreach (string item3 in list3)
					{
						string text2 = cpuType + "." + item3.ToString();
						FMPkgInfo fMPkgInfo4 = new FMPkgInfo(pkg, groupValue2);
						fMPkgInfo4.Wow = text2;
						fMPkgInfo4.PackagePath = pkg.GetWowPackagePath(item3);
						fMPkgInfo4.PackagePath = fMPkgInfo4.PackagePath.Replace("$(mspackageroot)", "$(mspackageroot)\\Wow\\" + text2, StringComparison.OrdinalIgnoreCase);
						fMPkgInfo4.ID = pkg.ID + PkgFile.DefaultWowPattern + text2.ToString();
						fMPkgInfo4.FeatureIdentifierPackage = false;
						list.Add(fMPkgInfo4);
					}
					return list;
				}
			}
			return list;
		}

		public List<string> GetPRSPackages(List<string> supportedUILanguages, List<string> supportedLocales, List<string> supportedResolutions, List<CpuId> supportedWowCpuTypes, string buildType, string cpuType, string msPackageRoot)
		{
			return (from pkg in GetAllPackagesByGroups(supportedUILanguages, supportedLocales, supportedResolutions, supportedWowCpuTypes, buildType, cpuType, msPackageRoot)
				where (!pkg.FMGroup.Equals(PackageGroups.RELEASE) || !string.Equals(pkg.GroupValue, "Test", StringComparison.OrdinalIgnoreCase)) && !pkg.FMGroup.Equals(PackageGroups.OEMFEATURE)
				select pkg.PackagePath).Distinct(IgnoreCase).ToList();
		}

		public List<FMPkgInfo> GetAllPackageByGroups(List<string> supportedUILanguages, List<string> supportedLocales, List<string> supportedResolutions, string buildType, string cpuType, string msPackageRoot)
		{
			return GetAllPackagesByGroups(supportedUILanguages, supportedLocales, supportedResolutions, new List<CpuId>(), buildType, cpuType, msPackageRoot);
		}

		public List<FMPkgInfo> GetAllPackagesByGroups(List<string> supportedUILanguages, List<string> supportedLocales, List<string> supportedResolutions, List<CpuId> supportedWowCpuTypes, string buildType, string cpuType, string msPackageRoot)
		{
			List<FMPkgInfo> packageList = new List<FMPkgInfo>();
			if (string.IsNullOrEmpty(buildType))
			{
				buildType = Environment.GetEnvironmentVariable("_BUILDTYPE");
			}
			if (string.IsNullOrEmpty(cpuType))
			{
				cpuType = Environment.GetEnvironmentVariable("_BUILDARCH");
			}
			foreach (PkgFile allPackage in _allPackages)
			{
				if (!string.IsNullOrEmpty(cpuType) && !allPackage.IncludesCPUType(cpuType))
				{
					continue;
				}
				switch (allPackage.FMGroup)
				{
				case PackageGroups.BOOTUI:
					packageList.AddRange(GetPackagesFromList(allPackage, supportedUILanguages));
					continue;
				case PackageGroups.BOOTLOCALE:
					packageList.AddRange(GetPackagesFromList(allPackage, supportedLocales));
					continue;
				case PackageGroups.KEYBOARD:
				case PackageGroups.SPEECH:
				{
					List<string> supportedList = PkgFile.GetSupportedList(allPackage.Language);
					packageList.AddRange(GetPackagesFromList(allPackage, supportedList));
					continue;
				}
				case PackageGroups.MSFEATURE:
				case PackageGroups.OEMFEATURE:
					foreach (string featureID in (allPackage as OptionalPkgFile).FeatureIDs)
					{
						if (!allPackage.NoBasePackage)
						{
							FMPkgInfo fMPkgInfo = new FMPkgInfo(allPackage, allPackage.GroupValue);
							fMPkgInfo.GroupValue = featureID;
							packageList.Add(fMPkgInfo);
						}
						packageList.AddRange(GetSatellites(allPackage, supportedUILanguages, supportedResolutions, supportedWowCpuTypes, cpuType, featureID));
					}
					continue;
				}
				if (allPackage.GroupValues != null)
				{
					foreach (string groupValue in allPackage.GroupValues)
					{
						FMPkgInfo fMPkgInfo = new FMPkgInfo(allPackage, groupValue);
						packageList.Add(fMPkgInfo);
						packageList.AddRange(GetSatellites(allPackage, supportedUILanguages, supportedResolutions, supportedWowCpuTypes, cpuType, groupValue));
					}
				}
				else
				{
					FMPkgInfo fMPkgInfo = new FMPkgInfo(allPackage, null);
					packageList.Add(fMPkgInfo);
					packageList.AddRange(GetSatellites(allPackage, supportedUILanguages, supportedResolutions, supportedWowCpuTypes, cpuType));
				}
			}
			ProcessVariablesForList(ref packageList, buildType, cpuType, msPackageRoot);
			return packageList;
		}

		public List<string> GetUILangFeatures(List<string> packages)
		{
			List<string> result = new List<string>();
			PkgFile pkgFile = _allPackages.First((PkgFile pkg) => pkg.Language != null && pkg.Language.Equals("*"));
			if (pkgFile != null)
			{
				result = GetValuesForPackagesMatchingPattern(pkgFile.ID, packages, PkgFile.DefaultLanguagePattern).ToList();
			}
			return result;
		}

		public List<string> GetResolutionFeatures(List<string> packages)
		{
			List<string> result = new List<string>();
			PkgFile pkgFile = _allPackages.First((PkgFile pkg) => pkg.Resolution != null && pkg.Resolution.Equals("*"));
			if (pkgFile != null)
			{
				result = GetValuesForPackagesMatchingPattern(pkgFile.ID, packages, PkgFile.DefaultResolutionPattern).ToList();
			}
			return result;
		}

		private List<string> GetValuesForPackagesMatchingPattern(string pattern, List<string> packages, string satelliteStr)
		{
			List<string> list = new List<string>();
			string originalString = pattern.ToUpper(CultureInfo.InvariantCulture);
			originalString = originalString.Replace(PkgConstants.c_strPackageExtension.ToUpper(CultureInfo.InvariantCulture), "", StringComparison.OrdinalIgnoreCase);
			if (!string.IsNullOrEmpty(satelliteStr) && !originalString.EndsWith(satelliteStr, StringComparison.OrdinalIgnoreCase))
			{
				originalString += satelliteStr;
			}
			foreach (string package in packages)
			{
				if (package.StartsWith(originalString, StringComparison.OrdinalIgnoreCase))
				{
					list.Add(package.Substring(originalString.Length));
				}
			}
			return list;
		}

		public List<FMPkgInfo> GetFeatureIdentifierPackages()
		{
			List<string> supportedUILanguages = new List<string>();
			List<string> supportedLocales = new List<string>();
			List<string> supportedResolutions = new List<string>();
			string cpuType = "";
			string buildType = "";
			string msPackageRoot = "";
			return (from pkg in GetAllPackageByGroups(supportedUILanguages, supportedLocales, supportedResolutions, buildType, cpuType, msPackageRoot)
				where pkg.FeatureIdentifierPackage
				select pkg).ToList();
		}

		private void ProcessVariablesForList(ref List<FMPkgInfo> packageList, string buildType, string cpuType, string msPackageRoot)
		{
			for (int i = 0; i < packageList.Count; i++)
			{
				FMPkgInfo fMPkgInfo = packageList[i];
				string text = fMPkgInfo.PackagePath;
				if (!string.IsNullOrEmpty(buildType))
				{
					text = text.Replace("$(buildtype)", buildType, StringComparison.OrdinalIgnoreCase);
				}
				if (!string.IsNullOrEmpty(cpuType))
				{
					text = text.Replace("$(cputype)", cpuType, StringComparison.OrdinalIgnoreCase);
				}
				if (!string.IsNullOrEmpty(msPackageRoot))
				{
					text = text.Replace("$(mspackageroot)", msPackageRoot, StringComparison.OrdinalIgnoreCase);
				}
				fMPkgInfo.PackagePath = Environment.ExpandEnvironmentVariables(text);
				packageList[i] = fMPkgInfo;
			}
		}

		public List<string> GetAllPackageFileList(List<string> supportedUILanguages, List<string> supportedResolutions, List<string> supportedLocales, string buildType, string cpuType, string msPackageRoot)
		{
			return GetAllPackageFilesList(supportedUILanguages, supportedResolutions, supportedLocales, new List<CpuId>(), buildType, cpuType, msPackageRoot);
		}

		public List<string> GetAllPackageFilesList(List<string> supportedUILanguages, List<string> supportedResolutions, List<string> supportedLocales, List<CpuId> supportedWowTypes, string buildType, string cpuType, string msPackageRoot)
		{
			List<string> list = new List<string>();
			if (string.IsNullOrEmpty(buildType))
			{
				buildType = Environment.GetEnvironmentVariable("_BUILDTYPE");
				if (string.IsNullOrEmpty(buildType))
				{
					buildType = "fre";
				}
			}
			if (string.IsNullOrEmpty(cpuType))
			{
				cpuType = Environment.GetEnvironmentVariable("_BUILDARCH");
				if (string.IsNullOrEmpty(cpuType))
				{
					cpuType = CPUType_ARM;
				}
			}
			List<FMPkgInfo> allPackagesByGroups = GetAllPackagesByGroups(supportedUILanguages, supportedLocales, supportedResolutions, supportedWowTypes, buildType, cpuType, msPackageRoot);
			list.AddRange(allPackagesByGroups.Select((FMPkgInfo pkg) => pkg.PackagePath));
			return list.Distinct(IgnoreCase).ToList();
		}

		public List<string> GetPackageFileList()
		{
			List<string> list = new List<string>();
			List<FMPkgInfo> list2 = new List<FMPkgInfo>();
			list2 = GetFilteredPackagesByGroups();
			list.AddRange(list2.Select((FMPkgInfo pkg) => pkg.PackagePath));
			return list.Distinct(IgnoreCase).ToList();
		}

		public List<FMPkgInfo> GetFilteredPackagesByGroups()
		{
			List<string> list = new List<string>();
			char[] separators = new char[1] { ';' };
			if (_oemInput == null)
			{
				throw new FeatureAPIException("FeatureAPI!GetFilteredPackagesByGroups: The OEMInput reference cannot be null.  Set the OEMInput before calling this function.");
			}
			if (Features != null)
			{
				Features.ValidateConstraints(_oemInput.MSFeatureIDs, _oemInput.OEMFeatureIDs);
			}
			list.Add(_oemInput.BootLocale);
			return (from pkg in GetAllPackagesByGroups(_oemInput.SupportedLanguages.UserInterface, list, _oemInput.Resolutions, _oemInput.Edition.GetSupportedWowCpuTypes(CpuIdParser.Parse(_oemInput.CPUType)), _oemInput.BuildType, _oemInput.CPUType, _oemInput.MSPackageRoot)
				where pkg.FMGroup.Equals(PackageGroups.BASE) || (pkg.FMGroup.Equals(PackageGroups.RELEASE) && string.Equals(pkg.GroupValue, _oemInput.ReleaseType, StringComparison.OrdinalIgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.PRERELEASE) && _oemInput.ExcludePrereleaseFeatures && string.Equals(pkg.GroupValue, "replacement", StringComparison.OrdinalIgnoreCase)) || (!_oemInput.ExcludePrereleaseFeatures && string.Equals(pkg.GroupValue, "protected", StringComparison.OrdinalIgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.SV) && string.Equals(pkg.GroupValue, _oemInput.SV, StringComparison.OrdinalIgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.SOC) && string.Equals(pkg.GroupValue, _oemInput.SOC, StringComparison.OrdinalIgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.DEVICE) && string.Equals(pkg.GroupValue, _oemInput.Device, StringComparison.OrdinalIgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.DEVICELAYOUT) && string.Equals(pkg.GroupValue, _oemInput.SOC, StringComparison.OrdinalIgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.OEMDEVICEPLATFORM) && string.Equals(pkg.GroupValue, _oemInput.Device, StringComparison.OrdinalIgnoreCase)) || pkg.FMGroup.Equals(PackageGroups.BOOTLOCALE) || (pkg.FMGroup.Equals(PackageGroups.BOOTUI) && string.Equals(pkg.GroupValue, _oemInput.BootUILanguage, StringComparison.OrdinalIgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.KEYBOARD) && _oemInput.SupportedLanguages.Keyboard.Contains(pkg.GroupValue, IgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.SPEECH) && _oemInput.SupportedLanguages.Speech.Contains(pkg.GroupValue, IgnoreCase)) || (pkg.FMGroup.Equals(PackageGroups.MSFEATURE) && _oemInput.Features.Microsoft != null && _oemInput.Features.Microsoft.Intersect(pkg.GroupValue.Split(separators, StringSplitOptions.RemoveEmptyEntries), IgnoreCase).Count() > 0) || (pkg.FMGroup.Equals(PackageGroups.OEMFEATURE) && _oemInput.Features.OEM != null && _oemInput.Features.OEM.Intersect(pkg.GroupValue.Split(separators, StringSplitOptions.RemoveEmptyEntries), IgnoreCase).Count() > 0)
				select pkg).ToList();
		}

		public void Merge(FeatureManifest sourceFM)
		{
			if (sourceFM.SchemaVersion != null)
			{
				if (string.Compare(sourceFM.SchemaVersion, "1.2", StringComparison.OrdinalIgnoreCase) > 0)
				{
					throw new FeatureAPIException("FeatureAPI!Merge: The source FM has a higher SchemaVersion than supported. Cannot merge.");
				}
				if (SchemaVersion == null)
				{
					SchemaVersion = sourceFM.SchemaVersion;
				}
				else
				{
					SchemaVersion = ((string.Compare(SchemaVersion, sourceFM.SchemaVersion, StringComparison.OrdinalIgnoreCase) > 0) ? SchemaVersion : sourceFM.SchemaVersion);
				}
			}
			if (SchemaVersion != null && string.Compare(SchemaVersion, "1.2", StringComparison.OrdinalIgnoreCase) > 0)
			{
				throw new FeatureAPIException("FeatureAPI!Merge: The current FM has a higher SchemaVersion than supported. Cannot merge.");
			}
			if (OwnerType != sourceFM.OwnerType && OwnerType == OwnerType.Invalid)
			{
				OwnerType = sourceFM.OwnerType;
			}
			if (ReleaseType != sourceFM.ReleaseType && ReleaseType == ReleaseType.Invalid)
			{
				ReleaseType = sourceFM.ReleaseType;
			}
			if (string.IsNullOrEmpty(Owner) != string.IsNullOrEmpty(sourceFM.Owner))
			{
				if (string.IsNullOrEmpty(Owner))
				{
					Owner = sourceFM.Owner;
				}
			}
			else if (!string.IsNullOrEmpty(Owner) && !Owner.Equals(sourceFM.Owner, StringComparison.OrdinalIgnoreCase))
			{
				throw new FeatureAPIException("FeatureAPI!Merge: The source FM and the destination FM have different Owners '" + Owner + "' and '" + sourceFM.Owner + "'. Cannot merge.");
			}
			if (string.IsNullOrEmpty(ID) != string.IsNullOrEmpty(sourceFM.ID))
			{
				if (string.IsNullOrEmpty(ID))
				{
					Owner = sourceFM.ID;
				}
			}
			else if (!string.IsNullOrEmpty(ID) && !ID.Equals(sourceFM.ID, StringComparison.OrdinalIgnoreCase))
			{
				throw new FeatureAPIException("FeatureAPI!Merge: The source FM and the destination FM have different IDs '" + ID + "' and '" + sourceFM.ID + "'. Cannot merge.");
			}
			if (sourceFM.BootUILanguagePackageFile != null)
			{
				if (BootUILanguagePackageFile != null)
				{
					throw new FeatureAPIException("FeatureAPI!Merge: The source FM and the destination FM cannot both contain BootUILanguagePackageFile. Cannot merge.");
				}
				BootUILanguagePackageFile = sourceFM.BootUILanguagePackageFile;
			}
			if (sourceFM.BootLocalePackageFile != null)
			{
				if (BootLocalePackageFile != null)
				{
					throw new FeatureAPIException("FeatureAPI!Merge: The source FM and the destination FM cannot both contain BootUILanguagePackageFile. Cannot merge.");
				}
				BootLocalePackageFile = sourceFM.BootLocalePackageFile;
			}
			if (sourceFM.BasePackages != null)
			{
				if (BasePackages == null)
				{
					BasePackages = sourceFM.BasePackages;
				}
				else
				{
					BasePackages.AddRange(sourceFM.BasePackages);
				}
			}
			if (sourceFM.ReleasePackages != null)
			{
				if (ReleasePackages == null)
				{
					ReleasePackages = sourceFM.ReleasePackages;
				}
				else
				{
					ReleasePackages.AddRange(sourceFM.ReleasePackages);
				}
			}
			if (sourceFM != null)
			{
				if (PrereleasePackages == null)
				{
					PrereleasePackages = sourceFM.PrereleasePackages;
				}
				else
				{
					PrereleasePackages.AddRange(sourceFM.PrereleasePackages);
				}
			}
			if (sourceFM.SVPackages != null)
			{
				if (SVPackages == null)
				{
					SVPackages = sourceFM.SVPackages;
				}
				else
				{
					SVPackages.AddRange(sourceFM.SVPackages);
				}
			}
			if (sourceFM.SOCPackages != null)
			{
				if (SOCPackages == null)
				{
					SOCPackages = sourceFM.SOCPackages;
				}
				else
				{
					SOCPackages.AddRange(sourceFM.SOCPackages);
				}
			}
			if (sourceFM.DeviceLayoutPackages != null)
			{
				if (DeviceLayoutPackages == null)
				{
					DeviceLayoutPackages = sourceFM.DeviceLayoutPackages;
				}
				else
				{
					DeviceLayoutPackages.AddRange(sourceFM.DeviceLayoutPackages);
				}
			}
			if (sourceFM.DeviceSpecificPackages != null)
			{
				if (DeviceSpecificPackages == null)
				{
					DeviceSpecificPackages = sourceFM.DeviceSpecificPackages;
				}
				else
				{
					DeviceSpecificPackages.AddRange(sourceFM.DeviceSpecificPackages);
				}
			}
			if (sourceFM.OEMDevicePlatformPackages != null)
			{
				if (OEMDevicePlatformPackages == null)
				{
					OEMDevicePlatformPackages = sourceFM.OEMDevicePlatformPackages;
				}
				else
				{
					OEMDevicePlatformPackages.AddRange(sourceFM.OEMDevicePlatformPackages);
				}
			}
			if (sourceFM.Features != null)
			{
				if (Features == null)
				{
					Features = sourceFM.Features;
				}
				else
				{
					Features.Merge(sourceFM.Features);
				}
			}
			if (sourceFM.SpeechPackages != null)
			{
				if (SpeechPackages == null)
				{
					SpeechPackages = sourceFM.SpeechPackages;
				}
				else
				{
					SpeechPackages.AddRange(sourceFM.SpeechPackages);
				}
			}
			if (sourceFM.KeyboardPackages != null)
			{
				if (KeyboardPackages == null)
				{
					KeyboardPackages = sourceFM.KeyboardPackages;
				}
				else
				{
					KeyboardPackages.AddRange(sourceFM.KeyboardPackages);
				}
			}
		}

		public void WriteToFile(string fileName)
		{
			SchemaVersion = "1.2";
			Revision = "1";
			string directoryName = Path.GetDirectoryName(fileName);
			if (!LongPathDirectory.Exists(directoryName))
			{
				LongPathDirectory.CreateDirectory(directoryName);
			}
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(fileName));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(FeatureManifest));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new FeatureAPIException("FeatureAPI!WriteToFile: Unable to write Feature Manifest XML file '" + fileName + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}

		public string GetOEMDevicePlatformPackage(string device)
		{
			string text = null;
			if (OEMDevicePlatformPackages != null)
			{
				foreach (OEMDevicePkgFile oEMDevicePlatformPackage in OEMDevicePlatformPackages)
				{
					if (oEMDevicePlatformPackage.IsValidGroupValue(device))
					{
						text = oEMDevicePlatformPackage.PackagePath;
						break;
					}
				}
			}
			if (string.IsNullOrEmpty(text) && DeviceSpecificPackages != null)
			{
				DevicePkgFile devicePkgFile = DeviceSpecificPackages.Find((DevicePkgFile pkg) => pkg.FeatureIdentifierPackage && pkg.IsValidGroupValue(device));
				if (devicePkgFile != null)
				{
					text = devicePkgFile.PackagePath;
				}
			}
			return text;
		}

		public string GetDeviceLayoutPackage(string SOC)
		{
			string text = null;
			if (DeviceLayoutPackages != null)
			{
				foreach (DeviceLayoutPkgFile deviceLayoutPackage in DeviceLayoutPackages)
				{
					if (deviceLayoutPackage.IsValidGroupValue(SOC))
					{
						text = deviceLayoutPackage.PackagePath;
						break;
					}
				}
			}
			if (string.IsNullOrEmpty(text) && SOCPackages != null)
			{
				SOCPkgFile sOCPkgFile = SOCPackages.Find((SOCPkgFile pkg) => pkg.FeatureIdentifierPackage && pkg.IsValidGroupValue(SOC));
				if (sOCPkgFile != null)
				{
					text = sOCPkgFile.PackagePath;
				}
			}
			return text;
		}

		public void ProcessVariables()
		{
			foreach (PkgFile allPackage in _allPackages)
			{
				allPackage.ProcessVariables();
			}
		}

		public static void ValidateAndLoad(ref FeatureManifest fm, string xmlFile, IULogger logger)
		{
			IULogger iULogger = new IULogger();
			iULogger.ErrorLogger = null;
			iULogger.InformationLogger = null;
			if (!LongPathFile.Exists(xmlFile))
			{
				throw new FeatureAPIException("FeatureAPI!ValidateAndLoad: FeatureManifest file was not found: " + xmlFile);
			}
			string text = string.Empty;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			string featureManifestSchema = DevicePaths.FeatureManifestSchema;
			string[] array = manifestResourceNames;
			foreach (string text2 in array)
			{
				if (text2.Contains(featureManifestSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new FeatureAPIException("FeatureAPI!ValidateAndLoad: XSD resource was not found: " + featureManifestSchema);
			}
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(FeatureManifest));
			try
			{
				fm = (FeatureManifest)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException)
			{
				throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to parse Feature Manifest XML file '" + xmlFile + "'.", innerException);
			}
			finally
			{
				textReader.Close();
			}
			bool flag = "1.2".Equals(fm.SchemaVersion);
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				XsdValidator xsdValidator = new XsdValidator();
				try
				{
					xsdValidator.ValidateXsd(xsdStream, xmlFile, iULogger);
				}
				catch (XsdValidatorException innerException2)
				{
					if (flag)
					{
						throw new FeatureAPIException("FeatureAPI!ValidateInput: Unable to validate Feature Manifest XSD for file '" + xmlFile + "'.", innerException2);
					}
					logger.LogWarning("Warning: FeatureAPI!ValidateInput: Unable to validate Feature Manifest XSD for file '" + xmlFile + "'.");
					if (string.IsNullOrEmpty(fm.SchemaVersion))
					{
						logger.LogWarning("Warning: Schema Version was not given in FM. Most up to date Schema Version is {1}.", "1.2");
					}
					else
					{
						logger.LogWarning("Warning: Schema Version given in FM ({0}) does not match most up to date Schema Version ({1}).", fm.SchemaVersion, "1.2");
					}
				}
			}
			logger.LogInfo("FeatureAPI: Successfully validated the Feature Manifest XML: {0}", xmlFile);
			if (fm.CPUPackages != null)
			{
				if (fm.BasePackages != null)
				{
					fm.BasePackages.AddRange(fm.CPUPackages);
				}
				else
				{
					FeatureManifest obj = fm;
					obj.BasePackages = obj.CPUPackages;
				}
				fm.CPUPackages = new List<PkgFile>();
			}
			fm.SourceFile = Path.GetFileName(xmlFile).ToUpper(CultureInfo.InvariantCulture);
		}

		public void AddPkgFile(PkgFile pkgEntry)
		{
			switch (pkgEntry.FMGroup)
			{
			case PackageGroups.BASE:
				if (pkgEntry != null)
				{
					PkgFile item = new PkgFile(pkgEntry);
					if (BasePackages == null)
					{
						BasePackages = new List<PkgFile>();
					}
					BasePackages.Add(item);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'PkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.DEVICE:
				if (pkgEntry is DevicePkgFile)
				{
					DevicePkgFile devicePkgFile = new DevicePkgFile();
					devicePkgFile.CopyPkgFile(pkgEntry);
					if (DeviceSpecificPackages == null)
					{
						DeviceSpecificPackages = new List<DevicePkgFile>();
					}
					DeviceSpecificPackages.Add(devicePkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'DevicePkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.DEVICELAYOUT:
				if (pkgEntry is DeviceLayoutPkgFile)
				{
					DeviceLayoutPkgFile deviceLayoutPkgFile = new DeviceLayoutPkgFile();
					deviceLayoutPkgFile.CopyPkgFile(pkgEntry);
					if (DeviceLayoutPackages == null)
					{
						DeviceLayoutPackages = new List<DeviceLayoutPkgFile>();
					}
					DeviceLayoutPackages.Add(deviceLayoutPkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'DeviceLayoutPkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.MSFEATURE:
			case PackageGroups.OEMFEATURE:
				if (Features == null)
				{
					Features = new FMFeatures();
				}
				if (pkgEntry.FMGroup == PackageGroups.MSFEATURE)
				{
					if (Features.Microsoft == null)
					{
						Features.Microsoft = new List<MSOptionalPkgFile>();
					}
					if (!(pkgEntry is MSOptionalPkgFile))
					{
						throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'MSOptionalPkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
					}
					MSOptionalPkgFile mSOptionalPkgFile = new MSOptionalPkgFile();
					mSOptionalPkgFile.CopyPkgFile(pkgEntry);
					Features.Microsoft.Add(mSOptionalPkgFile);
				}
				else
				{
					if (Features.OEM == null)
					{
						Features.OEM = new List<OEMOptionalPkgFile>();
					}
					if (!(pkgEntry is OEMOptionalPkgFile))
					{
						throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'OEMOptionalPkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
					}
					OEMOptionalPkgFile oEMOptionalPkgFile = new OEMOptionalPkgFile();
					oEMOptionalPkgFile.CopyPkgFile(pkgEntry);
					Features.OEM.Add(oEMOptionalPkgFile);
				}
				break;
			case PackageGroups.OEMDEVICEPLATFORM:
				if (pkgEntry is OEMDevicePkgFile)
				{
					OEMDevicePkgFile oEMDevicePkgFile = new OEMDevicePkgFile();
					oEMDevicePkgFile.CopyPkgFile(pkgEntry);
					if (OEMDevicePlatformPackages == null)
					{
						OEMDevicePlatformPackages = new List<OEMDevicePkgFile>();
					}
					OEMDevicePlatformPackages.Add(oEMDevicePkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'OEMDevicePkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.PRERELEASE:
				if (pkgEntry is PrereleasePkgFile)
				{
					PrereleasePkgFile prereleasePkgFile = new PrereleasePkgFile();
					prereleasePkgFile.CopyPkgFile(pkgEntry);
					if (PrereleasePackages == null)
					{
						PrereleasePackages = new List<PrereleasePkgFile>();
					}
					PrereleasePackages.Add(prereleasePkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'PrereleasePkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.RELEASE:
				if (pkgEntry is ReleasePkgFile)
				{
					ReleasePkgFile releasePkgFile = new ReleasePkgFile();
					releasePkgFile.CopyPkgFile(pkgEntry);
					if (ReleasePackages == null)
					{
						ReleasePackages = new List<ReleasePkgFile>();
					}
					ReleasePackages.Add(releasePkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'ReleasePkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.SOC:
				if (pkgEntry is SOCPkgFile)
				{
					SOCPkgFile sOCPkgFile = new SOCPkgFile();
					sOCPkgFile.CopyPkgFile(pkgEntry);
					if (SOCPackages == null)
					{
						SOCPackages = new List<SOCPkgFile>();
					}
					SOCPackages.Add(sOCPkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'SOCPkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.SV:
				if (pkgEntry is SVPkgFile)
				{
					SVPkgFile sVPkgFile = new SVPkgFile();
					sVPkgFile.CopyPkgFile(pkgEntry);
					if (SVPackages == null)
					{
						SVPackages = new List<SVPkgFile>();
					}
					SVPackages.Add(sVPkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'SVPkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.KEYBOARD:
				if (pkgEntry is KeyboardPkgFile)
				{
					KeyboardPkgFile keyboardPkgFile = new KeyboardPkgFile();
					keyboardPkgFile.CopyPkgFile(pkgEntry);
					if (KeyboardPackages == null)
					{
						KeyboardPackages = new List<KeyboardPkgFile>();
					}
					KeyboardPackages.Add(keyboardPkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'KeyboardPkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			case PackageGroups.SPEECH:
				if (pkgEntry is SpeechPkgFile)
				{
					SpeechPkgFile speechPkgFile = new SpeechPkgFile();
					speechPkgFile.CopyPkgFile(pkgEntry);
					if (SpeechPackages == null)
					{
						SpeechPackages = new List<SpeechPkgFile>();
					}
					SpeechPackages.Add(speechPkgFile);
					break;
				}
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Expected 'SpeechPkgFile' package type in FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'"));
			default:
				throw new FeatureAPIException(string.Concat("FeatureAPI!AddPkgFile: Unsupported FMGroup '", pkgEntry.FMGroup, "' for package '", pkgEntry.ID, "'.  Update the code to handle this new type."));
			}
		}

		public void AddPackagesFromMergeResult(List<FMPkgInfo> packageList, List<MergeResult> results, List<string> supportedLanguages, List<string> supportedResolutions, string packageOutputDir, string packageOutputDirReplacement)
		{
			if (packageList == null || packageList.Count() == 0)
			{
				return;
			}
			PackageGroups fMGroup = packageList[0].FMGroup;
			foreach (MergeResult result in results)
			{
				if ((fMGroup == PackageGroups.MSFEATURE || fMGroup == PackageGroups.OEMFEATURE) && Features == null)
				{
					Features = new FMFeatures();
				}
				PkgFile pkgFile = new PkgFile();
				bool flag = false;
				switch (fMGroup)
				{
				case PackageGroups.BASE:
					if (BasePackages == null)
					{
						BasePackages = new List<PkgFile>();
					}
					BasePackages.Add(pkgFile);
					break;
				case PackageGroups.RELEASE:
				{
					ReleasePkgFile releasePkgFile = new ReleasePkgFile();
					if (ReleasePackages == null)
					{
						ReleasePackages = new List<ReleasePkgFile>();
					}
					ReleasePackages.Add(releasePkgFile);
					pkgFile = releasePkgFile;
					break;
				}
				case PackageGroups.DEVICELAYOUT:
				{
					DeviceLayoutPkgFile deviceLayoutPkgFile = new DeviceLayoutPkgFile();
					if (DeviceLayoutPackages == null)
					{
						DeviceLayoutPackages = new List<DeviceLayoutPkgFile>();
					}
					deviceLayoutPkgFile.CPUType = result.PkgInfo.CpuType.ToString();
					DeviceLayoutPackages.Add(deviceLayoutPkgFile);
					pkgFile = deviceLayoutPkgFile;
					flag = true;
					break;
				}
				case PackageGroups.OEMDEVICEPLATFORM:
				{
					OEMDevicePkgFile oEMDevicePkgFile = new OEMDevicePkgFile();
					if (OEMDevicePlatformPackages == null)
					{
						OEMDevicePlatformPackages = new List<OEMDevicePkgFile>();
					}
					OEMDevicePlatformPackages.Add(oEMDevicePkgFile);
					pkgFile = oEMDevicePkgFile;
					break;
				}
				case PackageGroups.SV:
				{
					SVPkgFile sVPkgFile = new SVPkgFile();
					if (SVPackages == null)
					{
						SVPackages = new List<SVPkgFile>();
					}
					SVPackages.Add(sVPkgFile);
					pkgFile = sVPkgFile;
					break;
				}
				case PackageGroups.SOC:
				{
					SOCPkgFile sOCPkgFile = new SOCPkgFile();
					if (SOCPackages == null)
					{
						SOCPackages = new List<SOCPkgFile>();
					}
					sOCPkgFile.CPUType = result.PkgInfo.CpuType.ToString();
					SOCPackages.Add(sOCPkgFile);
					pkgFile = sOCPkgFile;
					break;
				}
				case PackageGroups.DEVICE:
				{
					DevicePkgFile devicePkgFile = new DevicePkgFile();
					if (DeviceSpecificPackages == null)
					{
						DeviceSpecificPackages = new List<DevicePkgFile>();
					}
					DeviceSpecificPackages.Add(devicePkgFile);
					pkgFile = devicePkgFile;
					break;
				}
				case PackageGroups.MSFEATURE:
				{
					MSOptionalPkgFile mSOptionalPkgFile = new MSOptionalPkgFile();
					if (Features.Microsoft == null)
					{
						Features.Microsoft = new List<MSOptionalPkgFile>();
					}
					Features.Microsoft.Add(mSOptionalPkgFile);
					pkgFile = mSOptionalPkgFile;
					break;
				}
				case PackageGroups.OEMFEATURE:
				{
					OEMOptionalPkgFile oEMOptionalPkgFile = new OEMOptionalPkgFile();
					if (Features.OEM == null)
					{
						Features.OEM = new List<OEMOptionalPkgFile>();
					}
					Features.OEM.Add(oEMOptionalPkgFile);
					pkgFile = oEMOptionalPkgFile;
					break;
				}
				case PackageGroups.KEYBOARD:
					if (KeyboardPackages == null)
					{
						KeyboardPackages = new List<KeyboardPkgFile>();
					}
					KeyboardPackages.Add(pkgFile as KeyboardPkgFile);
					break;
				case PackageGroups.SPEECH:
					if (SpeechPackages == null)
					{
						SpeechPackages = new List<SpeechPkgFile>();
					}
					SpeechPackages.Add(pkgFile as SpeechPkgFile);
					break;
				case PackageGroups.PRERELEASE:
				{
					PrereleasePkgFile prereleasePkgFile = new PrereleasePkgFile();
					if (PrereleasePackages == null)
					{
						PrereleasePackages = new List<PrereleasePkgFile>();
					}
					PrereleasePackages.Add(prereleasePkgFile);
					pkgFile = prereleasePkgFile;
					break;
				}
				}
				string groupValue = packageList[0].GroupValue;
				pkgFile.InitializeWithMergeResult(result, fMGroup, groupValue, supportedLanguages, supportedResolutions);
				if (flag)
				{
					pkgFile.FeatureIdentifierPackage = false;
				}
				if (!string.IsNullOrEmpty(packageOutputDirReplacement))
				{
					pkgFile.Directory = pkgFile.Directory.Replace(packageOutputDir, packageOutputDirReplacement, StringComparison.OrdinalIgnoreCase);
				}
				if (!pkgFile.Directory.Contains(packageOutputDirReplacement))
				{
					FMPkgInfo fMPkgInfo = packageList.Single((FMPkgInfo pkg) => pkg.PackagePath.StartsWith(Path.ChangeExtension(pkgFile.PackagePath, ""), StringComparison.OrdinalIgnoreCase));
					if (fMPkgInfo != null)
					{
						pkgFile.Directory = LongPath.GetDirectoryName(fMPkgInfo.RawBasePath);
					}
				}
			}
		}

		public static string GetFeatureIDWithFMID(string featureID, string fmID)
		{
			if (string.IsNullOrEmpty(fmID))
			{
				return featureID;
			}
			return featureID + "." + fmID;
		}

		public override string ToString()
		{
			return SourceFile;
		}
	}
}
