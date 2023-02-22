using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;
using Microsoft.WindowsPhone.Imaging;

namespace Microsoft.WindowsPhone.CompDB
{
	public class DesktopCompDBGen
	{
		private enum ClientTypes
		{
			Client,
			ClientN
		}

		public enum DesktopFilters
		{
			IncludeNone,
			IncludeAll,
			IncludeOnDemandFeatures,
			IncludeEditions,
			IncludeLanguagePacks,
			IncludeByFeatureIDs,
			CreateNeutralFeature,
			CreateLanguageFeature
		}

		public enum PackageTypes
		{
			EDITION,
			LANGPACK,
			FOD,
			APP,
			NGEN,
			ESD,
			UPDATEBOX
		}

		public static string c_EditionPackagesDirectory = "EditionPackages";

		public static string c_DesktopUpdatesDirectory = "UUP\\Desktop";

		public static string c_FODDirectory = "FeaturesOnDemand";

		public static string c_FODCabs = "Cabs";

		public static string c_NGENDirectory = "NGEN";

		public static string c_AppsDirectory = "Apps";

		public static string c_NeutralDirectory = "Neutral";

		public static string c_ClientDirectory = "Client";

		public static string c_ESDRoot = "UUP\\DESKTOP\\MetadataESDs";

		public static string c_BuildArchVar = "$(buildArch)";

		public static string c_WindowsEditionsMap = "data\\Windowseditions.xml";

		public static string c_EditionPackageMap = "EditionPackageMap.xml";

		public static string c_FODEditionsMapPattern = "FOD_InstallList_" + c_BuildArchVar + ".xml";

		public static string c_AppsEditionsMapPattern = "InstallList_" + c_BuildArchVar + ".xml";

		public static string c_WindowsDesktopProduct = "Desktop";

		private static string c_CabExtension = PkgConstants.c_strCBSPackageExtension;

		private static string c_WimExtenstion = ".wim";

		private static string c_EsdExtenstion = ".esd";

		private static string c_EsdFeaturePost = "_esd";

		private static string c_ClientEditions = string.Concat(ClientTypes.Client, "Editions");

		private static string c_ClientNEditions = string.Concat(ClientTypes.ClientN, "Editions");

		private static string c_LanguagePackPrefix = "Language.UI";

		private static string c_BaseNeutralFeatureID = "BaseNeutral";

		private static string c_BaseLanguageFeatureID = "BaseLanguage_";

		public static string c_MicrosoftDesktopConditionsFM_ID = "MSDC";

		public static string c_MicrosoftDesktopNeutralFM = "MicrosoftDesktopNeutralFM.xml";

		public static string c_MicrosoftDesktopNeutralFM_ID = "MSDN";

		public static string c_MicrosoftDesktopLangsFM = "MicrosoftDesktopLangsFM.xml";

		public static string c_MicrosoftDesktopLangsFM_ID = "MSDL";

		public static string c_MicrosoftDesktopEditionsFM = "MicrosoftDesktopEditionsFM.xml";

		public static string c_MicrosoftDesktopEditionsFM_ID = "MSDE";

		public static string c_MicrosoftDesktopToolsFM = "MicrosoftDesktopToolsFM.xml";

		public static string c_MicrosoftDesktopToolsFM_ID = "MSDT";

		public static string c_MicorosftDesktopConditionalFeaturesFMName = "MicrosoftDesktopConditionsFM.xml";

		public static string c_MicorosftDesktopChunksMappingFileName = "MicrosoftDesktopChunksMapping.xml";

		public static string c_UpdateBoxName = "WindowsUpdateBox";

		public static string c_UpdateBoxFilename = c_UpdateBoxName + ".exe";

		public static string c_UpdateBoxRoot = "WindowsUpdateBox";

		public static string c_NeutralLanguage = "Neutral";

		private static bool _bInitialized = false;

		private static IULogger _logger = new IULogger();

		private static string _sdxRoot;

		private static string _buildArch;

		private static string _pkgRoot;

		private static bool _generatingFMs = false;

		private static bool _generateHashes = false;

		private static Dictionary<ClientTypes, List<CompDBFeature>> _clientEditionsFeatures = new Dictionary<ClientTypes, List<CompDBFeature>>();

		private static Dictionary<CompDBFeature, List<CompDBFeaturePackage>> _featurePkgMap = new Dictionary<CompDBFeature, List<CompDBFeaturePackage>>();

		private static Dictionary<string, CompDBPackageInfo> _pkgMap = new Dictionary<string, CompDBPackageInfo>(StringComparer.OrdinalIgnoreCase);

		private static Dictionary<string, string> _pkgHashs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private static Dictionary<string, long> _pkgSizes = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

		private static Dictionary<ClientTypes, string> _editionVersionLookup = new Dictionary<ClientTypes, string>();

		private static HashSet<string> _processedOptionalPkgs = new HashSet<string>();

		public static void Initialize(string pkgRoot, string buildArch, string sdxRoot, IULogger logger)
		{
			_pkgRoot = pkgRoot;
			_buildArch = buildArch;
			_sdxRoot = sdxRoot;
			_logger = logger;
			_bInitialized = true;
		}

		public static BuildCompDB GenerateCompDB(FMCollection fmCollection, string fmDirectory, string msPackageRoot, string language, string buildType, CpuId buildArch, string buildInfo, IULogger logger)
		{
			return GenerateCompDB(fmCollection, fmDirectory, msPackageRoot, language, buildType, buildArch, buildInfo, logger, null, null, false);
		}

