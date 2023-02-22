using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.WindowsPhone.CompDB;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.ImageUpdate.FeatureMerger
{
	public class FeatureMerger
	{
		private enum CriticalFMProcessing
		{
			Yes,
			No,
			All
		}

		private static CommandLineParser _commandLineParser = null;

		private static bool _singleFM = false;

		private static FMCollectionManifest _fmFileList = null;

		private static Dictionary<string, string> _variableLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private static List<string> _generatedSPKGs = new List<string>();

		private static string _outputPackageDir;

		private static string _outputFMDir;

		private static string _inputFMDir;

		private static string _packagePathReplacement = string.Empty;

		private static string _msPackageRoot = string.Empty;

		private static string _binaryRoot = string.Empty;

		private static CpuId _cpuId = CpuId.Invalid;

		private static string _buildTypeStr = null;

		private static BuildType _buildType = BuildType.Invalid;

		private static VersionInfo _version = VersionInfo.Empty;

		private static ReleaseType _releaseType = ReleaseType.Test;

		private static CriticalFMProcessing _critical = CriticalFMProcessing.All;

		private static bool _incremental = false;

		private static bool _compress = false;

		private static bool _convertToCBS = false;

		private static StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

		private static readonly string MicrosoftPhoneFMName = "MicrosoftPhoneFM";

		private static IULogger _iuLogger = new IULogger();

		private static bool _doPlatformManifest = false;

		private static PlatformManifestGen _pmMainOS = null;

		private static PlatformManifestGen _pmUpdateOS = null;

		private static PlatformManifestGen _pmEFIESP = null;

		private static readonly string PlatformManifestRelativeDir = "DeviceImaging\\PlatformManifest\\";

		private static string _pmPath;

		private static string _signInfoPath;

		private static string _buildBranchInfo;

		private static FMCollectionItem _uiItem = null;

		private static FMCollectionItem _msPhoneItem = null;

		private static Guid _buildID = Guid.NewGuid();

		private static string _buildInfo;

		private static List<string> _supportedLocales = new List<string>();

		private static void Main(string[] args)
		{
			SetCmdLineParams();
			if (!_commandLineParser.ParseString(Environment.CommandLine, true))
			{
				DisplayUsage();
				Environment.ExitCode = 1;
				return;
			}
			string parameterAsString = _commandLineParser.GetParameterAsString("InputFile");
			string parameterAsString2 = _commandLineParser.GetParameterAsString("OutputPackageDir");
			string parameterAsString3 = _commandLineParser.GetParameterAsString("OutputFMDir");
			string parameterAsString4 = _commandLineParser.GetParameterAsString("OutputPackageVersion");
			string switchAsString = _commandLineParser.GetSwitchAsString("InputFMDir");
			string switchAsString2 = _commandLineParser.GetSwitchAsString("MergePackageRootReplacement");
			string switchAsString3 = _commandLineParser.GetSwitchAsString("variables");
			string switchAsString4 = _commandLineParser.GetSwitchAsString("FMID");
			string switchAsString5 = _commandLineParser.GetSwitchAsString("OwnerType");
			string switchAsString6 = _commandLineParser.GetSwitchAsString("OwnerName");
			string switchAsString7 = _commandLineParser.GetSwitchAsString("Languages");
			string switchAsString8 = _commandLineParser.GetSwitchAsString("Resolutions");
			string text = _commandLineParser.GetSwitchAsString("Critical");
			if (string.IsNullOrEmpty(text))
			{
				text = CriticalFMProcessing.All.ToString();
			}
			bool switchAsBoolean = _commandLineParser.GetSwitchAsBoolean("Incremental");
			bool switchAsBoolean2 = _commandLineParser.GetSwitchAsBoolean("Compress");
			bool switchAsBoolean3 = _commandLineParser.GetSwitchAsBoolean("ConvertToCBS");
			_supportedLocales.Add("en-us");
			try
			{
				if (!MergeFeatures(parameterAsString, parameterAsString2, parameterAsString3, parameterAsString4, switchAsString, text, switchAsString2, switchAsString3, switchAsString4, switchAsString5, switchAsString6, switchAsString7, switchAsString8, switchAsBoolean2, switchAsBoolean3, switchAsBoolean))
				{
					Environment.ExitCode = 1;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("{0}", ex.Message);
				if (ex.InnerException != null)
				{
					Console.WriteLine("\t{0}", ex.InnerException.ToString());
				}
				Console.WriteLine("An unhandled exception was thrown: {0}", ex.ToString());
				Environment.ExitCode = 3;
			}
		}

		public static bool MergeFeatures(string inputFile, string outputPackageDir, string outputFMDir, string versionStr, string inputFMDir, string critical, string packagePathReplacement, string variablesStr, string fmID, string ownerTypeStr, string ownerName, string languages, string resolutions, bool compress, bool convertToCBS, bool incremental)
		{
			_outputPackageDir = outputPackageDir;
			_outputFMDir = outputFMDir;
			_inputFMDir = inputFMDir;
			_packagePathReplacement = packagePathReplacement;
			_incremental = incremental;
			_compress = compress;
			_convertToCBS = convertToCBS;
			if (string.IsNullOrEmpty(inputFile))
			{
				LogUtil.Error("Input file cannot be empty.");
				DisplayUsage();
				Environment.ExitCode = 1;
				return false;
			}
			if (!File.Exists(inputFile))
			{
				LogUtil.Error("Input file must be an existing FMFileList or Feature Manifest.");
				DisplayUsage();
				Environment.ExitCode = 1;
				return false;
			}
			if (string.IsNullOrEmpty(versionStr))
			{
				LogUtil.Error("Non-empty output version string is required");
				DisplayUsage();
				Environment.ExitCode = 1;
				return false;
			}
			if (!VersionInfo.TryParse(versionStr, out _version))
			{
				LogUtil.Error("Invalid output version string '{0}'", versionStr);
				DisplayUsage();
				Environment.ExitCode = 1;
				return false;
			}
			if (!LoadFMFiles(inputFile))
			{
				LogUtil.Error("Invalid input file '{0}'", inputFile);
				Environment.ExitCode = 1;
				return false;
			}
			if (!ParseVariables(variablesStr))
			{
				DisplayUsage();
				Environment.ExitCode = 1;
				return false;
			}
			if (_singleFM)
			{
				if (!PrepSingleFM(inputFile, fmID, ownerTypeStr, ownerName, languages, resolutions))
				{
					DisplayUsage();
					Environment.ExitCode = 1;
					return false;
				}
			}
			else if (!string.IsNullOrEmpty(critical))
			{
				bool ignoreCase = true;
				if (!Enum.TryParse<CriticalFMProcessing>(critical, ignoreCase, out _critical))
				{
					LogUtil.Error("Critical value is not recognized.  Must one of the following:\n\t{0}\n\t{1}\n\t{2}", CriticalFMProcessing.Yes.ToString(), CriticalFMProcessing.No.ToString(), CriticalFMProcessing.All.ToString());
					return false;
				}
			}
			if (!Directory.Exists(_outputPackageDir))
			{
				Directory.CreateDirectory(_outputPackageDir);
			}
			if (!Directory.Exists(_outputFMDir))
			{
				Directory.CreateDirectory(_outputFMDir);
			}
			if (_doPlatformManifest)
			{
				CheckForUserInstallableFM();
			}
			List<FMCollectionItem> allRelavantFMs = GetAllRelavantFMs(_fmFileList);
			if (!CheckPackages(allRelavantFMs))
			{
				return false;
			}
			if (allRelavantFMs.Any((FMCollectionItem item) => item.ownerType == OwnerType.Microsoft))
			{
				_buildInfo = Environment.ExpandEnvironmentVariables("%_RELEASELABEL%.%_PARENTBRANCHBUILDNUMBER%.%_QFELEVEL%.%_BUILDTIME%");
			}
			foreach (FMCollectionItem item in allRelavantFMs)
			{
				if (!MergeOneFM(item))
				{
					return false;
				}
			}
			return true;
		}

		private static bool CheckPackages(List<FMCollectionItem> fmItems)
		{
			bool flag = false;
			IULogger iULogger = new IULogger();
			iULogger.DebugLogger = null;
			iULogger.ErrorLogger = null;
			iULogger.InformationLogger = null;
			iULogger.WarningLogger = null;
			foreach (FMCollectionItem fmItem in fmItems)
			{
				FeatureManifest fm = new FeatureManifest();
				try
				{
					string text = fmItem.Path;
					if (!string.IsNullOrEmpty(_inputFMDir))
					{
						text = text.ToUpper(CultureInfo.InvariantCulture).Replace("$(FMDIRECTORY)", _inputFMDir);
					}
					FeatureManifest.ValidateAndLoad(ref fm, text, iULogger);
				}
				catch (Exception ex)
				{
					LogUtil.Error("Error: {0}", ex);
					LogUtil.Error("Error: failed to load Feature Manifest: {0}", fmItem.Path);
					flag = true;
					continue;
				}
				foreach (FeatureManifest.FMPkgInfo allPackagesByGroup in fm.GetAllPackagesByGroups(_fmFileList.SupportedLanguages, _supportedLocales, _fmFileList.SupportedResolutions, _fmFileList.GetWowGuestCpuTypes(_cpuId), _buildTypeStr, _cpuId.ToString(), _msPackageRoot))
				{
					if (!File.Exists(allPackagesByGroup.PackagePath) && !File.Exists(Path.ChangeExtension(allPackagesByGroup.PackagePath, PkgConstants.c_strPackageExtension)))
					{
						LogUtil.Error("Error: Missing package: {0}", allPackagesByGroup.PackagePath);
						flag = true;
					}
				}
			}
			return !flag;
		}

		private static List<FMCollectionItem> GetAllRelavantFMs(FMCollectionManifest manifest)
		{
			List<FMCollectionItem> list = new List<FMCollectionItem>();
			foreach (FMCollectionItem fM in manifest.FMs)
			{
				if (!fM.UserInstallable && (_singleFM || fM.CPUType == CpuId.Invalid || fM.CPUType == _cpuId) && (_critical == CriticalFMProcessing.All || ((_critical != 0 || fM.Critical) && (_critical != CriticalFMProcessing.No || !fM.Critical))))
				{
					list.Add(fM);
				}
			}
			return list;
		}

		private static bool PrepSingleFM(string inputFile, string fmID, string ownerTypeStr, string ownerName, string languages, string resolutions)
		{
			_fmFileList = new FMCollectionManifest();
			_fmFileList.FMs = new List<FMCollectionItem>();
			if (string.IsNullOrEmpty(languages))
			{
				LogUtil.Error("A list of languages is required when specifying a single FM.");
				return false;
			}
			_fmFileList.SupportedLanguages = languages.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (_fmFileList.SupportedLanguages.Count() == 0)
			{
				LogUtil.Error("The list of languages cannot be empty when specifying a single FM.");
				return false;
			}
			if (string.IsNullOrEmpty(resolutions))
			{
				LogUtil.Error("A list of resolutions is required when specifying a single FM.");
			}
			_fmFileList.SupportedResolutions = resolutions.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
			if (_fmFileList.SupportedResolutions.Count() == 0)
			{
				LogUtil.Error("The list of resolutions cannot be empty when specifying a single FM.");
				return false;
			}
			FMCollectionItem fMCollectionItem = new FMCollectionItem();
			fMCollectionItem.ID = fmID;
			fMCollectionItem.ownerType = OwnerType.OEM;
			if (!string.IsNullOrEmpty(ownerTypeStr))
			{
				bool ignoreCase = true;
				if (!Enum.TryParse<OwnerType>(ownerTypeStr, ignoreCase, out fMCollectionItem.ownerType))
				{
					LogUtil.Error("OwnerType is not recognized.  Must one of the following:\n\t{0}\n\t{1}\n\t{2}\n\t{3}", OwnerType.OEM.ToString(), OwnerType.Microsoft.ToString(), OwnerType.MobileOperator.ToString(), OwnerType.SiliconVendor.ToString());
					return false;
				}
			}
			if (fMCollectionItem.ownerType != OwnerType.Microsoft)
			{
				if (string.IsNullOrEmpty(ownerName))
				{
					LogUtil.Error("OwnerName is required when specifying a single FM.");
					return false;
				}
				fMCollectionItem.Owner = ownerName;
			}
			else
			{
				fMCollectionItem.Owner = OwnerType.Microsoft.ToString();
			}
			fMCollectionItem.ValidateAsMicrosoftPhoneFM = Path.GetFileName(inputFile).Equals(MicrosoftPhoneFMName, StringComparison.OrdinalIgnoreCase);
			if ((fMCollectionItem.ownerType != OwnerType.Microsoft || !fMCollectionItem.ValidateAsMicrosoftPhoneFM) && string.IsNullOrEmpty(fMCollectionItem.ID))
			{
				LogUtil.Error("FMID must be specified for Single FM InputFile.");
				return false;
			}
			fMCollectionItem.Path = inputFile;
			fMCollectionItem.CPUType = _cpuId;
			fMCollectionItem.releaseType = _releaseType;
			_fmFileList.FMs.Add(fMCollectionItem);
			return true;
		}

		private static bool MergeOneFM(FMCollectionItem fmItem)
		{
			FeatureManifest fm = new FeatureManifest();
			bool flag = _doPlatformManifest && (fmItem.ownerType == OwnerType.Microsoft || !string.IsNullOrEmpty(_binaryRoot));
			bool flag2 = false;
			if (flag)
			{
				_pmMainOS = null;
				_pmUpdateOS = null;
				_pmEFIESP = null;
				flag2 = _uiItem != null && _msPhoneItem != null && _msPhoneItem == fmItem;
			}
			string text = fmItem.Path;
			if (!string.IsNullOrEmpty(_inputFMDir))
			{
				text = text.ToUpper(CultureInfo.InvariantCulture).Replace("$(FMDIRECTORY)", _inputFMDir);
			}
			string text2 = Path.Combine(_outputFMDir, Path.GetFileName(fmItem.Path));
			bool incremental = _incremental;
			if (_incremental)
			{
				if (!FileUtils.IsTargetUpToDate(text, text2))
				{
					incremental = false;
				}
				if (flag2 && !FileUtils.IsTargetUpToDate(_uiItem.Path.ToUpper(CultureInfo.InvariantCulture).Replace("$(FMDIRECTORY)", _inputFMDir), text2))
				{
					incremental = false;
				}
			}
			if (!fmItem.ValidateAsMicrosoftPhoneFM)
			{
				if (!_singleFM && string.IsNullOrEmpty(fmItem.ID))
				{
					LogUtil.Error("FMID must be specified for entry '{0}' in the FMFileList InputFile.", fmItem.Path);
					return false;
				}
				if (fmItem.ID.Contains('.'))
				{
					LogUtil.Error("FMID '{0} contains the invalid character '.'", fmItem.ID);
					return false;
				}
			}
			FeatureManifest.ValidateAndLoad(ref fm, text, _iuLogger);
			FeatureManifest newFM = new FeatureManifest();
			newFM.SourceFile = fm.SourceFile;
			newFM.OwnerType = fmItem.ownerType;
			newFM.Owner = fmItem.Owner;
			newFM.ReleaseType = fmItem.releaseType;
			newFM.ID = fmItem.ID;
			newFM.OSVersion = _version.ToString();
			if (fm.Features != null)
			{
				newFM.Features = new FMFeatures();
				if (fm.Features.Microsoft != null)
				{
					newFM.Features.Microsoft = new List<MSOptionalPkgFile>();
					newFM.Features.MSFeatureGroups = fm.Features.MSFeatureGroups;
					if (fm.Features.MSConditionalFeatures != null)
					{
						newFM.Features.MSConditionalFeatures = fm.Features.MSConditionalFeatures;
						foreach (FMConditionalFeature mSConditionalFeature in newFM.Features.MSConditionalFeatures)
						{
							mSConditionalFeature.FMID = fmItem.ID;
						}
					}
				}
				if (fm.Features.OEM != null)
				{
					newFM.Features.OEM = new List<OEMOptionalPkgFile>();
					newFM.Features.OEMFeatureGroups = fm.Features.OEMFeatureGroups;
				}
			}
			List<FeatureManifest.FMPkgInfo> pkgs = fm.GetAllPackagesByGroups(_fmFileList.SupportedLanguages, _supportedLocales, _fmFileList.SupportedResolutions, _fmFileList.GetWowGuestCpuTypes(_cpuId), _buildTypeStr, _cpuId.ToString(), _msPackageRoot);
			List<FeatureManifest.FMPkgInfo> list = pkgs.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.OEMDEVICEPLATFORM || pkg.FMGroup == FeatureManifest.PackageGroups.DEVICE).ToList();
			pkgs = pkgs.Except(list).ToList();
			foreach (FeatureManifest.FMPkgInfo item in list)
			{
				FeatureManifest.FMPkgInfo current;
				if ((current = item).FMGroup == FeatureManifest.PackageGroups.OEMDEVICEPLATFORM)
				{
					current.FMGroup = FeatureManifest.PackageGroups.DEVICE;
				}
				pkgs.Add(current);
			}
			if (fm.KeyboardPackages.Count() > 0)
			{
				if (newFM.KeyboardPackages == null)
				{
					newFM.KeyboardPackages = fm.KeyboardPackages;
				}
				else
				{
					newFM.KeyboardPackages.AddRange(fm.KeyboardPackages);
				}
			}
			if (fm.SpeechPackages.Count() > 0)
			{
				if (newFM.SpeechPackages == null)
				{
					newFM.SpeechPackages = fm.SpeechPackages;
				}
				else
				{
					newFM.SpeechPackages.AddRange(fm.SpeechPackages);
				}
			}
			List<string> source = (from pkg in pkgs
				where pkg.FMGroup != FeatureManifest.PackageGroups.BOOTUI && pkg.FMGroup != FeatureManifest.PackageGroups.BOOTLOCALE && pkg.FMGroup != FeatureManifest.PackageGroups.SPEECH && pkg.FMGroup != FeatureManifest.PackageGroups.KEYBOARD
				select pkg.FeatureID).Distinct(IgnoreCase).ToList();
			if (fm.BootLocalePackageFile != null)
			{
				newFM.BootLocalePackageFile = fm.BootLocalePackageFile;
			}
			if (fm.BootUILanguagePackageFile != null)
			{
				newFM.BootUILanguagePackageFile = fm.BootUILanguagePackageFile;
			}
			List<string> list2 = source.Where((string feature) => feature.Contains('.') || !Regex.Match(feature, PkgConstants.c_strPackageStringPattern).Success).ToList();
			if (list2.Count() > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("FeatureIDs for Merging only support digits/letters and underscore.");
				stringBuilder.AppendLine("Invalid characters found in the following FeatureIDs for FM " + fmItem.Path);
				foreach (string item2 in list2)
				{
					stringBuilder.AppendLine("\t" + item2);
				}
				LogUtil.Error(stringBuilder.ToString());
				return false;
			}
			List<FeatureManifest.FMPkgInfo> source2 = pkgs.Where((FeatureManifest.FMPkgInfo pkg) => Path.GetExtension(pkg.PackagePath).Equals(PkgConstants.c_strCBSPackageExtension, StringComparison.OrdinalIgnoreCase)).ToList();
			if (_convertToCBS)
			{
				List<FeatureManifest.FMPkgInfo> list3 = source2.Where((FeatureManifest.FMPkgInfo pkg) => !File.Exists(Path.ChangeExtension(pkg.PackagePath, PkgConstants.c_strPackageExtension))).ToList();
				pkgs = pkgs.Except(list3).ToList();
				AddCBSOnlyPackagesToFM(list3, fm, ref newFM);
			}
			else if (source2.Any())
			{
				LogUtil.Error("The following packages have an unsupported extension of '{0}': {1}", PkgConstants.c_strCBSPackageExtension, string.Join(" ", source2.Select((FeatureManifest.FMPkgInfo pkg) => pkg.PackagePath)));
				return false;
			}
			IPkgInfo templatePkg = null;
			Parallel.ForEach(source, delegate(string feature)
			{
				List<FeatureManifest.FMPkgInfo> list5 = pkgs.Where((FeatureManifest.FMPkgInfo pkg) => feature.Equals(pkg.FeatureID, StringComparison.OrdinalIgnoreCase)).ToList();
				string[] inputPkgs = ((!_convertToCBS) ? list5.Select((FeatureManifest.FMPkgInfo pkg) => ProcessVariables(pkg.PackagePath)).ToArray() : list5.Select((FeatureManifest.FMPkgInfo pkg) => ProcessVariables(Path.ChangeExtension(pkg.PackagePath, PkgConstants.c_strPackageExtension))).ToArray());
				string text3 = feature;
				if (!string.IsNullOrEmpty(fmItem.ID))
				{
					text3 = text3 + "." + fmItem.ID;
				}
				MergeResult[] source3 = Package.MergePackage(inputPkgs, _outputPackageDir, text3, _version, fmItem.Owner, fmItem.ownerType, fmItem.releaseType, _cpuId, _buildType, _compress, incremental);
				if (source3.Count() != 0)
				{
					List<MergeResult> list6 = source3.ToList();
					if (_convertToCBS)
					{
						for (int i = 0; i < list6.Count(); i++)
						{
							list6[i].FilePath = Path.ChangeExtension(list6[i].FilePath, PkgConstants.c_strCBSPackageExtension);
						}
					}
					if (templatePkg == null)
					{
						foreach (MergeResult item3 in list6)
						{
							if (item3.PkgInfo != null && !item3.PkgInfo.IsWow && item3.FeatureIdentifierPackage)
							{
								templatePkg = item3.PkgInfo;
								break;
							}
						}
					}
					newFM.AddPackagesFromMergeResult(list5, list6, _fmFileList.SupportedLanguages, _fmFileList.SupportedResolutions, _outputPackageDir, _packagePathReplacement);
				}
			});
			if (templatePkg == null)
			{
				LogUtil.Warning("The FM '{0}' contains no applicable packages.  The FM will not be saved to the output directory.", fmItem.Path);
				return true;
			}
			if (_convertToCBS)
			{
				List<string> list4 = (from pkg in newFM.GetAllPackagesByGroups(_fmFileList.SupportedLanguages, _fmFileList.SupportedLocales, _fmFileList.SupportedResolutions, _fmFileList.GetWowGuestCpuTypes(_cpuId), _buildTypeStr, _cpuId.ToString(), _msPackageRoot)
					select Path.ChangeExtension(pkg.PackagePath, PkgConstants.c_strPackageExtension)).ToList();
				_generatedSPKGs.AddRange(list4.Where((string pkg) => File.Exists(pkg) && !FileUtils.IsTargetUpToDate(pkg, Path.ChangeExtension(pkg, PkgConstants.c_strCBSPackageExtension))).ToList());
				GenerateCBSPackages(_generatedSPKGs);
				if (flag)
				{
					foreach (string item4 in list4)
					{
						AddPackageToPlatformManifest(Package.LoadFromCab(Path.ChangeExtension(item4, PkgConstants.c_strCBSPackageExtension)), fmItem);
					}
				}
				_generatedSPKGs.Clear();
			}
			if (flag2)
			{
				ProcessUserInstallableFM(_uiItem, newFM, templatePkg);
			}
			FinalizeFeatureManifest(newFM, text2, fmItem, templatePkg);
			if (_convertToCBS)
			{
				SetFIPInfo(newFM.GetAllPackagesByGroups(_fmFileList.SupportedLanguages, _fmFileList.SupportedLocales, _fmFileList.SupportedResolutions, _fmFileList.GetWowGuestCpuTypes(_cpuId), _buildTypeStr, _cpuId.ToString(), _msPackageRoot), fmItem);
			}
			return true;
		}

		private static void SetFIPInfo(List<FeatureManifest.FMPkgInfo> pkgs, FMCollectionItem fmItem)
		{
			List<FeatureManifest.FMPkgInfo> source = new List<FeatureManifest.FMPkgInfo>(pkgs);
			foreach (FeatureManifest.FMPkgInfo item in source.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.OEMDEVICEPLATFORM))
			{
				item.FMGroup = FeatureManifest.PackageGroups.DEVICE;
			}
			foreach (FeatureManifest.FMPkgInfo item2 in source.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FMGroup == FeatureManifest.PackageGroups.DEVICELAYOUT))
			{
				item2.FMGroup = FeatureManifest.PackageGroups.SOC;
			}
			foreach (string feature in source.Select((FeatureManifest.FMPkgInfo pkg) => pkg.FeatureID).Distinct(IgnoreCase).ToList())
			{
				List<FeatureManifest.FMPkgInfo> list = source.Where((FeatureManifest.FMPkgInfo pkg) => feature.Equals(pkg.FeatureID, StringComparison.OrdinalIgnoreCase)).ToList();
				FeatureManifest.FMPkgInfo fMPkgInfo = list.FirstOrDefault((FeatureManifest.FMPkgInfo pkg) => pkg.FeatureIdentifierPackage);
				if (fMPkgInfo == null)
				{
					fMPkgInfo = list.FirstOrDefault((FeatureManifest.FMPkgInfo pkg) => pkg.Partition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase));
				}
				PrepCBSFeature.Prep(fMPkgInfo.PackagePath, fmItem.ID, feature, fmItem.ownerType.ToString(), _cpuId.ToString(), list, true);
			}
		}

		private static void CheckForUserInstallableFM()
		{
			if (_fmFileList != null)
			{
				_uiItem = _fmFileList.FMs.FirstOrDefault((FMCollectionItem fmItem) => fmItem.UserInstallable);
				_msPhoneItem = _fmFileList.FMs.FirstOrDefault((FMCollectionItem fmItem) => Path.GetFileName(fmItem.Path).Equals("MicrosoftPhoneFM.xml", StringComparison.OrdinalIgnoreCase));
			}
		}

		private static void ProcessUserInstallableFM(FMCollectionItem userInstallableFMItem, FeatureManifest newMSPhoneFM, IPkgInfo templatePkgInfo)
		{
			PlatformManifestGen platformManifestGen = new PlatformManifestGen(userInstallableFMItem.MicrosoftFMGUID, _buildBranchInfo, _signInfoPath, userInstallableFMItem.releaseType, _iuLogger);
			FeatureManifest fm = new FeatureManifest();
			string text = userInstallableFMItem.Path;
			if (!string.IsNullOrEmpty(_inputFMDir))
			{
				text = text.ToUpper(CultureInfo.InvariantCulture).Replace("$(FMDIRECTORY)", _inputFMDir);
			}
			FeatureManifest.ValidateAndLoad(ref fm, text, _iuLogger);
			foreach (string item in fm.GetAllPackageFilesList(_fmFileList.SupportedLanguages, _fmFileList.SupportedResolutions, _fmFileList.SupportedLocales, _fmFileList.GetWowGuestCpuTypes(_cpuId), _buildTypeStr, _cpuId.ToString(), _msPackageRoot).ToList())
			{
				if (!File.Exists(item))
				{
					platformManifestGen.ErrorMessages.Append("Error: FeatureMerger!ProcessUserInstallableFM: The package file '" + item + "' does not exist.");
					continue;
				}
				IPkgInfo package = Package.LoadFromCab(item);
				platformManifestGen.AddPackage(package);
			}
			if (platformManifestGen.ErrorsFound)
			{
				_iuLogger.LogWarning(platformManifestGen.ErrorMessages.ToString());
			}
			CreatePlatformManifestPackage(platformManifestGen, PkgConstants.c_strMainOsPartition, newMSPhoneFM, templatePkgInfo, PlatformManifestGen.c_strPlatformManifestMainOSDevicePath, userInstallableFMItem.Path);
			SetFIPInfo(fm.GetAllPackagesByGroups(_fmFileList.SupportedLanguages, _fmFileList.SupportedResolutions, _fmFileList.SupportedLocales, _fmFileList.GetWowGuestCpuTypes(_cpuId), _buildTypeStr, _cpuId.ToString(), _msPackageRoot), userInstallableFMItem);
		}

		private static void AddPackageToPlatformManifest(IPkgInfo package, FMCollectionItem fmItem)
		{
			if (package.Partition.Equals(PkgConstants.c_strMainOsPartition, StringComparison.OrdinalIgnoreCase) || package.Partition.Equals(PkgConstants.c_strDataPartition, StringComparison.OrdinalIgnoreCase))
			{
				if (_pmMainOS == null)
				{
					_pmMainOS = new PlatformManifestGen(fmItem.MicrosoftFMGUID, _buildBranchInfo, _signInfoPath, fmItem.releaseType, _iuLogger);
				}
				_pmMainOS.AddPackage(package);
			}
			else if (package.Partition.Equals(PkgConstants.c_strUpdateOsPartition, StringComparison.OrdinalIgnoreCase))
			{
				if (package.OwnerType == OwnerType.Microsoft)
				{
					if (_pmUpdateOS == null)
					{
						_pmUpdateOS = new PlatformManifestGen(fmItem.MicrosoftFMGUID, _buildBranchInfo, _signInfoPath, fmItem.releaseType, _iuLogger);
					}
					_pmUpdateOS.AddPackage(package);
				}
			}
			else if (package.Partition.Equals(PkgConstants.c_strEfiPartition, StringComparison.OrdinalIgnoreCase))
			{
				if (_pmEFIESP == null)
				{
					_pmEFIESP = new PlatformManifestGen(fmItem.MicrosoftFMGUID, _buildBranchInfo, _signInfoPath, fmItem.releaseType, _iuLogger);
				}
				_pmEFIESP.AddPackage(package);
			}
		}

		private static void GenerateCBSPackage(string packageFile)
		{
			GenerateCBSPackages(new List<string> { packageFile });
		}

		private static void GenerateCBSPackages(List<string> packageList)
		{
			if (packageList.Count() != 0)
			{
				FileUtils.GetTempDirectory();
				PkgConvertDSM.CONVERTDSM_PARAMETERS_FLAGS Flags = PkgConvertDSM.CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_MAKE_CAB | PkgConvertDSM.CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_SIGN_OUTPUT | PkgConvertDSM.CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_SKIP_POLICY | PkgConvertDSM.CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_USE_FILENAME_AS_NAME | PkgConvertDSM.CONVERTDSM_PARAMETERS_FLAGS.CONVERTDSM_PARAMETERS_FLAGS_OUTPUT_NEXT_TO_INPUT;
				Parallel.ForEach(packageList.Distinct(StringComparer.OrdinalIgnoreCase).ToList(), delegate(string spkgFile)
				{
					List<string> packageList2 = new List<string> { spkgFile };
					PkgConvertDSM.ConvertPackagesToCBS(Flags, packageList2, null);
				});
			}
		}

		private static void AddCBSOnlyPackagesToFM(List<FeatureManifest.FMPkgInfo> list, FeatureManifest orgFM, ref FeatureManifest newFM)
		{
			if (!list.Any())
			{
				return;
			}
			List<PkgFile> list2 = new List<PkgFile>();
			foreach (FeatureManifest.FMPkgInfo fmPkgInfo in list.Where((FeatureManifest.FMPkgInfo pkg) => string.IsNullOrWhiteSpace(pkg.Language) && string.IsNullOrWhiteSpace(pkg.Resolution)).ToList())
			{
				PkgFile pkgEntry = orgFM.AllPackages.FirstOrDefault((PkgFile pkg) => pkg.ID.Equals(fmPkgInfo.ID) && pkg.FMGroup == fmPkgInfo.FMGroup && (pkg.FMGroup == FeatureManifest.PackageGroups.BASE || pkg.FMGroup == FeatureManifest.PackageGroups.KEYBOARD || pkg.FMGroup == FeatureManifest.PackageGroups.SPEECH || pkg.GroupValues.Contains(fmPkgInfo.GroupValue, StringComparer.OrdinalIgnoreCase)) && (pkg.CPUIds == null || pkg.CPUIds.Contains(_cpuId)));
				if (!list2.Any((PkgFile pkg) => pkg == pkgEntry))
				{
					newFM.AddPkgFile(pkgEntry);
					list2.Add(pkgEntry);
				}
			}
		}

		private static void FinalizeFeatureManifest(FeatureManifest fm, string newFMFilePath, FMCollectionItem item, IPkgInfo templatePackage)
		{
			bool featureIdentifierPackage = fm.BasePackages == null || fm.BasePackages.Count() == 0 || !fm.BasePackages.Any((PkgFile pkg) => pkg.FeatureIdentifierPackage);
			if (_doPlatformManifest && (item.ownerType == OwnerType.Microsoft || !string.IsNullOrEmpty(_binaryRoot)))
			{
				CreatePlatformManifestPackages(fm, templatePackage);
			}
			if (fm.OwnerType == OwnerType.Microsoft)
			{
				fm.BuildID = _buildID.ToString();
				fm.BuildInfo = _buildInfo;
			}
			using (IPkgBuilder pkgBuilder = Package.Create())
			{
				pkgBuilder.Partition = "MainOS";
				pkgBuilder.Owner = templatePackage.Owner;
				pkgBuilder.Platform = templatePackage.Platform;
				pkgBuilder.ReleaseType = templatePackage.ReleaseType;
				if (item.ValidateAsMicrosoftPhoneFM)
				{
					pkgBuilder.Component = "PhoneFM";
				}
				else
				{
					pkgBuilder.Component = Path.GetFileNameWithoutExtension(item.Path);
				}
				pkgBuilder.SubComponent = null;
				pkgBuilder.BuildString = templatePackage.BuildString;
				pkgBuilder.OwnerType = templatePackage.OwnerType;
				pkgBuilder.GroupingKey = (string.IsNullOrEmpty(item.ID) ? "" : (item.ID + "."));
				pkgBuilder.GroupingKey = FeatureManifest.PackageGroups.BASE.ToString();
				pkgBuilder.Version = templatePackage.Version;
				pkgBuilder.BuildType = templatePackage.BuildType;
				pkgBuilder.CpuType = templatePackage.CpuType;
				PkgFile pkgFile = new PkgFile();
				if (fm.BasePackages == null)
				{
					fm.BasePackages = new List<PkgFile>();
				}
				pkgFile.FeatureIdentifierPackage = featureIdentifierPackage;
				if (string.IsNullOrEmpty(_packagePathReplacement))
				{
					pkgFile.Directory = _outputPackageDir;
				}
				else
				{
					pkgFile.Directory = _packagePathReplacement;
				}
				pkgFile.Name = pkgBuilder.Name + (_convertToCBS ? PkgConstants.c_strCBSPackageExtension : PkgConstants.c_strPackageExtension);
				pkgFile.ID = pkgBuilder.Name;
				pkgFile.Partition = pkgBuilder.Partition;
				fm.BasePackages.Add(pkgFile);
				string fileName = Path.GetFileName(newFMFilePath);
				fm.WriteToFile(newFMFilePath);
				string text = "\\";
				text = ((item.ownerType != OwnerType.Microsoft) ? (text + DevicePaths.OEMFMPath + "\\" + fileName) : (text + DevicePaths.MSFMPath + "\\" + fileName));
				pkgBuilder.AddFile(FileType.Regular, newFMFilePath, text, FileAttributes.Normal, null);
				string text2 = Path.Combine(_outputPackageDir, pkgBuilder.Name + PkgConstants.c_strPackageExtension);
				pkgBuilder.SaveCab(text2);
				if (_convertToCBS)
				{
					GenerateCBSPackage(text2);
				}
			}
		}

		private static void CreatePlatformManifestPackages(FeatureManifest fm, IPkgInfo templatePackage)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_pmMainOS != null)
			{
				if (_pmMainOS.ErrorsFound)
				{
					stringBuilder.AppendLine(_pmMainOS.ErrorMessages.ToString());
				}
				CreatePlatformManifestPackage(_pmMainOS, PkgConstants.c_strMainOsPartition, fm, templatePackage, PlatformManifestGen.c_strPlatformManifestMainOSDevicePath);
				_pmMainOS = null;
			}
			if (_pmUpdateOS != null)
			{
				if (_pmUpdateOS.ErrorsFound)
				{
					stringBuilder.AppendLine(_pmUpdateOS.ErrorMessages.ToString());
				}
				CreatePlatformManifestPackage(_pmUpdateOS, PkgConstants.c_strUpdateOsPartition, fm, templatePackage, PlatformManifestGen.c_strPlatformManifestMainOSDevicePath);
				_pmUpdateOS = null;
			}
			if (_pmEFIESP != null)
			{
				if (_pmEFIESP.ErrorsFound)
				{
					stringBuilder.AppendLine(_pmEFIESP.ErrorMessages.ToString());
				}
				CreatePlatformManifestPackage(_pmEFIESP, PkgConstants.c_strEfiPartition, fm, templatePackage, PlatformManifestGen.c_strPlatformManifestEFIESPDevicePath);
				_pmEFIESP = null;
			}
			if (stringBuilder.Length > 0)
			{
				_iuLogger.LogWarning("Warning: FeatureMerger!CreatePlatformManifestPackages: Failed to create Platform Manfiests: " + Environment.NewLine + stringBuilder.ToString());
			}
		}

		private static void CreatePlatformManifestPackage(PlatformManifestGen pm, string partition, FeatureManifest fm, IPkgInfo templatePackage, string DeviceBasePath)
		{
			CreatePlatformManifestPackage(pm, partition, fm, templatePackage, DeviceBasePath, fm.SourceFile);
		}

		private static void CreatePlatformManifestPackage(PlatformManifestGen pm, string partition, FeatureManifest fm, IPkgInfo templatePackage, string DeviceBasePath, string fmName)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fmName);
			string text = Path.Combine(_pmPath, fileNameWithoutExtension + "." + partition + PlatformManifestGen.c_strPlatformManifestExtension);
			pm.WriteToFile(text);
			if (pm.ErrorsFound)
			{
				File.WriteAllText(text + ".err", pm.ErrorMessages.ToString());
			}
			using (IPkgBuilder pkgBuilder = Package.Create())
			{
				pkgBuilder.Partition = partition;
				pkgBuilder.Owner = templatePackage.Owner;
				pkgBuilder.Platform = templatePackage.Platform;
				pkgBuilder.ReleaseType = templatePackage.ReleaseType;
				pkgBuilder.Component = Path.GetFileNameWithoutExtension(fileNameWithoutExtension);
				pkgBuilder.SubComponent = PlatformManifestGen.c_strPlatformManifestSubcomponent + "." + partition;
				pkgBuilder.BuildString = templatePackage.BuildString;
				pkgBuilder.OwnerType = templatePackage.OwnerType;
				pkgBuilder.Version = templatePackage.Version;
				pkgBuilder.BuildType = templatePackage.BuildType;
				pkgBuilder.CpuType = templatePackage.CpuType;
				PkgFile pkgFile = new PkgFile();
				if (fm.BasePackages == null || fm.BasePackages.Count() == 0)
				{
					fm.BasePackages = new List<PkgFile>();
				}
				pkgFile.Directory = (string.IsNullOrEmpty(_packagePathReplacement) ? _outputPackageDir : _packagePathReplacement);
				pkgFile.Name = pkgBuilder.Name + (_convertToCBS ? PkgConstants.c_strCBSPackageExtension : PkgConstants.c_strPackageExtension);
				pkgFile.ID = pkgBuilder.Name;
				pkgFile.Partition = pkgBuilder.Partition;
				fm.BasePackages.Add(pkgFile);
				string destination = DeviceBasePath + fileNameWithoutExtension + PlatformManifestGen.c_strPlatformManifestExtension;
				pkgBuilder.AddFile(FileType.Regular, text, destination, FileAttributes.Normal, null);
				string text2 = Path.Combine(_outputPackageDir, pkgBuilder.Name + PkgConstants.c_strPackageExtension);
				pkgBuilder.SaveCab(text2);
				GenerateCBSPackage(text2);
			}
		}

		private static bool ParseVariables(string variablesStr)
		{
			if (variablesStr != null)
			{
				foreach (string item in variablesStr.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList())
				{
					List<string> list = item.Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries).ToList();
					if (list.Count != 2)
					{
						LogUtil.Error("The variable list contains the '{0}' entry which is not in the proper variable=value format", item);
						return false;
					}
					_variableLookup[list[0]] = list[1];
				}
			}
			_binaryRoot = Environment.GetEnvironmentVariable("BINARY_ROOT");
			if (string.IsNullOrEmpty(_binaryRoot))
			{
				_binaryRoot = Environment.GetEnvironmentVariable("_NTTREE");
				if (!string.IsNullOrEmpty(_binaryRoot))
				{
					_binaryRoot += ".WM.MC";
				}
			}
			if (!string.IsNullOrEmpty(_binaryRoot))
			{
				_doPlatformManifest = true;
				_signInfoPath = Path.Combine(_binaryRoot, PlatformManifestGen.c_strSignInfoDir);
				_pmPath = LongPath.GetFullPath(Path.Combine(_binaryRoot, PlatformManifestRelativeDir));
				if (!LongPathDirectory.Exists(_pmPath))
				{
					LongPathDirectory.CreateDirectory(_pmPath);
				}
				_buildBranchInfo = Environment.ExpandEnvironmentVariables("%_RELEASELABEL%.%_NTRAZZLEBUILDNUMBER%.%_QFELEVEL%.%_BUILDTIME%");
			}
			string text = null;
			if (_variableLookup.ContainsKey("_cpuType"))
			{
				text = _variableLookup["_cpuType"];
			}
			if (string.IsNullOrEmpty(text))
			{
				text = Environment.GetEnvironmentVariable("_BUILDARCH");
			}
			if (!string.IsNullOrEmpty(text))
			{
				bool ignoreCase = true;
				if (!Enum.TryParse<CpuId>(text, ignoreCase, out _cpuId))
				{
					LogUtil.Error("The variable cputype '{0}' is not recognized as a valid value", text);
					return false;
				}
			}
			if (_variableLookup.ContainsKey("buildtype"))
			{
				_buildTypeStr = _variableLookup["buildtype"];
			}
			if (string.IsNullOrEmpty(_buildTypeStr))
			{
				_buildTypeStr = Environment.GetEnvironmentVariable("buildtype");
				if (string.IsNullOrEmpty(_buildTypeStr))
				{
					_buildTypeStr = Environment.GetEnvironmentVariable("_buildtype");
				}
			}
			if (!string.IsNullOrEmpty(_buildTypeStr))
			{
				if (_buildTypeStr.Equals("fre", StringComparison.OrdinalIgnoreCase))
				{
					_buildType = BuildType.Retail;
				}
				else if (_buildTypeStr.Equals("chk", StringComparison.OrdinalIgnoreCase))
				{
					_buildType = BuildType.Checked;
				}
			}
			if (_variableLookup.ContainsKey("mspackageroot"))
			{
				_msPackageRoot = _variableLookup["mspackageroot"];
			}
			else if (!string.IsNullOrEmpty(_binaryRoot))
			{
				_msPackageRoot = Path.Combine(_binaryRoot, "Prebuilt");
			}
			string text2 = string.Empty;
			if (_variableLookup.ContainsKey("releasetype"))
			{
				text2 = _variableLookup["releasetype"];
			}
			if (!string.IsNullOrEmpty(text2))
			{
				bool ignoreCase2 = true;
				if (!Enum.TryParse<ReleaseType>(text2, ignoreCase2, out _releaseType))
				{
					LogUtil.Error("The variable releaseType '{0}' is not recognized as a valid value", text2);
					return false;
				}
			}
			return true;
		}

		private static string ProcessVariables(string input)
		{
			string text = input.ToLower(CultureInfo.InvariantCulture);
			foreach (string key in _variableLookup.Keys)
			{
				text = text.Replace("$(" + key + ")", _variableLookup[key].ToLower(CultureInfo.InvariantCulture));
				text = text.Replace("%" + key + "%", _variableLookup[key].ToLower(CultureInfo.InvariantCulture));
			}
			return text;
		}

		private static bool SetCmdLineParams()
		{
			try
			{
				_commandLineParser = new CommandLineParser();
				_commandLineParser.SetRequiredParameterString("InputFile", "Specify the input file. Can be either a FMFileList XML file or a single Feature Manifest XML file.");
				_commandLineParser.SetRequiredParameterString("OutputPackageDir", "Directory where merged packages will be placed.");
				_commandLineParser.SetRequiredParameterString("OutputPackageVersion", "Version string in the form of <major>.<minor>.<qfe>.<build>");
				_commandLineParser.SetRequiredParameterString("OutputFMDir", "Directory where Feature Manifests will be place.  Feature Manifests are generated with the same file name as the original Feature Manifest.");
				_commandLineParser.SetOptionalSwitchString("InputFMDir", "Directory where source Feature Manifests can be found. Required with FMFileList XML file.");
				_commandLineParser.SetOptionalSwitchString("Critical", "Reserved");
				_commandLineParser.SetOptionalSwitchString("FMID", "Required short ID for Feature Manifest used in merged package name to ensure features from different FM files don't collid.");
				_commandLineParser.SetOptionalSwitchString("Languages", "Supported UI language identifier list, separated by ';'. Required with single FM as InputFile.");
				_commandLineParser.SetOptionalSwitchString("Resolutions", "Supported resolutions identifier list, separated by ';'. Required with single FM as InputFile.");
				_commandLineParser.SetOptionalSwitchString("MergePackageRootReplacement", "Specifies a string to be used in the generated FM file for packages.  Replaces the OutputPackageDir in the package paths.");
				_commandLineParser.SetOptionalSwitchString("OwnerType", "Resulting Package owner type: Microsoft or OEM.  Ignored when specifying FM File List");
				_commandLineParser.SetOptionalSwitchString("OwnerName", "Name of package owner.  Ignored when specifying FM File List");
				_commandLineParser.SetOptionalSwitchBoolean("Incremental", "Specifies to only remerge existing merged packages when one of the sources packages has changed. Default is rebuild all.", false);
				_commandLineParser.SetOptionalSwitchBoolean("Compress", "Specifies to compress merged packages.", false);
				_commandLineParser.SetOptionalSwitchBoolean("ConvertToCBS", "Convert all output to CBS.  Default is SPKG and no CBS packages allowed in FMs", false);
				_commandLineParser.SetOptionalSwitchString("variables", "Additional variables used in the project file, syntax:<name>=<value>;<name>=<value>;...");
			}
			catch (Exception ex)
			{
				Console.WriteLine("FeatureMerger!SetCmdLineParams: Unable to set an option: {0}", ex.Message);
				return false;
			}
			return true;
		}

		private static bool LoadFMFiles(string inputFile)
		{
			try
			{
				LogUtil.Message("Trying to load file '{0}' as a FM file list ...", inputFile);
				IULogger iULogger = new IULogger();
				iULogger.ErrorLogger = null;
				iULogger.InformationLogger = null;
				_fmFileList = FMCollectionManifest.ValidateAndLoad(inputFile, iULogger);
				_singleFM = false;
				return true;
			}
			catch (Exception ex)
			{
				LogUtil.Message("Input file '{0}' doesn't look like a valid FM file list, see the following information:", inputFile);
				LogUtil.Message(ex.Message);
			}
			try
			{
				LogUtil.Message("Trying to load file '{0}' as a single FM file ...", inputFile);
				_singleFM = true;
				FMCollectionItem fMCollectionItem = new FMCollectionItem();
				fMCollectionItem.Path = inputFile;
				fMCollectionItem.releaseType = ReleaseType.Test;
				_fmFileList.FMs.Add(fMCollectionItem);
				return true;
			}
			catch (Exception ex2)
			{
				LogUtil.Message("Input file '{0}' is not a valid FM file, see the following information:", inputFile);
				LogUtil.Message(ex2.Message);
			}
			return false;
		}

		private static void DisplayUsage()
		{
			Console.WriteLine("FeatureMerger Usage Description:");
			Console.WriteLine(_commandLineParser.UsageString());
			Console.WriteLine("\tExamples:");
			Console.WriteLine("\t\tFeatureMerger C:\\PreMergeFMs\\OEMFM.xml C:\\MergedPackages 8.0.0.1 C:\\MergedFMs /Languages:en-us;de-de /Resolutions:480x800;720x1280;768x1280;1080x1920  /variables:_cputype=arm;buildtype=fre;releasetype=production");
			Console.WriteLine("\t\tFeatureMerger C:\\FMFileList.xml C:\\MergedPackages 8.0.0.1 C:\\MergedFMs /InputFMDir:C:\\PreMergeFMs /MergePackageRootReplacement:$(MSPackageRoot)\\Merged /variables:_cputype=arm;buildtype=fre");
		}
	}
}
