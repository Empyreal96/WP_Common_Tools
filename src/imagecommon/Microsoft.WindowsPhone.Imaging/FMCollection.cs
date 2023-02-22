using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	public class FMCollection
	{
		public static string c_FMDirectoryVariable = "$(FMDIRECTORY)";

		public FMCollectionManifest Manifest;

		public IULogger Logger;

		private StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

		public void LoadFromManifest(string xmlFile, IULogger logger)
		{
			Logger = logger;
			Manifest = FMCollectionManifest.ValidateAndLoad(xmlFile, Logger);
		}

		public PublishingPackageList GetPublishingPackageList2(string fmDirectory, string msPackageRoot, string buildType, CpuId cpuType, bool cbsBased)
		{
			return GetPublishingPackageList2(fmDirectory, msPackageRoot, buildType, cpuType, cbsBased, false);
		}

		public PublishingPackageList GetPublishingPackageList2(string fmDirectory, string msPackageRoot, string buildType, CpuId cpuType, bool cbsBased, bool skipMissingPackages)
		{
			PublishingPackageList publishingPackageList = GetPublishingPackageList(fmDirectory, msPackageRoot, buildType, cpuType, skipMissingPackages);
			foreach (PublishingPackageInfo package in publishingPackageList.Packages)
			{
				string path = Path.Combine(msPackageRoot, package.Path);
				if (FileUtils.IsTargetUpToDate(package.Path, Path.ChangeExtension(path, PkgConstants.c_strCBSPackageExtension)))
				{
					package.Path = Path.ChangeExtension(package.Path, PkgConstants.c_strCBSPackageExtension);
				}
			}
			return publishingPackageList;
		}

		public PublishingPackageList GetPublishingPackageList(string fmDirectory, string msPackageRoot, string buildType, CpuId cpuType)
		{
			return GetPublishingPackageList(fmDirectory, msPackageRoot, buildType, cpuType, false);
		}

		public PublishingPackageList GetPublishingPackageList(string fmDirectory, string msPackageRoot, string buildType, CpuId cpuType, bool skipMissingPackages)
		{
			PublishingPackageList fullList = new PublishingPackageList();
			fullList.MSFeatureGroups = new List<FMFeatureGrouping>();
			fullList.OEMFeatureGroups = new List<FMFeatureGrouping>();
			if (Manifest == null)
			{
				throw new ImageCommonException("ImageCommon!GetPublishingPackageList: Unable to generate Publishing Package List without a FM Collection.");
			}
			fullList.IsTargetFeatureEnabled = Manifest.IsBuildFeatureEnabled;
			if (!Manifest.IsBuildFeatureEnabled)
			{
				if (Manifest.FeatureIdentifierPackages != null)
				{
					fullList.FeatureIdentifierPackages = Manifest.FeatureIdentifierPackages;
				}
				else
				{
					fullList.FeatureIdentifierPackages = new List<FeatureIdentifierPackage>();
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			foreach (FMCollectionItem fM in Manifest.FMs)
			{
				if ((cpuType != fM.CPUType && fM.CPUType != 0) || fM.SkipForPublishing)
				{
					continue;
				}
				FeatureManifest fm = new FeatureManifest();
				string text = Environment.ExpandEnvironmentVariables(fM.Path);
				text = text.ToUpper(CultureInfo.InvariantCulture).Replace(c_FMDirectoryVariable, fmDirectory, StringComparison.OrdinalIgnoreCase);
				FeatureManifest.ValidateAndLoad(ref fm, text, Logger);
				List<FeatureManifest.FMPkgInfo> list = fm.GetAllPackagesByGroups(Manifest.SupportedLanguages, Manifest.SupportedLocales, Manifest.SupportedResolutions, Manifest.GetWowGuestCpuTypes(cpuType), buildType, cpuType.ToString(), msPackageRoot);
				List<PublishingPackageInfo> list2 = new List<PublishingPackageInfo>();
				if (skipMissingPackages)
				{
					List<string> missingPackageFeatures = (from pkg in list
						where !File.Exists(pkg.PackagePath)
						select pkg.FeatureID).ToList();
					list = list.Where((FeatureManifest.FMPkgInfo pkg) => !missingPackageFeatures.Contains(pkg.FeatureID, IgnoreCase)).ToList();
				}
				StringBuilder stringBuilder2 = new StringBuilder();
				bool flag2 = false;
				foreach (FeatureManifest.FMPkgInfo pkgInfo in list)
				{
					if (Manifest.FeatureIdentifierPackages.Where((FeatureIdentifierPackage fip) => fip.FeatureID.Equals(pkgInfo.FeatureID, StringComparison.OrdinalIgnoreCase) && fip.ID.Equals(pkgInfo.ID, StringComparison.OrdinalIgnoreCase) && fip.FixUpAction == FeatureIdentifierPackage.FixUpActions.Ignore).Count() != 0)
					{
						continue;
					}
					PublishingPackageInfo publishingPackageInfo;
					try
					{
						publishingPackageInfo = new PublishingPackageInfo(pkgInfo, fM, msPackageRoot, fM.UserInstallable);
					}
					catch (FileNotFoundException)
					{
						flag2 = true;
						stringBuilder2.AppendFormat("\t{0}\n", pkgInfo.ID);
						continue;
					}
					if (publishingPackageInfo.IsFeatureIdentifierPackage)
					{
						FeatureIdentifierPackage idPkg = new FeatureIdentifierPackage(publishingPackageInfo);
						if (fullList.FeatureIdentifierPackages == null)
						{
							fullList.FeatureIdentifierPackages = new List<FeatureIdentifierPackage>();
						}
						else
						{
							List<FeatureIdentifierPackage> list3 = fullList.FeatureIdentifierPackages.Where((FeatureIdentifierPackage fipPkg) => string.Equals(fipPkg.ID, idPkg.ID, StringComparison.OrdinalIgnoreCase) && string.Equals(fipPkg.Partition, idPkg.Partition, StringComparison.OrdinalIgnoreCase)).ToList();
							if (list3.Count() > 0)
							{
								StringBuilder stringBuilder3 = new StringBuilder();
								stringBuilder3.Append(Environment.NewLine + idPkg.FeatureID + " : ");
								foreach (FeatureIdentifierPackage item in list3)
								{
									stringBuilder3.Append(Environment.NewLine + "\t" + item.ID);
								}
								throw new AmbiguousArgumentException("Some features have more than one FeatureIdentifierPackage defined: " + stringBuilder3.ToString());
							}
						}
						fullList.FeatureIdentifierPackages.Add(idPkg);
					}
					list2.Add(publishingPackageInfo);
				}
				if (flag2)
				{
					flag = true;
					stringBuilder.AppendFormat("\nThe FM File '{0}' following package file(s) could not be found: \n {1}", text, stringBuilder2.ToString());
				}
				fullList.Packages.AddRange(list2);
				if (fm.Features == null)
				{
					continue;
				}
				if (fm.Features.MSFeatureGroups != null)
				{
					foreach (FMFeatureGrouping mSFeatureGroup in fm.Features.MSFeatureGroups)
					{
						mSFeatureGroup.FMID = fM.ID;
						fullList.MSFeatureGroups.Add(mSFeatureGroup);
					}
				}
				if (fm.Features.OEMFeatureGroups == null)
				{
					continue;
				}
				foreach (FMFeatureGrouping oEMFeatureGroup in fm.Features.OEMFeatureGroups)
				{
					oEMFeatureGroup.FMID = fM.ID;
					fullList.OEMFeatureGroups.Add(oEMFeatureGroup);
				}
			}
			if (flag)
			{
				throw new ImageCommonException("ImageCommon!GetPublishingPackageList: Errors processing FM File(s):\n" + stringBuilder.ToString());
			}
			DoFeatureIDFixUps(ref fullList);
			if (!Manifest.IsBuildFeatureEnabled)
			{
				List<FeatureIdentifierPackage> list4 = new List<FeatureIdentifierPackage>();
				foreach (FeatureIdentifierPackage featureIdentifierPackage in fullList.FeatureIdentifierPackages)
				{
					FeatureIdentifierPackage newFip = featureIdentifierPackage;
					if (string.IsNullOrEmpty(newFip.FMID))
					{
						PublishingPackageInfo publishingPackageInfo2 = null;
						publishingPackageInfo2 = fullList.Packages.Find((PublishingPackageInfo pkg) => pkg.ID.Equals(newFip.ID, StringComparison.OrdinalIgnoreCase) && pkg.FeatureID.Equals(newFip.FeatureID, StringComparison.OrdinalIgnoreCase));
						if (publishingPackageInfo2 == null)
						{
							throw new ImageCommonException("ImageCommon!GetPublishingPackageList: Unable to find FeatureIdentifierPackage in Package List: " + featureIdentifierPackage.ID);
						}
						newFip.FMID = publishingPackageInfo2.FMID;
					}
					list4.Add(newFip);
				}
				fullList.FeatureIdentifierPackages = list4;
			}
			ValidateFeatureIdentifers(fullList);
			fullList.ValidateConstraints();
			return fullList;
		}

		private void DoFeatureIDFixUps(ref PublishingPackageList fullList)
		{
			if (fullList.FeatureIdentifierPackages == null || fullList.FeatureIdentifierPackages.Count() == 0)
			{
				return;
			}
			List<FeatureIdentifierPackage> list = fullList.FeatureIdentifierPackages.Where((FeatureIdentifierPackage pkg) => pkg.FixUpAction == FeatureIdentifierPackage.FixUpActions.Ignore || pkg.FixUpAction == FeatureIdentifierPackage.FixUpActions.MoveToAnotherFeature).ToList();
			foreach (FeatureIdentifierPackage fip in list)
			{
				List<PublishingPackageInfo> list2 = ((!string.IsNullOrEmpty(fip.ID)) ? fullList.Packages.Where((PublishingPackageInfo pkg) => (string.Equals(pkg.ID, fip.ID, StringComparison.OrdinalIgnoreCase) || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && pkg.ID.StartsWith(fip.ID + PkgFile.DefaultLanguagePattern, StringComparison.OrdinalIgnoreCase)) || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Resolution && pkg.ID.StartsWith(fip.ID + PkgFile.DefaultResolutionPattern, StringComparison.OrdinalIgnoreCase))) && string.Equals(pkg.Partition, fip.Partition, StringComparison.OrdinalIgnoreCase) && string.Equals(pkg.FeatureID, fip.FeatureID, StringComparison.OrdinalIgnoreCase)).ToList() : fullList.Packages.Where((PublishingPackageInfo pkg) => string.Equals(pkg.FeatureID, fip.FeatureID, StringComparison.OrdinalIgnoreCase)).ToList());
				fullList.Packages = fullList.Packages.Except(list2).ToList();
				if (fip.FixUpAction != FeatureIdentifierPackage.FixUpActions.MoveToAnotherFeature)
				{
					continue;
				}
				foreach (PublishingPackageInfo item in list2)
				{
					fullList.Packages.Remove(item);
					foreach (string item2 in fip.FixUpActionValue.Split(new char[1] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList())
					{
						PublishingPackageInfo publishingPackageInfo = new PublishingPackageInfo(item);
						publishingPackageInfo.FeatureID = item2;
						fullList.Packages.Add(publishingPackageInfo);
					}
				}
			}
			fullList.FeatureIdentifierPackages = fullList.FeatureIdentifierPackages.Except(list).ToList();
			if (!fullList.IsTargetFeatureEnabled)
			{
				fullList.Packages = fullList.Packages.Distinct(PublishingPackageInfoComparer.IgnorePaths).ToList();
			}
		}

		private void ValidateFeatureIdentifers(PublishingPackageList list)
		{
			if (list.FeatureIdentifierPackages == null || !list.FeatureIdentifierPackages.Any())
			{
				return;
			}
			Manifest.ValidateFeatureIdentiferPackages(list.Packages);
			List<string> first = (from pkg in list.Packages
				where pkg.OwnerType == OwnerType.Microsoft
				select pkg.FeatureIDWithFMID).Distinct().ToList();
			List<string> second = (from pkg in list.FeatureIdentifierPackages
				where pkg.ownerType == OwnerType.Microsoft
				select pkg.FeatureIDWithFMID).Distinct().ToList();
			List<string> list2 = first.Except(second).ToList();
			if (list2.Count() != 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				foreach (string item in list2)
				{
					stringBuilder.Append(Environment.NewLine + "\t" + item);
				}
				throw new AmbiguousArgumentException("FeatureAPI!ValidateFeatureIdentifiers: The following features don't have the required FeatureIdentifierPackage defined: " + stringBuilder.ToString());
			}
			list.GetFeatureIDWithFMIDPackages(OwnerType.Invalid);
			List<string> fipPackageIDs = new List<string>(from pkg in list.Packages
				where pkg.IsFeatureIdentifierPackage
				select pkg.ID + "." + pkg.Partition);
			List<PublishingPackageInfo> source = list.Packages.Where((PublishingPackageInfo pkg) => pkg.OwnerType == OwnerType.Microsoft && fipPackageIDs.Contains(pkg.ID + "." + pkg.Partition, IgnoreCase)).ToList();
			StringBuilder stringBuilder2 = new StringBuilder();
			bool flag = false;
			foreach (string packageID in fipPackageIDs)
			{
				List<PublishingPackageInfo> list3 = new List<PublishingPackageInfo>(source.Where((PublishingPackageInfo listPkg) => string.Equals(listPkg.ID + "." + listPkg.Partition, packageID, StringComparison.OrdinalIgnoreCase)));
				if (list3.Count <= 1)
				{
					continue;
				}
				flag = true;
				foreach (PublishingPackageInfo item2 in list3)
				{
					stringBuilder2.AppendLine();
					stringBuilder2.AppendFormat("\t{0} ({1}) {2}", item2.ID, item2.FeatureID, item2.IsFeatureIdentifierPackage ? "(IsFeatureIdentifierPackage)" : "");
				}
				stringBuilder2.AppendLine();
			}
			if (!flag)
			{
				return;
			}
			throw new AmbiguousArgumentException("FeatureAPI!ValidateFeatureIdentifiers: Feature Identifier Packages found in multiple Features: " + stringBuilder2.ToString());
		}
	}
}