		public static BuildCompDB GenerateCompDB(FMCollection fmCollection, string fmDirectory, string msPackageRoot, string language, string buildType, CpuId buildArch, string buildInfo, IULogger logger, List<DesktopFilters> filters, List<string> featureIDs, bool generateHashes)
		{
			_generateHashes = generateHashes;
			if (!string.IsNullOrEmpty(fmCollection.Manifest.ChunkMappingsFile))
			{
				string chunkMappingFile = fmCollection.Manifest.GetChunkMappingFile(fmDirectory);
				if (LongPathFile.Exists(chunkMappingFile))
				{
					BuildCompDB.InitializeChunkMapping(chunkMappingFile, fmCollection.Manifest.SupportedLanguages, logger);
				}
			}
			BuildCompDB newCompDB = new BuildCompDB(logger);
			if (fmCollection.Manifest == null)
			{
				throw new ImageCommonException("ImageCommon::DesktopCompDBGen!GenerateCompDB: Unable to generate Build CompDB without a FM Collection.");
			}
			Initialize(msPackageRoot, buildArch.ToString(), null, logger);
			newCompDB.Product = fmCollection.Manifest.Product;
			newCompDB.BuildArch = buildArch.ToString();
			List<string> list = new List<string>();
			List<FMCollectionItem> list2 = new List<FMCollectionItem>();
			if (string.IsNullOrEmpty(language))
			{
				list2.AddRange(fmCollection.Manifest.FMs.Where((FMCollectionItem fm) => fm.ID.Equals(c_MicrosoftDesktopEditionsFM_ID, StringComparison.OrdinalIgnoreCase) || fm.ID.Equals(c_MicrosoftDesktopToolsFM_ID, StringComparison.OrdinalIgnoreCase)));
			}
			else if (language.Equals("Neutral", StringComparison.OrdinalIgnoreCase))
			{
				list2.AddRange(fmCollection.Manifest.FMs.Where((FMCollectionItem fm) => fm.ID.Equals(c_MicrosoftDesktopNeutralFM_ID, StringComparison.OrdinalIgnoreCase) || fm.ID.Equals(c_MicorosftDesktopConditionalFeaturesFMName, StringComparison.OrdinalIgnoreCase)));
			}
			else
			{
				if (string.IsNullOrEmpty(language))
				{
					list = fmCollection.Manifest.SupportedLanguages;
				}
				else
				{
					list.Add(language);
				}
				list2.AddRange(fmCollection.Manifest.FMs.Where((FMCollectionItem fm) => fm.ID.Equals(c_MicrosoftDesktopLangsFM_ID, StringComparison.OrdinalIgnoreCase)));
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			Func<string, bool> func = default(Func<string, bool>);
			foreach (FMCollectionItem item in list2)
			{
				if (buildArch != item.CPUType && item.CPUType != 0)
				{
					continue;
				}
				FeatureManifest fm2 = new FeatureManifest();
				string text = Environment.ExpandEnvironmentVariables(item.Path);
				text = item.ResolveFMPath(fmDirectory);
				FeatureManifest.ValidateAndLoad(ref fm2, text, _logger);
				if (fm2.OwnerType == OwnerType.Microsoft && newCompDB.BuildID == Guid.Empty && !string.IsNullOrEmpty(fm2.BuildID))
				{
					newCompDB.BuildID = new Guid(fm2.BuildID);
					newCompDB.BuildInfo = fm2.BuildInfo;
					newCompDB.OSVersion = fm2.OSVersion;
				}
				List<FeatureManifest.FMPkgInfo> allPackageByGroups = fm2.GetAllPackageByGroups(list, fmCollection.Manifest.SupportedLocales, fmCollection.Manifest.SupportedResolutions, buildType, _buildArch, msPackageRoot);
				if (list.Any())
				{
					foreach (FeatureManifest.FMPkgInfo item2 in allPackageByGroups)
					{
						if (!string.IsNullOrEmpty(item2.Language))
						{
							item2.PackagePath = item2.PackagePath.Replace(PkgFile.DefaultLanguagePattern + item2.Language, "", StringComparison.OrdinalIgnoreCase);
							item2.ID = item2.ID.Replace(PkgFile.DefaultLanguagePattern + item2.Language, "", StringComparison.OrdinalIgnoreCase);
						}
					}
				}
				List<string> list3 = allPackageByGroups.Select((FeatureManifest.FMPkgInfo pkg) => pkg.FeatureID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
				if (filters != null && filters.Contains(DesktopFilters.IncludeByFeatureIDs))
				{
					List<string> source = list3;
					Func<string, bool> func2 = func;
					if (func2 == null)
					{
						func2 = (func = (string feat) => featureIDs.Contains(FMFeatures.GetFeatureIDWithoutPrefix(feat), StringComparer.OrdinalIgnoreCase));
					}
					list3 = source.Where(func2).ToList();
				}
				else if (string.IsNullOrEmpty(language))
				{
					list3.Remove("MS_" + c_BaseNeutralFeatureID);
					list3.Remove("MS_" + c_BaseLanguageFeatureID);
				}
				foreach (string feature in list3)
				{
					List<FeatureManifest.FMPkgInfo> list4 = allPackageByGroups.Where((FeatureManifest.FMPkgInfo pkg) => pkg.FeatureID.Equals(feature, StringComparison.OrdinalIgnoreCase)).ToList();
					CompDBFeature compDBFeature = new CompDBFeature(feature, item.ID, CompDBFeature.CompDBFeatureTypes.MobileFeature, (item.ownerType == OwnerType.Microsoft) ? item.ownerType.ToString() : OwnerType.OEM.ToString());
					compDBFeature.FeatureID = FMFeatures.GetFeatureIDWithoutPrefix(compDBFeature.FeatureID);
					foreach (FeatureManifest.FMPkgInfo pkg3 in list4)
					{
						if (!newCompDB.Packages.Any((CompDBPackageInfo pkg2) => pkg2.ID.Equals(pkg3.ID, StringComparison.OrdinalIgnoreCase)))
						{
							AssemblyIdentity assemblyIdentity = new AssemblyIdentity();
							if (!language.Equals("Neutral", StringComparison.OrdinalIgnoreCase))
							{
								assemblyIdentity.language = language;
							}
							assemblyIdentity.buildType = "fre";
							assemblyIdentity.name = pkg3.ID;
							assemblyIdentity.processorArchitecture = buildArch.ToString();
							if (pkg3.Version.HasValue)
							{
								assemblyIdentity.version = pkg3.Version.ToString();
							}
							CompDBPackageInfo compDBPackageInfo = GenerateFeaturePackage(compDBFeature, pkg3.ID, assemblyIdentity, pkg3.PackagePath, language, msPackageRoot);
							if ((string.IsNullOrEmpty(compDBPackageInfo.VersionStr) || compDBPackageInfo.VersionStr.Equals("0.0.0.0")) && !string.IsNullOrEmpty(assemblyIdentity.version))
							{
								compDBPackageInfo.VersionStr = assemblyIdentity.version;
							}
							newCompDB.Packages.Add(compDBPackageInfo);
						}
						else
						{
							CompDBFeaturePackage compDBFeaturePackage = newCompDB.Features.SelectMany((CompDBFeature feat) => feat.Packages).FirstOrDefault((CompDBFeaturePackage featPkg) => featPkg.ID.Equals(pkg3.ID, StringComparison.OrdinalIgnoreCase));
							if (compDBFeaturePackage == null)
							{
								flag = true;
								stringBuilder.AppendLine("Package '" + pkg3.ID + "' found in Packages but not in any features.");
							}
							else
							{
								compDBFeature.Packages.Add(compDBFeaturePackage);
							}
						}
					}
					compDBFeature.Type = GetFeatureTypeFromGrouping(fm2.Features.MSFeatureGroups, compDBFeature.FeatureID);
					newCompDB.Features.Add(compDBFeature);
				}
				if (fm2.Features != null && fm2.Features.MSConditionalFeatures != null && fm2.Features.MSConditionalFeatures.Count() != 0)
				{
					if (newCompDB.MSConditionalFeatures == null)
					{
						newCompDB.MSConditionalFeatures = new List<FMConditionalFeature>();
					}
					newCompDB.MSConditionalFeatures.AddRange(fm2.Features.MSConditionalFeatures);
				}
			}
			if (flag)
			{
				throw new ImageCommonException("ImageCommon::DesktopCompDBGen!GenerateCompDB: Errors processing FM File(s):\n" + stringBuilder.ToString());
			}
			newCompDB.Packages = newCompDB.Packages.Distinct(CompDBPackageInfoComparer.Standard).ToList();
			newCompDB.Packages = newCompDB.Packages.Select((CompDBPackageInfo pkg) => pkg.SetParentDB(newCompDB)).ToList();
			if (newCompDB.BuildID == Guid.Empty)
			{
				newCompDB.BuildID = Guid.NewGuid();
				newCompDB.BuildInfo = buildInfo;
			}
			return newCompDB;
		}

		private static CompDBFeature.CompDBFeatureTypes GetFeatureTypeFromGrouping(List<FMFeatureGrouping> groupings, string feature)
		{
			CompDBFeature.CompDBFeatureTypes result = CompDBFeature.CompDBFeatureTypes.None;
			if (groupings != null && groupings.Any())
			{
				FMFeatureGrouping fMFeatureGrouping = groupings.FirstOrDefault((FMFeatureGrouping grp) => grp.FeatureIDs.Contains(feature, StringComparer.OrdinalIgnoreCase) && !string.IsNullOrEmpty(grp.GroupingType));
				if (fMFeatureGrouping != null && !Enum.TryParse<CompDBFeature.CompDBFeatureTypes>(fMFeatureGrouping.GroupingType, out result))
				{
					result = CompDBFeature.CompDBFeatureTypes.None;
				}
			}
			return result;
		}

		public static void GenerateCompDBFMs(List<string> langs, Guid buildID, string buildInfo, string fmDirectory, string conditionalFeaturesFMFile)
		{
			if (!_bInitialized)
			{
				throw new Exception("ImageCommon::DesktopCompDBGen!GenerateCompDBFMs: Function called before being initialized.");
			}
			if (!LongPathDirectory.Exists(fmDirectory))
			{
				LongPathDirectory.CreateDirectory(fmDirectory);
			}
			_generatingFMs = true;
			FMCollectionManifest fMCollectionManifest = new FMCollectionManifest();
			fMCollectionManifest.Product = c_WindowsDesktopProduct;
			fMCollectionManifest.SupportedLanguages = langs;
			CpuId result;
			if (!Enum.TryParse<CpuId>(_buildArch, out result))
			{
				result = CpuId.Invalid;
			}
			if (LongPathFile.Exists(Path.Combine(fmDirectory, c_MicorosftDesktopChunksMappingFileName)))
			{
				fMCollectionManifest.ChunkMappingsFile = FMCollection.c_FMDirectoryVariable + "\\" + c_MicorosftDesktopChunksMappingFileName;
			}
			string text = null;
			FeatureManifest fm = null;
			FMCollectionItem fMCollectionItem;
			if (!string.IsNullOrEmpty(conditionalFeaturesFMFile) && LongPathFile.Exists(conditionalFeaturesFMFile))
			{
				string fileName = Path.GetFileName(conditionalFeaturesFMFile);
				text = LongPath.Combine(fmDirectory, fileName);
				fm = new FeatureManifest();
				FeatureManifest.ValidateAndLoad(ref fm, conditionalFeaturesFMFile, _logger);
				fMCollectionItem = new FMCollectionItem();
				fMCollectionItem.CPUType = result;
				fMCollectionItem.ID = (fm.ID = c_MicrosoftDesktopConditionsFM_ID);
				FMCollectionItem fMCollectionItem2 = fMCollectionItem;
				string owner = (fm.Owner = OwnerType.Microsoft.ToString());
				fMCollectionItem2.Owner = owner;
				fMCollectionItem.ownerType = (fm.OwnerType = OwnerType.Microsoft);
				fMCollectionItem.releaseType = (fm.ReleaseType = ReleaseType.Production);
				fMCollectionItem.Path = FMCollection.c_FMDirectoryVariable + "\\" + fileName;
				fm.BuildID = buildID.ToString();
				fm.BuildInfo = buildInfo;
				fMCollectionManifest.FMs.Add(fMCollectionItem);
			}
			fMCollectionItem = new FMCollectionItem();
			FeatureManifest fm2 = new FeatureManifest();
			InitializeFMItem(ref fMCollectionItem, ref fm2, c_MicrosoftDesktopNeutralFM_ID, c_MicrosoftDesktopNeutralFM, result, fmDirectory, buildID.ToString(), buildInfo);
			fMCollectionManifest.FMs.Add(fMCollectionItem);
			fMCollectionItem = new FMCollectionItem();
			FeatureManifest fm3 = new FeatureManifest();
			InitializeFMItem(ref fMCollectionItem, ref fm3, c_MicrosoftDesktopLangsFM_ID, c_MicrosoftDesktopLangsFM, result, fmDirectory, buildID.ToString(), buildInfo);
			fMCollectionManifest.FMs.Add(fMCollectionItem);
			fMCollectionItem = new FMCollectionItem();
			FeatureManifest fm4 = new FeatureManifest();
			InitializeFMItem(ref fMCollectionItem, ref fm4, c_MicrosoftDesktopEditionsFM_ID, c_MicrosoftDesktopEditionsFM, result, fmDirectory, buildID.ToString(), buildInfo);
			fMCollectionManifest.FMs.Add(fMCollectionItem);
			fMCollectionItem = new FMCollectionItem();
			FeatureManifest fm5 = new FeatureManifest();
			InitializeFMItem(ref fMCollectionItem, ref fm5, c_MicrosoftDesktopToolsFM_ID, c_MicrosoftDesktopToolsFM, result, fmDirectory, buildID.ToString(), buildInfo);
			fMCollectionManifest.FMs.Add(fMCollectionItem);
			BuildCompDB buildCompDB = null;
			foreach (string lang in langs)
			{
				BuildCompDB buildCompDB2 = GenerateCompDB(lang, buildID, buildInfo);
				if (buildCompDB == null)
				{
					buildCompDB = buildCompDB2;
				}
				else
				{
					buildCompDB.Merge(buildCompDB2);
				}
				_pkgMap = new Dictionary<string, CompDBPackageInfo>(StringComparer.OrdinalIgnoreCase);
				_featurePkgMap = new Dictionary<CompDBFeature, List<CompDBFeaturePackage>>();
				_clientEditionsFeatures = new Dictionary<ClientTypes, List<CompDBFeature>>();
			}
			BuildCompDB srcDB = GenerateCompDBForOptionalPkgs(c_NeutralLanguage, buildID, buildInfo);
			buildCompDB.Merge(srcDB);
			fm2.OSVersion = _editionVersionLookup[ClientTypes.Client];
			fm3.OSVersion = _editionVersionLookup[ClientTypes.Client];
			fm4.OSVersion = _editionVersionLookup[ClientTypes.Client];
			fm5.OSVersion = _editionVersionLookup[ClientTypes.Client];
			if (fm != null && !string.IsNullOrEmpty(text))
			{
				fm.OSVersion = _editionVersionLookup[ClientTypes.Client];
				fm.WriteToFile(text);
			}
			List<CompDBFeature> list = buildCompDB.Features.Where((CompDBFeature feat) => feat.Type == CompDBFeature.CompDBFeatureTypes.LanguagePack).ToList();
			List<string> langPkgIDs = (from pkg in list.SelectMany((CompDBFeature feat) => feat.Packages)
				select pkg.ID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			buildCompDB.Packages.Where((CompDBPackageInfo pkg) => langPkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
			FMFeatureGrouping fMFeatureGrouping = new FMFeatureGrouping();
			fMFeatureGrouping.GroupingType = CompDBFeature.CompDBFeatureTypes.LanguagePack.ToString();
			fMFeatureGrouping.FeatureIDs = list.Select((CompDBFeature feat) => feat.FeatureID).ToList();
			fm3.Features.MSFeatureGroups = new List<FMFeatureGrouping> { fMFeatureGrouping };
			List<CompDBPackageInfo> source = buildCompDB.Packages.Where((CompDBPackageInfo pkg) => pkg.SatelliteType == CompDBPackageInfo.SatelliteTypes.Language && !langPkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
			if (source.Any())
			{
				CompDBFeature compDBFeature = new CompDBFeature();
				compDBFeature.Type = CompDBFeature.CompDBFeatureTypes.None;
				compDBFeature.Group = OwnerType.Microsoft.ToString();
				compDBFeature.FeatureID = c_BaseLanguageFeatureID;
				compDBFeature.Packages = source.Select((CompDBPackageInfo pkg) => new CompDBFeaturePackage(pkg.ID, false)).ToList();
				list.Add(compDBFeature);
				langPkgIDs.AddRange(compDBFeature.Packages.Select((CompDBFeaturePackage pkg) => pkg.ID));
			}
			List<CompDBPackageInfo> packages = buildCompDB.Packages;
			Func<CompDBPackageInfo, bool> func = default(Func<CompDBPackageInfo, bool>);
			Func<CompDBPackageInfo, bool> func2 = func;
			if (func2 == null)
			{
				func2 = (func = (CompDBPackageInfo pkg) => langPkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase));
			}
			foreach (CompDBPackageInfo pkg4 in packages.Where(func2))
			{
				List<string> featureIDs = (from feat in list
					where feat.Packages.Any((CompDBFeaturePackage featPkg) => featPkg.ID.Equals(pkg4.ID, StringComparison.OrdinalIgnoreCase))
					select feat.FeatureID).ToList();
				MSOptionalPkgFile mSOptionalPkgFile = new MSOptionalPkgFile();
				mSOptionalPkgFile.FeatureIdentifierPackage = false;
				mSOptionalPkgFile.FeatureIDs = featureIDs;
				mSOptionalPkgFile.Language = "(" + pkg4.SatelliteValue + ")";
				mSOptionalPkgFile.NoBasePackage = true;
				mSOptionalPkgFile.ID = pkg4.ID;
				string path = pkg4.FirstPayloadItem.Path;
				mSOptionalPkgFile.Directory = "$(mspackageroot)\\" + LongPath.GetDirectoryName(path);
				mSOptionalPkgFile.Name = LongPath.GetFileName(path);
				mSOptionalPkgFile.Version = pkg4.VersionStr;
				fm3.Features.Microsoft.Add(mSOptionalPkgFile);
			}
			string fileName2 = Path.Combine(fmDirectory, c_MicrosoftDesktopLangsFM);
			fm3.WriteToFile(fileName2);
			List<CompDBFeature> list2 = buildCompDB.Features.Where((CompDBFeature feat) => feat.Type == CompDBFeature.CompDBFeatureTypes.OnDemandFeature).ToList();
			List<string> neutralPkgIDs = (from pkg in list2.SelectMany((CompDBFeature feat) => feat.Packages)
				select pkg.ID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			buildCompDB.Packages.Where((CompDBPackageInfo pkg) => neutralPkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
			FMFeatureGrouping fMFeatureGrouping2 = new FMFeatureGrouping();
			fMFeatureGrouping2.GroupingType = CompDBFeature.CompDBFeatureTypes.OnDemandFeature.ToString();
			fMFeatureGrouping2.FeatureIDs = list2.Select((CompDBFeature feat) => feat.FeatureID).ToList();
			fm2.Features.MSFeatureGroups = new List<FMFeatureGrouping> { fMFeatureGrouping2 };
			List<CompDBPackageInfo> source2 = buildCompDB.Packages.Where((CompDBPackageInfo pkg) => pkg.SatelliteType != CompDBPackageInfo.SatelliteTypes.Language && !neutralPkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).Distinct().ToList();
			if (source2.Any())
			{
				CompDBFeature compDBFeature2 = new CompDBFeature();
				compDBFeature2.Type = CompDBFeature.CompDBFeatureTypes.None;
				compDBFeature2.Group = OwnerType.Microsoft.ToString();
				compDBFeature2.FeatureID = c_BaseNeutralFeatureID;
				compDBFeature2.Packages = source2.Select((CompDBPackageInfo pkg) => new CompDBFeaturePackage(pkg.ID, false)).ToList();
				list2.Add(compDBFeature2);
				neutralPkgIDs.AddRange(compDBFeature2.Packages.Select((CompDBFeaturePackage pkg) => pkg.ID));
			}
			List<CompDBPackageInfo> packages2 = buildCompDB.Packages;
			Func<CompDBPackageInfo, bool> func3 = default(Func<CompDBPackageInfo, bool>);
			Func<CompDBPackageInfo, bool> func4 = func3;
			if (func4 == null)
			{
				func4 = (func3 = (CompDBPackageInfo pkg) => neutralPkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase));
			}
			foreach (CompDBPackageInfo pkg3 in packages2.Where(func4))
			{
				List<string> featureIDs2 = (from feat in list2
					where feat.Packages.Any((CompDBFeaturePackage featPkg) => featPkg.ID.Equals(pkg3.ID, StringComparison.OrdinalIgnoreCase))
					select feat.FeatureID).ToList();
				MSOptionalPkgFile mSOptionalPkgFile2 = new MSOptionalPkgFile();
				mSOptionalPkgFile2.FeatureIdentifierPackage = false;
				mSOptionalPkgFile2.FeatureIDs = featureIDs2;
				mSOptionalPkgFile2.ID = pkg3.ID;
				string path2 = pkg3.FirstPayloadItem.Path;
				mSOptionalPkgFile2.Directory = "$(mspackageroot)\\" + LongPath.GetDirectoryName(path2);
				mSOptionalPkgFile2.Name = LongPath.GetFileName(path2);
				mSOptionalPkgFile2.Version = pkg3.VersionStr;
				fm2.Features.Microsoft.Add(mSOptionalPkgFile2);
			}
			string fileName3 = Path.Combine(fmDirectory, c_MicrosoftDesktopNeutralFM);
			fm2.WriteToFile(fileName3);
			List<CompDBFeature> list3 = buildCompDB.Features.Where((CompDBFeature feat) => feat.Type == CompDBFeature.CompDBFeatureTypes.DesktopMedia).ToList();
			List<string> editionPkgIDs = (from pkg in list3.SelectMany((CompDBFeature feat) => feat.Packages)
				select pkg.ID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			FMFeatureGrouping fMFeatureGrouping3 = new FMFeatureGrouping();
			fMFeatureGrouping3.GroupingType = CompDBFeature.CompDBFeatureTypes.DesktopMedia.ToString();
			fMFeatureGrouping3.FeatureIDs = list3.Select((CompDBFeature feat) => feat.FeatureID).ToList();
			fm4.Features.MSFeatureGroups = new List<FMFeatureGrouping> { fMFeatureGrouping3 };
			List<CompDBPackageInfo> packages3 = buildCompDB.Packages;
			Func<CompDBPackageInfo, bool> func5 = default(Func<CompDBPackageInfo, bool>);
			Func<CompDBPackageInfo, bool> func6 = func5;
			if (func6 == null)
			{
				func6 = (func5 = (CompDBPackageInfo pkg) => editionPkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase));
			}
			foreach (CompDBPackageInfo pkg2 in packages3.Where(func6))
			{
				List<string> featureIDs3 = (from feat in list3
					where feat.Packages.Any((CompDBFeaturePackage featPkg) => featPkg.ID.Equals(pkg2.ID, StringComparison.OrdinalIgnoreCase))
					select feat.FeatureID).ToList();
				MSOptionalPkgFile mSOptionalPkgFile3 = new MSOptionalPkgFile();
				mSOptionalPkgFile3.FeatureIdentifierPackage = false;
				mSOptionalPkgFile3.FeatureIDs = featureIDs3;
				mSOptionalPkgFile3.ID = pkg2.ID;
				string path3 = pkg2.FirstPayloadItem.Path;
				mSOptionalPkgFile3.Directory = "$(mspackageroot)\\" + LongPath.GetDirectoryName(path3);
				mSOptionalPkgFile3.Name = LongPath.GetFileName(path3);
				mSOptionalPkgFile3.Version = pkg2.VersionStr;
				fm4.Features.Microsoft.Add(mSOptionalPkgFile3);
			}
			List<CompDBChunkMapItem> list4 = new List<CompDBChunkMapItem>();
			foreach (CompDBFeature item in list3)
			{
				MSOptionalPkgFile mSOptionalPkgFile4 = new MSOptionalPkgFile();
				mSOptionalPkgFile4.FeatureIdentifierPackage = true;
				mSOptionalPkgFile4.FeatureIDs = new List<string>();
				mSOptionalPkgFile4.FeatureIDs.Add(item.FeatureID);
				mSOptionalPkgFile4.ID = item.FeatureID + c_EsdFeaturePost;
				string text3 = Path.Combine(c_ESDRoot, item.FeatureID);
				mSOptionalPkgFile4.Directory = "$(mspackageroot)\\" + text3;
				mSOptionalPkgFile4.Name = item.FeatureID + c_EsdExtenstion;
				mSOptionalPkgFile4.Version = _editionVersionLookup[GetEditionFeatureClientType(item, buildCompDB.Packages)];
				fm4.Features.Microsoft.Add(mSOptionalPkgFile4);
				CompDBChunkMapItem compDBChunkMapItem = new CompDBChunkMapItem();
				compDBChunkMapItem.Type = PackageTypes.ESD;
				compDBChunkMapItem.ChunkName = $"MetaESD_{item.FeatureID}";
				compDBChunkMapItem.Path = text3;
				list4.Add(compDBChunkMapItem);
			}
			string fileName4 = Path.Combine(fmDirectory, c_MicrosoftDesktopEditionsFM);
			fm4.WriteToFile(fileName4);
			MSOptionalPkgFile mSOptionalPkgFile5 = new MSOptionalPkgFile();
			mSOptionalPkgFile5.FeatureIdentifierPackage = true;
			mSOptionalPkgFile5.FeatureIDs = new List<string>();
			mSOptionalPkgFile5.FeatureIDs.Add(c_UpdateBoxName);
			mSOptionalPkgFile5.ID = c_UpdateBoxName;
			mSOptionalPkgFile5.Directory = "$(mspackageroot)\\" + c_UpdateBoxRoot;
			mSOptionalPkgFile5.Name = c_UpdateBoxFilename;
			fm5.Features.Microsoft.Add(mSOptionalPkgFile5);
			FMFeatureGrouping fMFeatureGrouping4 = new FMFeatureGrouping();
			fMFeatureGrouping4.GroupingType = CompDBFeature.CompDBFeatureTypes.Tool.ToString();
			fMFeatureGrouping4.FeatureIDs = mSOptionalPkgFile5.FeatureIDs;
			fm5.Features.MSFeatureGroups = new List<FMFeatureGrouping>();
			fm5.Features.MSFeatureGroups.Add(fMFeatureGrouping4);
			string fileName5 = Path.Combine(fmDirectory, c_MicrosoftDesktopToolsFM);
			fm5.WriteToFile(fileName5);
			string xmlFile = Path.Combine(fmDirectory, "FMFileList.xml");
			fMCollectionManifest.WriteToFile(xmlFile);
			if (!string.IsNullOrEmpty(fMCollectionManifest.ChunkMappingsFile))
			{
				string chunkMappingFile = fMCollectionManifest.GetChunkMappingFile(fmDirectory);
				CompDBChunkMapping compDBChunkMapping = CompDBChunkMapping.ValidateAndLoad(chunkMappingFile, fMCollectionManifest.SupportedLanguages, _logger);
				compDBChunkMapping.ChunkMappings.RemoveAll((CompDBChunkMapItem map) => map.Type == PackageTypes.ESD);
				compDBChunkMapping.ChunkMappings.AddRange(list4);
				compDBChunkMapping.WriteToFile(chunkMappingFile);
			}
		}

		private static ClientTypes GetEditionFeatureClientType(CompDBFeature editionFeature, List<CompDBPackageInfo> packages)
		{
			List<string> featurePkgIDs = editionFeature.Packages.Select((CompDBFeaturePackage pkg) => pkg.ID).ToList();
			if (packages.Any((CompDBPackageInfo pkg) => featurePkgIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase) && pkg.FirstPayloadItem.Path.ToUpper(CultureInfo.InvariantCulture).Contains(c_ClientNEditions.ToUpper(CultureInfo.InvariantCulture))))
			{
				return ClientTypes.ClientN;
			}
			return ClientTypes.Client;
		}

		private static void InitializeFMItem(ref FMCollectionItem fmItem, ref FeatureManifest fm, string FMID, string fmFile, CpuId cpuType, string fmDirectory, string buildID, string buildInfo)
		{
			fmItem.CPUType = cpuType;
			fmItem.ID = (fm.ID = FMID);
			FMCollectionItem obj = fmItem;
			string owner = (fm.Owner = OwnerType.Microsoft.ToString());
			obj.Owner = owner;
			fmItem.ownerType = (fm.OwnerType = OwnerType.Microsoft);
			fmItem.releaseType = (fm.ReleaseType = ReleaseType.Production);
			fmItem.Path = FMCollection.c_FMDirectoryVariable + "\\" + fmFile;
			fmItem.Critical = true;
			fm.BuildID = buildID;
			fm.BuildInfo = buildInfo;
			fm.Features = new FMFeatures();
			fm.Features.Microsoft = new List<MSOptionalPkgFile>();
		}

		public static BuildCompDB GenerateCompDBForOptionalPkgs(string lang, Guid buildID, string buildInfo)
		{
			if (!_bInitialized)
			{
				throw new Exception("ImageCommon::DesktopCompDBGen!GenerateCompDB: Function called before being initialized.");
			}
			BuildCompDB buildCompDB = new BuildCompDB(_logger);
			buildCompDB.Product = c_WindowsDesktopProduct;
			buildCompDB.ReleaseType = ReleaseType.Production;
			buildCompDB.BuildID = buildID;
			buildCompDB.BuildInfo = buildInfo;
			buildCompDB.BuildArch = _buildArch;
			try
			{
				LongPath.Combine(_pkgRoot, c_DesktopUpdatesDirectory);
				_logger.LogInfo("ImageCommon::DesktopCompDBGen: Processing FOD ({0} and {1})...", c_NeutralDirectory, lang);
				GenerateCompDBForRemainingOptionalPkgs(Path.GetFullPath(Path.Combine(_pkgRoot, c_FODDirectory, c_NeutralDirectory, c_FODCabs)), PackageTypes.FOD, lang, _pkgRoot);
				buildCompDB.Features = CompDBFeatures();
				buildCompDB.Packages = CompDBPackages();
				return buildCompDB;
			}
			catch (Exception ex)
			{
				_logger.LogException(ex);
				Environment.Exit(Marshal.GetHRForException(ex));
				return buildCompDB;
			}
		}

		public static BuildCompDB GenerateCompDB(string lang, Guid buildID, string buildInfo)
		{
			if (!_bInitialized)
			{
				throw new Exception("ImageCommon::DesktopCompDBGen!GenerateCompDB: Function called before being initialized.");
			}
			BuildCompDB buildCompDB = new BuildCompDB(_logger);
			buildCompDB.Product = c_WindowsDesktopProduct;
			buildCompDB.ReleaseType = ReleaseType.Production;
			buildCompDB.BuildID = buildID;
			buildCompDB.BuildInfo = buildInfo;
			buildCompDB.BuildArch = _buildArch;
			try
			{
				string path = LongPath.Combine(_pkgRoot, c_DesktopUpdatesDirectory);
				string text = LongPath.Combine(path, c_EditionPackagesDirectory);
				_logger.LogInfo("ImageCommon::DesktopCompDBGen: Processing All Editions ({0} and {1})...", c_NeutralDirectory, lang);
				CompDBForAllEditions(LongPath.Combine(text, c_NeutralDirectory), lang, _pkgRoot);
				CompDBForOtherPackages(Path.Combine(text, lang, c_ClientDirectory), PackageTypes.LANGPACK, lang, _pkgRoot);
				_logger.LogInfo("ImageCommon::DesktopCompDBGen: Processing FOD ({0} and {1})...", c_NeutralDirectory, lang);
				string path2 = LongPath.Combine(_pkgRoot, c_FODDirectory);
				GenerateCompDB(editionsMap: LongPath.Combine(_sdxRoot, "data\\DistribData\\mc\\FeaturesOnDemand\\" + c_FODEditionsMapPattern.Replace(c_BuildArchVar, _buildArch, StringComparison.OrdinalIgnoreCase)), pkgPath: Path.GetFullPath(path2), pkgType: PackageTypes.FOD, lang: lang, pkgRoot: _pkgRoot);
				string text2 = LongPath.Combine(path, c_NGENDirectory);
				if (Directory.Exists(text2))
				{
					_logger.LogInfo("ImageCommon::DesktopCompDBGen: Processing NGEN...");
					CompDBForOtherPackages(text2, PackageTypes.NGEN, lang, _pkgRoot);
				}
				string text3 = LongPath.Combine(path, c_AppsDirectory);
				if (Directory.Exists(text3))
				{
					_logger.LogInfo("ImageCommon::DesktopCompDBGen: Processing APPS...");
					string editionsMap2 = LongPath.Combine(_sdxRoot, "data\\" + c_AppsEditionsMapPattern.Replace(c_BuildArchVar, _buildArch, StringComparison.OrdinalIgnoreCase));
					GenerateCompDB(text3, PackageTypes.APP, lang, _pkgRoot, editionsMap2);
				}
				buildCompDB.Features = CompDBFeatures();
				buildCompDB.Packages = CompDBPackages();
				return buildCompDB;
			}
			catch (Exception ex)
			{
				_logger.LogException(ex);
				Environment.Exit(Marshal.GetHRForException(ex));
				return buildCompDB;
			}
		}

		public static string PackageIDFromAssemblyIdentity(AssemblyIdentity ai)
		{
			return string.Format("{0}_{1}{2}", ai.processorArchitecture, ai.name, ai.language.Equals("Neutral", StringComparison.OrdinalIgnoreCase) ? "" : ("_" + ai.language));
		}

		private static List<CompDBFeature> CompDBFeatures()
		{
			List<CompDBFeature> list = _featurePkgMap.Keys.ToList();
			_logger.LogInfo("ImageCommon::DesktopCompDBGen: Generated {0} DesktopCompDB Features", list.Count);
			return list;
		}

		private static List<CompDBPackageInfo> CompDBPackages()
		{
			_logger.LogInfo("ImageCommon::DesktopCompDBGen: Generated {0} DesktopCompDB Packages", _pkgMap.Values.Count());
			return _pkgMap.Values.ToList();
		}

		private static void CompDBForAllEditions(string pkgPath, string lang, string pkgRoot)
		{
			if (!Directory.Exists(pkgPath))
			{
				throw new DirectoryNotFoundException("ImageCommon::DesktopCompDBGen!CompDBForAllEditions: Invalid package path:" + pkgPath);
			}
			string editionsPkgMap = Path.Combine(pkgPath, c_EditionPackageMap);
			Dictionary<string, List<string>> dictionary = GenerateEditionsMap(PackageTypes.EDITION, lang, editionsPkgMap);
			XDocument xDocument = XDocument.Load(LongPath.Combine(_sdxRoot, c_WindowsEditionsMap));
			XNamespace RNS = "urn:schemas-microsoft-com:windows:editions:v1";
			foreach (string edition in dictionary.Keys)
			{
				string text = (from e1 in xDocument.Root.Elements(RNS + "Edition")
					where e1.Element(RNS + "Name").Value.ToString().Equals(edition, StringComparison.InvariantCultureIgnoreCase) && e1.Element(RNS + "Media") != null && (e1.Attribute(RNS + "legacy") == null || !e1.Attribute(RNS + "legacy").Value.Equals("true"))
					select e1.Element(RNS + "Media").Value).First();
				ClientTypes result;
				if (Enum.TryParse<ClientTypes>(text, out result))
				{
					if (!_clientEditionsFeatures.ContainsKey(result))
					{
						_clientEditionsFeatures.Add(result, new List<CompDBFeature>());
					}
					string featureId = edition + "_" + lang;
					foreach (string item2 in dictionary[edition])
					{
						string text2 = Path.Combine(pkgRoot, item2);
						if (LongPathFile.Exists(text2))
						{
							AssemblyIdentity assemblyIdMum = GetAssemblyIdMum(Path.ChangeExtension(text2, PkgConstants.c_strMumExtension));
							_editionVersionLookup[result] = assemblyIdMum.version;
							CompDBFeature item = GenerateFeaturePackageMapping(featureId, assemblyIdMum, text2, lang, pkgRoot);
							if (!_clientEditionsFeatures[result].Contains(item))
							{
								_clientEditionsFeatures[result].Add(item);
							}
						}
					}
					continue;
				}
				throw new ImageCommonException("ImageCommon::DesktopCompDBGen!CompDBForAllEditions: Unknown Client Type: " + text);
			}
		}

		private static void CompDBForOtherPackages(string pkgPathRoot, PackageTypes pkgType, string lang, string pkgRoot)
		{
			List<string> obj = new List<string> { c_WimExtenstion, c_EsdExtenstion, c_CabExtension };
			string path = pkgPathRoot;
			if (_generatingFMs && pkgType == PackageTypes.LANGPACK && !lang.Equals("en-us", StringComparison.OrdinalIgnoreCase))
			{
				path = pkgPathRoot.Replace(lang, "en-us", StringComparison.OrdinalIgnoreCase);
			}
			foreach (string item in obj)
			{
				string[] files = Directory.GetFiles(path, "*" + item);
				foreach (string obj2 in files)
				{
					AssemblyIdentity assemblyIdFromPkg = GetAssemblyIdFromPkg(obj2);
					string optionalFeatureId = null;
					string text = obj2;
					if (pkgType == PackageTypes.LANGPACK)
					{
						optionalFeatureId = c_LanguagePackPrefix + "~" + lang;
						if (!lang.Equals("en-us", StringComparison.OrdinalIgnoreCase))
						{
							text = text.Replace("en-us", lang, StringComparison.OrdinalIgnoreCase);
							assemblyIdFromPkg.language = lang;
						}
					}
					GenerateFeaturePackageMapping(string.Concat(ClientTypes.Client, "_", lang), assemblyIdFromPkg, text, lang, pkgRoot, pkgType, optionalFeatureId);
					GenerateFeaturePackageMapping(string.Concat(ClientTypes.ClientN, "_", lang), assemblyIdFromPkg, text, lang, pkgRoot);
				}
			}
		}

		private static void GenerateCompDB(string pkgPath, PackageTypes pkgType, string lang, string pkgRoot, string editionsMap)
		{
			if (string.IsNullOrEmpty(editionsMap) || !LongPathFile.Exists(editionsMap))
			{
				throw new ImageCommonException("ImageCommon::DesktopCompDBGen!GenerateCompDB: Invalid editions map specified: " + editionsMap);
			}
			Dictionary<string, List<string>> dictionary = GenerateEditionsMap(pkgType, lang, editionsMap);
			foreach (string key in dictionary.Keys)
			{
				foreach (string item in dictionary[key])
				{
					string text = LongPath.Combine(pkgPath, item);
					if (!LongPathFile.Exists(text))
					{
						text = Path.ChangeExtension(text, c_EsdExtenstion);
					}
					if (LongPathFile.Exists(text))
					{
						AssemblyIdentity ai = null;
						string featureId = null;
						if (pkgType == PackageTypes.FOD)
						{
							GetAssemblyIdAndFeatureIdFromCab(text, out ai, out featureId);
						}
						else
						{
							ai = GetAssemblyIdFromPkg(text);
						}
						GenerateFeaturePackageMapping(key + "_" + lang, ai, text, lang, pkgRoot, pkgType, featureId);
						_processedOptionalPkgs.Add(text);
					}
				}
			}
		}

		private static void GenerateCompDBForRemainingOptionalPkgs(string pkgPath, PackageTypes pkgType, string lang, string pkgRoot)
		{
			string[] files = Directory.GetFiles(pkgPath, "*" + c_CabExtension);
			foreach (string pkgPath2 in files)
			{
				if (_processedOptionalPkgs.Contains(pkgPath))
				{
					continue;
				}
				AssemblyIdentity ai = null;
				string featureId = null;
				if (pkgType == PackageTypes.FOD)
				{
					GetAssemblyIdAndFeatureIdFromCab(pkgPath2, out ai, out featureId);
					if (!string.IsNullOrEmpty(featureId))
					{
						GenerateFeaturePackageMapping(string.Empty, ai, pkgPath2, lang, pkgRoot, pkgType, featureId, true);
					}
				}
			}
		}

		private static Dictionary<string, List<string>> GenerateEditionsMap(PackageTypes pkgType, string lang, string editionsPkgMap)
		{
			XDocument xDocument = XDocument.Load(editionsPkgMap);
			Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
			XDocument xDocument2 = XDocument.Load(LongPath.Combine(_sdxRoot, c_WindowsEditionsMap));
			XNamespace RNS = "urn:schemas-microsoft-com:windows:editions:v1";
			List<string> media = (from e1 in xDocument2.Root.Elements(RNS + "Edition")
				where e1.Element(RNS + "Media") != null && (e1.Attribute(RNS + "legacy") == null || !e1.Attribute(RNS + "legacy").Value.Equals("true"))
				select e1.Element(RNS + "Media").Value).Distinct().ToList();
			Func<XElement, bool> func = default(Func<XElement, bool>);
			foreach (XElement item in from p in xDocument.Root.Elements("Product")
				where media.Contains(p.Attribute("name").Value.ToString())
				select p)
			{
				foreach (XElement item2 in item.Elements("Edition"))
				{
					List<string> list = new List<string>();
					string text = ((pkgType == PackageTypes.APP) ? "App" : "Package");
					foreach (XElement item3 in item2.Elements(text))
					{
						IEnumerable<XElement> source = item3.Elements("lang");
						Func<XElement, bool> func2 = func;
						if (func2 == null)
						{
							func2 = (func = (XElement x) => x.Value.ToString().Equals(lang) || x.Value.ToString().Equals("*"));
						}
						if (source.FirstOrDefault(func2) != null)
						{
							string text2 = item3.Attribute("name").Value;
							if (Path.GetExtension(text2).Equals(".appxbundle"))
							{
								text2 = text2.Replace(".appxbundle", c_EsdExtenstion, StringComparison.OrdinalIgnoreCase);
							}
							list.Add(text2);
						}
					}
					if (list.Count() > 0)
					{
						string value = item2.Attribute("name").Value;
						if (value.Equals("All", StringComparison.OrdinalIgnoreCase))
						{
							value = item.Attribute("name").Value;
						}
						if (dictionary.ContainsKey(value))
						{
							dictionary[value].AddRange(list);
						}
						else
						{
							dictionary.Add(value, list);
						}
						_logger.LogInfo("ImageCommon::DesktopCompDBGen: Generating editionsMap. Edition: {0}, Num of packages: {1}", value, dictionary[value].Count);
					}
				}
			}
			return dictionary;
		}

		public static AssemblyIdentity GetAssemblyIdFromPkg(string pkgPath)
		{
			if (!_bInitialized)
			{
				throw new Exception("ImageCommon::DesktopCompDBGen!GetAssemblyIdFromPkg: Function called before being initialized.");
			}
			if (Path.GetExtension(pkgPath).Equals(c_WimExtenstion, StringComparison.OrdinalIgnoreCase) || Path.GetExtension(pkgPath).Equals(c_EsdExtenstion, StringComparison.OrdinalIgnoreCase))
			{
				return GetAssemblyIdFromWim(pkgPath);
			}
			if (Path.GetExtension(pkgPath).Equals(c_CabExtension, StringComparison.OrdinalIgnoreCase))
			{
				return GetAssemblyIdFromCab(pkgPath);
			}
			throw new ImageCommonException("ImageCommon::DesktopCompDBGen!GetAssemblyIdFromPkg: Invalid package type:" + Path.GetExtension(pkgPath));
		}

		private static AssemblyIdentity GetAssemblyIdFromWim(string pkgPath)
		{
			XElement xElement = null;
			string text = LongPath.Combine(Path.GetTempPath(), PkgConstants.c_strMumFile);
			Process process = new Process();
			process.StartInfo.FileName = LongPath.Combine(_sdxRoot, "tools\\PostBuildScripts\\DistribData\\esd\\tools\\x64\\esdtool.exe");
			process.StartInfo.Arguments = " /wimextract " + pkgPath + " 1";
			ProcessStartInfo startInfo = process.StartInfo;
			startInfo.Arguments = startInfo.Arguments + " " + PkgConstants.c_strMumFile + " " + text;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = true;
			_logger.LogInfo("ImageCommon::DesktopCompDBGen: Calling ESDTool with arguments: " + process.StartInfo.Arguments);
			process.Start();
			process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			if (process.ExitCode == 0)
			{
				XDocument xDocument = XDocument.Load(text);
				XNamespace @namespace = xDocument.Root.Name.Namespace;
				xElement = xDocument.Root.Element(@namespace + "assemblyIdentity");
			}
			else
			{
				_logger.LogInfo("ImageCommon::DesktopCompDBGen: EsdTool_exit_code: {0}", process.ExitCode.ToString());
				xElement = new XElement((XNamespace)"urn:schemas-microsoft-com:asm.v3" + "assemblyIdentity");
				xElement.Add(new XAttribute("buildType", "release"));
				xElement.Add(new XAttribute("language", "neutral"));
				xElement.Add(new XAttribute("name", Path.GetFileNameWithoutExtension(pkgPath)));
				xElement.Add(new XAttribute("processorArchitecture", _buildArch));
				xElement.Add(new XAttribute("publicKeyToken", "0"));
				xElement.Add(new XAttribute("version", "00.0.00000.0000"));
			}
			return GetAssemblyIdentity(xElement);
		}

		private static void GetAssemblyIdAndFeatureIdFromCab(string pkgPath, out AssemblyIdentity ai, out string featureId)
		{
			string mumFileFromCab = GetMumFileFromCab(pkgPath);
			ai = GetAssemblyIdMum(mumFileFromCab);
			featureId = GetFeatureIdFromMum(mumFileFromCab);
		}

		private static string GetMumFileFromCab(string pkgPath)
		{
			string tempFile = FileUtils.GetTempFile();
			Directory.CreateDirectory(tempFile);
			string text = LongPath.Combine(tempFile, PkgConstants.c_strMumFile);
			Process process = new Process();
			process.StartInfo.FileName = Environment.ExpandEnvironmentVariables("%systemroot%\\system32\\expand.exe");
			process.StartInfo.Arguments = " " + pkgPath;
			ProcessStartInfo startInfo = process.StartInfo;
			startInfo.Arguments = startInfo.Arguments + " -F:" + PkgConstants.c_strMumFile;
			startInfo = process.StartInfo;
			startInfo.Arguments = startInfo.Arguments + " " + tempFile;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.RedirectStandardOutput = true;
			_logger.LogInfo("ImageCommon::DesktopCompDBGen: Calling Expand.exe with arguments: " + process.StartInfo.Arguments);
			process.Start();
			_logger.LogInfo(process.StandardOutput.ReadToEnd());
			process.WaitForExit();
			_logger.LogInfo("ImageCommon::DesktopCompDBGen: Expand.exe ExitCode: {0}", process.ExitCode.ToString());
			if (process.ExitCode != 0 || !LongPathFile.Exists(text))
			{
				throw new ImageCommonException("ImageCommon::DesktopCompDBGen!GetMumFileFromCab: Unable to extract update.mum from :" + pkgPath);
			}
			return text;
		}

		private static AssemblyIdentity GetAssemblyIdFromCab(string pkgPath)
		{
			return GetAssemblyIdMum(GetMumFileFromCab(pkgPath));
		}

		private static AssemblyIdentity GetAssemblyIdMum(string mumPath)
		{
			XDocument xDocument = XDocument.Load(mumPath);
			XNamespace @namespace = xDocument.Root.Name.Namespace;
			return GetAssemblyIdentity(xDocument.Root.Element(@namespace + "assemblyIdentity"));
		}

		private static string GetFeatureIdFromMum(string mumPath)
		{
			XDocument xDocument = XDocument.Load(mumPath);
			XNamespace @namespace = xDocument.Root.Name.Namespace;
			XElement xElement = xDocument.Root.Element(@namespace + "package");
			bool num = xElement.Element(@namespace + "declareCapability") != null;
			bool flag = xElement.Element(@namespace + "parent") != null && xElement.Element(@namespace + "parent").Element(@namespace + "assemblyIdentity").Attribute("name")
				.Value.ToString().Contains("NanoServer-Edition");
			if (num && !flag)
			{
				XElement xElement2 = xDocument.Root.Element(@namespace + "package").Element(@namespace + "declareCapability").Element(@namespace + "capability")
					.Element(@namespace + "capabilityIdentity");
				string value = xElement2.Attribute("name").Value;
				string text = null;
				string text2 = null;
				if (xElement2.Attribute("language") != null)
				{
					text = xElement2.Attribute("language").Value;
				}
				if (xElement2.Attribute("version") != null)
				{
					text2 = xElement2.Attribute("version").Value;
				}
				return value + "~" + (string.IsNullOrEmpty(text) ? "" : text) + (string.IsNullOrEmpty(text2) ? "" : ("~" + text2));
			}
			return string.Empty;
		}

		public static AssemblyIdentity GetAssemblyIdentity(XElement assemblyIdentity)
		{
			return (AssemblyIdentity)new XmlSerializer(typeof(AssemblyIdentity)).Deserialize(assemblyIdentity.CreateReader());
		}

		public static CompDBPackageInfo GenerateFeaturePackage(CompDBFeature feature, AssemblyIdentity ai, string pkgPath, string lang, string pkgRoot)
		{
			string id = PackageIDFromAssemblyIdentity(ai);
			return GenerateFeaturePackage(feature, id, ai, pkgPath, lang, pkgRoot);
		}

		public static CompDBPackageInfo GenerateFeaturePackage(CompDBFeature feature, string id, AssemblyIdentity ai, string pkgPath, string lang, string pkgRoot)
		{
			CompDBFeaturePackage featurePackage = new CompDBFeaturePackage();
			CompDBPackageInfo compDBPackageInfo = new CompDBPackageInfo();
			compDBPackageInfo.OwnerType = OwnerType.Microsoft;
			compDBPackageInfo.ReleaseType = ReleaseType.Production;
			featurePackage.ID = (compDBPackageInfo.ID = id);
			if (feature.Packages.Any((CompDBFeaturePackage pkg) => pkg.ID.Equals(featurePackage.ID, StringComparison.OrdinalIgnoreCase)))
			{
				return null;
			}
			if (!ai.processorArchitecture.Equals(_buildArch, StringComparison.OrdinalIgnoreCase))
			{
				compDBPackageInfo.BuildArchOverride = ai.processorArchitecture;
			}
			if (!string.IsNullOrEmpty(ai.language) && !ai.language.Equals("neutral", StringComparison.OrdinalIgnoreCase))
			{
				compDBPackageInfo.SatelliteType = CompDBPackageInfo.SatelliteTypes.Language;
				compDBPackageInfo.SatelliteValue = ai.language;
			}
			compDBPackageInfo.PublicKeyToken = ai.publicKeyToken;
			VersionInfo versionInfo = default(VersionInfo);
			if (VersionInfo.TryParse(ai.version, out versionInfo))
			{
				compDBPackageInfo.Version = versionInfo;
			}
			CompDBPayloadInfo compDBPayloadInfo = new CompDBPayloadInfo(pkgPath, pkgRoot, compDBPackageInfo, false);
			compDBPackageInfo.Payload.Add(compDBPayloadInfo);
			if (!_generatingFMs && _generateHashes)
			{
				string key = compDBPayloadInfo.Path.ToUpper(CultureInfo.InvariantCulture);
				if (_pkgHashs.ContainsKey(key))
				{
					compDBPayloadInfo.PayloadHash = _pkgHashs[key];
					compDBPayloadInfo.PayloadSize = _pkgSizes[key];
				}
				else
				{
					compDBPayloadInfo.PayloadHash = CompDBPackageInfo.GetPackageHash(pkgPath);
					_pkgHashs.Add(key, compDBPayloadInfo.PayloadHash);
					compDBPayloadInfo.PayloadSize = CompDBPackageInfo.GetPackageSize(pkgPath);
					_pkgSizes.Add(key, compDBPayloadInfo.PayloadSize);
				}
			}
			if (pkgPath.ToUpper(CultureInfo.InvariantCulture).Contains(c_ESDRoot.ToUpper(CultureInfo.InvariantCulture)))
			{
				featurePackage.PackageType = CompDBFeaturePackage.PackageTypes.MetadataESD;
			}
			feature.Packages.Add(featurePackage);
			return compDBPackageInfo;
		}

		public static void PopulateHashesLookup(BuildCompDB srcCompDB)
		{
			foreach (CompDBPackageInfo package in srcCompDB.Packages)
			{
				foreach (CompDBPayloadInfo item in package.Payload)
				{
					string key = item.Path.ToUpper(CultureInfo.InvariantCulture);
					if (!string.IsNullOrEmpty(item.PayloadHash) && !_pkgHashs.ContainsKey(key))
					{
						_pkgHashs.Add(key, item.PayloadHash);
					}
					if (item.PayloadSize > 0 && !_pkgSizes.ContainsKey(key))
					{
						_pkgSizes.Add(key, item.PayloadSize);
					}
				}
			}
		}

		private static CompDBFeature GenerateFeaturePackageMapping(string featureId, AssemblyIdentity ai, string pkgPath, string lang, string pkgRoot, PackageTypes pkgType = PackageTypes.FOD, string optionalFeatureId = null, bool optionalFeatureOnly = false)
		{
			CompDBFeature compDBFeature = new CompDBFeature();
			CompDBPackageInfo compDBPackageInfo = GenerateFeaturePackage(compDBFeature, ai, pkgPath, lang, pkgRoot);
			CompDBFeaturePackage featurePackage = compDBFeature.FindPackage(compDBPackageInfo.ID);
			if (!optionalFeatureOnly)
			{
				ClientTypes result;
				if (Enum.TryParse<ClientTypes>(featureId.Replace("_" + lang, "", StringComparison.OrdinalIgnoreCase), out result))
				{
					if (_clientEditionsFeatures[result] == null)
					{
						throw new ImageCommonException("ImageCommon::DesktopCompDBGen!GenerateFeaturePackageMapping: The client '" + result.ToString() + "' was not found.  Ensure all Editions are processed before doing feature mapping for FOD\\Apps.");
					}
					Func<CompDBFeaturePackage, bool> func = default(Func<CompDBFeaturePackage, bool>);
					foreach (CompDBFeature item in _clientEditionsFeatures[result])
					{
						List<CompDBFeaturePackage> source = _featurePkgMap[item];
						Func<CompDBFeaturePackage, bool> func2 = func;
						if (func2 == null)
						{
							func2 = (func = (CompDBFeaturePackage pkg) => pkg.ID.Equals(featurePackage.ID, StringComparison.OrdinalIgnoreCase));
						}
						if (!source.Any(func2))
						{
							_featurePkgMap[item].Add(featurePackage);
						}
					}
				}
				else
				{
					CompDBFeature compDBFeature2 = _featurePkgMap.Keys.FirstOrDefault((CompDBFeature feat) => feat.FeatureID.Equals(featureId, StringComparison.OrdinalIgnoreCase));
					if (compDBFeature2 == null)
					{
						CompDBFeature compDBFeature3 = new CompDBFeature(featureId, null, CompDBFeature.CompDBFeatureTypes.DesktopMedia, OwnerType.Microsoft.ToString());
						_featurePkgMap.Add(compDBFeature3, compDBFeature3.Packages);
						compDBFeature = compDBFeature3;
					}
					else
					{
						compDBFeature = compDBFeature2;
					}
					if (!_featurePkgMap[compDBFeature].Any((CompDBFeaturePackage pkg) => pkg.ID.Equals(featurePackage.ID, StringComparison.OrdinalIgnoreCase)))
					{
						_featurePkgMap[compDBFeature].Add(featurePackage);
					}
				}
			}
			if (!string.IsNullOrEmpty(optionalFeatureId) && _featurePkgMap.Keys.FirstOrDefault((CompDBFeature feat) => feat.FeatureID.Equals(optionalFeatureId, StringComparison.OrdinalIgnoreCase)) == null)
			{
				CompDBFeature.CompDBFeatureTypes type = ((pkgType == PackageTypes.LANGPACK) ? CompDBFeature.CompDBFeatureTypes.LanguagePack : CompDBFeature.CompDBFeatureTypes.OnDemandFeature);
				CompDBFeature compDBFeature4 = new CompDBFeature(optionalFeatureId, null, type, OwnerType.Microsoft.ToString());
				compDBFeature4.Packages.Add(featurePackage);
				_featurePkgMap.Add(compDBFeature4, compDBFeature4.Packages);
				_logger.LogInfo("ImageCommon::DesktopCompDBGen: Generating feature package mapping. Feature: {0}, Package: {1}", optionalFeatureId, featurePackage.ID);
			}
			if (!_pkgMap.ContainsKey(compDBPackageInfo.ID))
			{
				_pkgMap[compDBPackageInfo.ID] = compDBPackageInfo;
			}
			_logger.LogInfo("ImageCommon::DesktopCompDBGen: Generating feature package mapping. Feature: {0}, Package: {1}", featureId, featurePackage.ID);
			return compDBFeature;
		}

		public static void FilterCompDB(BuildCompDB compDB, List<DesktopFilters> filters, List<string> features)
		{
			if (filters.Contains(DesktopFilters.IncludeNone))
			{
				compDB.Features = new List<CompDBFeature>();
				compDB.Packages = new List<CompDBPackageInfo>();
				return;
			}
			List<CompDBFeature> list = new List<CompDBFeature>();
			foreach (CompDBFeature feature in compDB.Features)
			{
				switch (feature.Type)
				{
				case CompDBFeature.CompDBFeatureTypes.LanguagePack:
					if (filters.Contains(DesktopFilters.IncludeLanguagePacks))
					{
						list.Add(feature);
						continue;
					}
					break;
				case CompDBFeature.CompDBFeatureTypes.OnDemandFeature:
					if (filters.Contains(DesktopFilters.IncludeOnDemandFeatures))
					{
						list.Add(feature);
						continue;
					}
					break;
				case CompDBFeature.CompDBFeatureTypes.DesktopMedia:
					if (filters.Contains(DesktopFilters.IncludeEditions))
					{
						list.Add(feature);
						continue;
					}
					break;
				}
				if (filters.Contains(DesktopFilters.IncludeByFeatureIDs) && features.Contains(feature.FeatureIDWithFMID, StringComparer.OrdinalIgnoreCase))
				{
					list.Add(feature);
				}
			}
			if (filters.Contains(DesktopFilters.CreateNeutralFeature))
			{
				CompDBFeature compDBFeature = new CompDBFeature(c_BaseNeutralFeatureID, null, CompDBFeature.CompDBFeatureTypes.None, OwnerType.Microsoft.ToString());
				compDBFeature.Packages = (from pkg in compDB.Packages
					where pkg.SatelliteType == CompDBPackageInfo.SatelliteTypes.Base
					select new CompDBFeaturePackage(pkg.ID, false)).ToList();
				List<string> alreadyInOtherFeatures2 = list.SelectMany((CompDBFeature feat) => feat.Packages.Select((CompDBFeaturePackage pkg) => pkg.ID)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
				compDBFeature.Packages.RemoveAll((CompDBFeaturePackage pkg) => alreadyInOtherFeatures2.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase));
				if (compDBFeature.Packages.Any())
				{
					list.Add(compDBFeature);
				}
			}
			if (filters.Contains(DesktopFilters.CreateLanguageFeature))
			{
				string satelliteValue = compDB.Packages.FirstOrDefault((CompDBPackageInfo pkg) => pkg.SatelliteType == CompDBPackageInfo.SatelliteTypes.Language).SatelliteValue;
				CompDBFeature compDBFeature2 = new CompDBFeature(c_BaseLanguageFeatureID + satelliteValue, null, CompDBFeature.CompDBFeatureTypes.None, OwnerType.Microsoft.ToString());
				compDBFeature2.Packages = (from pkg in compDB.Packages
					where pkg.SatelliteType == CompDBPackageInfo.SatelliteTypes.Language
					select new CompDBFeaturePackage(pkg.ID, false)).ToList();
				List<string> alreadyInOtherFeatures = list.SelectMany((CompDBFeature feat) => feat.Packages.Select((CompDBFeaturePackage pkg) => pkg.ID)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
				compDBFeature2.Packages.RemoveAll((CompDBFeaturePackage pkg) => alreadyInOtherFeatures.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase));
				if (compDBFeature2.Packages.Any())
				{
					list.Add(compDBFeature2);
				}
			}
			compDB.Features = list;
			List<string> filteredPackageIDs = list.SelectMany((CompDBFeature feat) => feat.Packages.Select((CompDBFeaturePackage pkg) => pkg.ID)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			compDB.Packages = compDB.Packages.Where((CompDBPackageInfo pkg) => filteredPackageIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
		}
	}
}
