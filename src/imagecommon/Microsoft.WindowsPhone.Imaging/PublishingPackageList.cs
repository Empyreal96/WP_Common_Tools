using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.FeatureAPI;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Imaging
{
	[XmlType(Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate")]
	[XmlRoot(ElementName = "PublishingPackageInfo", Namespace = "http://schemas.microsoft.com/embedded/2004/10/ImageUpdate", IsNullable = false)]
	public class PublishingPackageList
	{
		private const string DiffPackageExtension = ".spku.cab";

		private const string CanonicalPackageExtension = ".spkg.cab";

		private const string RemovePackageExtension = ".spkr.cab";

		[DefaultValue(false)]
		public bool IsUpdateList;

		[DefaultValue(false)]
		public bool IsTargetFeatureEnabled;

		[XmlArrayItem(ElementName = "FeatureGroup", Type = typeof(FMFeatureGrouping), IsNullable = false)]
		[XmlArray]
		public List<FMFeatureGrouping> MSFeatureGroups;

		[XmlArrayItem(ElementName = "FeatureGroup", Type = typeof(FMFeatureGrouping), IsNullable = false)]
		[XmlArray]
		public List<FMFeatureGrouping> OEMFeatureGroups;

		[XmlArrayItem(ElementName = "PackageInfo", Type = typeof(PublishingPackageInfo), IsNullable = false)]
		[XmlArray]
		public List<PublishingPackageInfo> Packages;

		[XmlArrayItem(ElementName = "FeatureIdentifierPackage", Type = typeof(FeatureIdentifierPackage), IsNullable = false)]
		[XmlArray]
		public List<FeatureIdentifierPackage> FeatureIdentifierPackages;

		[XmlIgnore]
		private StringComparer IgnoreCase = StringComparer.OrdinalIgnoreCase;

		public PublishingPackageList()
		{
			if (Packages == null)
			{
				Packages = new List<PublishingPackageInfo>();
			}
		}

		public PublishingPackageList(string sourceListPath, string destListPath, IULogger logger)
		{
			PublishingPackageList publishingPackageList = ValidateAndLoad(sourceListPath, logger);
			PublishingPackageList publishingPackageList2 = ValidateAndLoad(destListPath, logger);
			List<PublishingPackageInfo> packages = publishingPackageList.Packages;
			List<PublishingPackageInfo> packages2 = publishingPackageList2.Packages;
			IsUpdateList = true;
			FeatureIdentifierPackages = publishingPackageList.FeatureIdentifierPackages;
			IsTargetFeatureEnabled = publishingPackageList.IsTargetFeatureEnabled;
			Packages = new List<PublishingPackageInfo>();
			IEnumerable<PublishingPackageInfo> source = from srcPkg in packages
				join destPkg in packages2 on srcPkg.ID equals destPkg.ID
				where !destPkg.Path.Equals(srcPkg.Path, StringComparison.OrdinalIgnoreCase) && destPkg.Equals(srcPkg, PublishingPackageInfo.PublishingPackageInfoComparison.IgnorePaths)
				select destPkg.SetPreviousPath(srcPkg.Path);
			source = source.Select((PublishingPackageInfo pkg) => pkg.SetUpdateType(PublishingPackageInfo.UpdateTypes.Diff)).ToList();
			Packages.AddRange(source);
			packages = packages.Except(source, PublishingPackageInfoComparer.IgnorePaths).ToList();
			packages2 = packages2.Except(source, PublishingPackageInfoComparer.IgnorePaths).ToList();
			Packages.AddRange(from pkg in packages2.Intersect(packages, PublishingPackageInfoComparer.IgnorePaths)
				select pkg.SetUpdateType(PublishingPackageInfo.UpdateTypes.Diff));
			Packages.AddRange(from pkg in packages2.Except(packages, PublishingPackageInfoComparer.IgnorePaths)
				select pkg.SetUpdateType(PublishingPackageInfo.UpdateTypes.Canonical));
			Packages.AddRange(from pkg in packages.Except(packages2, PublishingPackageInfoComparer.IgnorePaths)
				select pkg.SetUpdateType(PublishingPackageInfo.UpdateTypes.PKR));
			if (publishingPackageList.IsTargetFeatureEnabled || publishingPackageList.MSFeatureGroups.Count() > 0 || publishingPackageList.OEMFeatureGroups.Count() > 0)
			{
				MSFeatureGroups = publishingPackageList.MSFeatureGroups;
				OEMFeatureGroups = publishingPackageList.OEMFeatureGroups;
			}
			else
			{
				MSFeatureGroups = publishingPackageList2.MSFeatureGroups;
				OEMFeatureGroups = publishingPackageList2.OEMFeatureGroups;
			}
		}

		public Dictionary<string, string> GetFeatureIDWithFMIDPackages(OwnerType forOwner)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (string featureIDWithFMID in GetFeatureIDWithFMIDs(forOwner))
			{
				List<PublishingPackageInfo> list = (from pkg in GetAllPackagesForFeatureIDWithFMID(featureIDWithFMID, forOwner)
					where pkg.IsFeatureIdentifierPackage
					select pkg).ToList();
				if (list.Count() > 1)
				{
					flag = true;
					stringBuilder.Append(Environment.NewLine + featureIDWithFMID + " : ");
					foreach (PublishingPackageInfo item in list)
					{
						stringBuilder.Append(Environment.NewLine + "\t" + item.ID);
					}
				}
				else
				{
					string value = "";
					if (list.Count() == 1)
					{
						value = list.ElementAt(0).ID;
					}
					dictionary.Add(featureIDWithFMID, value);
				}
			}
			if (flag)
			{
				throw new AmbiguousArgumentException("Some features have more than one FeatureIdentifierPackage defined: " + stringBuilder.ToString());
			}
			return dictionary;
		}

		public List<string> GetFeatureIDWithFMIDs(OwnerType forOwner)
		{
			IEnumerable<PublishingPackageInfo> source = Packages.Where((PublishingPackageInfo pkg) => pkg.OwnerType == OwnerType.Microsoft || pkg.OwnerType == OwnerType.OEM);
			if (forOwner != 0)
			{
				source = Packages.Where((PublishingPackageInfo pkg) => pkg.OwnerType == forOwner);
			}
			return source.Select((PublishingPackageInfo pkg) => pkg.FeatureIDWithFMID).Distinct(IgnoreCase).ToList();
		}

		public List<PublishingPackageInfo> GetAllPackagesForFeature(string FeatureID, OwnerType forOwner)
		{
			List<string> list = new List<string>();
			list.Add(FeatureID);
			return GetAllPackagesForFeatures(list, forOwner);
		}

		public List<PublishingPackageInfo> GetAllPackagesForFeatureIDWithFMID(string FeatureIDWithFMID, OwnerType forOwner)
		{
			List<PublishingPackageInfo> list = Packages.Where((PublishingPackageInfo pkg) => FeatureIDWithFMID.Equals(pkg.FeatureIDWithFMID, StringComparison.OrdinalIgnoreCase)).ToList();
			if (forOwner != 0)
			{
				list = list.Where((PublishingPackageInfo pkg) => pkg.OwnerType == forOwner).ToList();
			}
			return list;
		}

		private List<PublishingPackageInfo> GetAllPackagesForFeaturesAndFMs(List<string> FeatureIDs, List<string> fmFilter, OwnerType forOwner = OwnerType.Invalid)
		{
			List<PublishingPackageInfo> allPackagesForFeatures = GetAllPackagesForFeatures(FeatureIDs, forOwner);
			List<string> newFMFilter = fmFilter.Select((string fm) => fm.ToUpper(CultureInfo.InvariantCulture).Replace("SKU", "FM", StringComparison.OrdinalIgnoreCase)).ToList();
			List<string> newSKUFilter = fmFilter.Select((string fm) => fm.ToUpper(CultureInfo.InvariantCulture).Replace("FM", "SKU", StringComparison.OrdinalIgnoreCase)).ToList();
			return allPackagesForFeatures.Where((PublishingPackageInfo pkg) => newFMFilter.Contains(pkg.SourceFMFile, IgnoreCase) || newSKUFilter.Contains(pkg.SourceFMFile, IgnoreCase)).ToList();
		}

		public List<PublishingPackageInfo> GetAllPackagesForFeatures(List<string> FeatureIDs, OwnerType forOwner)
		{
			IEnumerable<PublishingPackageInfo> source = Packages.Where((PublishingPackageInfo pkg) => pkg.OwnerType == forOwner && FeatureIDs.Contains(pkg.FeatureID, IgnoreCase));
			if (forOwner == OwnerType.Invalid)
			{
				source = Packages.Where((PublishingPackageInfo pkg) => FeatureIDs.Contains(pkg.FeatureID, IgnoreCase));
			}
			return source.ToList();
		}

		public List<PublishingPackageInfo> GetUpdatePackageList(OEMInput orgOemInput, OEMInput newOemInput, OEMInput.OEMFeatureTypes featureFilter)
		{
			return GetUpdatePackageList(orgOemInput, newOemInput, featureFilter, OwnerType.Invalid);
		}

		public List<PublishingPackageInfo> GetUpdatePackageList(OEMInput orgOemInput, OEMInput newOemInput, OEMInput.OEMFeatureTypes featureFilter, OwnerType forOwner)
		{
			List<string> featureList = orgOemInput.GetFeatureList(featureFilter);
			List<string> featureList2 = newOemInput.GetFeatureList(featureFilter);
			List<string> fMs = orgOemInput.GetFMs();
			List<string> fMs2 = newOemInput.GetFMs();
			return GetUpdatePackageList(featureList, featureList2, orgOemInput.SupportedLanguages.UserInterface, newOemInput.SupportedLanguages.UserInterface, newOemInput.Resolutions, fMs, fMs2, forOwner);
		}

		private List<PublishingPackageInfo> GetUpdatePackageList(List<string> orgFeatures, List<string> newFeatures, List<string> orgLangs, List<string> newLangs, List<string> resolutions, List<string> orgFMs, List<string> newFMs, OwnerType forOwner = OwnerType.Invalid, bool DoOnlyChanges = false)
		{
			List<PublishingPackageInfo> list = new List<PublishingPackageInfo>();
			if (forOwner != OwnerType.Microsoft && forOwner != OwnerType.OEM && forOwner != 0)
			{
				return list;
			}
			List<string> addFeatures = newFeatures.Except(orgFeatures, IgnoreCase).ToList();
			List<string> removeFeatures = orgFeatures.Except(newFeatures, IgnoreCase).ToList();
			List<string> commonFeatures = newFeatures.Intersect(orgFeatures, IgnoreCase).ToList();
			List<string> addLangs = newLangs.Except(orgLangs, IgnoreCase).ToList();
			List<string> removeLangs = orgLangs.Except(newLangs, IgnoreCase).ToList();
			List<string> commonLangs = newLangs.Intersect(orgLangs, IgnoreCase).ToList();
			List<string> orgFMsNormalized = orgFMs.Select((string fm) => NormalizeFM(fm)).ToList();
			List<string> addFMs = newFMs.Except(orgFMsNormalized, IgnoreCase).ToList();
			List<string> removeFMs = orgFMsNormalized.Except(newFMs, IgnoreCase).ToList();
			List<string> commonFMs = newFMs.Intersect(orgFMsNormalized, IgnoreCase).ToList();
			List<PublishingPackageInfo> list2 = Packages.Where((PublishingPackageInfo pkg) => addFeatures.Contains(pkg.FeatureID, IgnoreCase) && pkg.UpdateType != 0 && newFMs.Contains(NormalizeFM(pkg.SourceFMFile), IgnoreCase) && (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Base || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Resolution && resolutions.Contains(pkg.SatelliteValue, IgnoreCase)) || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && newLangs.Contains(pkg.SatelliteValue, IgnoreCase)))).ToList();
			List<PublishingPackageInfo> list3 = Packages.Where((PublishingPackageInfo pkg) => removeFeatures.Contains(pkg.FeatureID, IgnoreCase) && orgFMsNormalized.Contains(NormalizeFM(pkg.SourceFMFile), IgnoreCase) && (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Base || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Resolution && resolutions.Contains(pkg.SatelliteValue, IgnoreCase)) || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && orgLangs.Contains(pkg.SatelliteValue, IgnoreCase)))).ToList();
			list2.AddRange(Packages.Where((PublishingPackageInfo pkg) => commonFeatures.Contains(pkg.FeatureID, IgnoreCase) && addFMs.Contains(NormalizeFM(pkg.SourceFMFile), IgnoreCase) && pkg.UpdateType != 0 && (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Base || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Resolution && resolutions.Contains(pkg.SatelliteValue, IgnoreCase)) || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && newLangs.Contains(pkg.SatelliteValue, IgnoreCase)))).ToList());
			list3.AddRange(Packages.Where((PublishingPackageInfo pkg) => commonFeatures.Contains(pkg.FeatureID, IgnoreCase) && removeFMs.Contains(NormalizeFM(pkg.SourceFMFile), IgnoreCase) && (!IsUpdateList || pkg.UpdateType != PublishingPackageInfo.UpdateTypes.Canonical) && (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Base || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Resolution && resolutions.Contains(pkg.SatelliteValue, IgnoreCase)) || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && orgLangs.Contains(pkg.SatelliteValue, IgnoreCase)))).ToList());
			list2.AddRange(Packages.Where((PublishingPackageInfo pkg) => commonFeatures.Contains(pkg.FeatureID, IgnoreCase) && newFMs.Contains(NormalizeFM(pkg.SourceFMFile), IgnoreCase) && pkg.UpdateType != 0 && pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && addLangs.Contains(pkg.SatelliteValue, IgnoreCase)).ToList());
			list3.AddRange(Packages.Where((PublishingPackageInfo pkg) => commonFeatures.Contains(pkg.FeatureID, IgnoreCase) && orgFMsNormalized.Contains(NormalizeFM(pkg.SourceFMFile), IgnoreCase) && pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && removeLangs.Contains(pkg.SatelliteValue, IgnoreCase)).ToList());
			if (IsUpdateList && !DoOnlyChanges)
			{
				list.AddRange(Packages.Where((PublishingPackageInfo pkg) => commonFeatures.Contains(pkg.FeatureID, IgnoreCase) && commonFMs.Contains(NormalizeFM(pkg.SourceFMFile), IgnoreCase) && (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Base || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Resolution && resolutions.Contains(pkg.SatelliteValue, IgnoreCase)) || (pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && commonLangs.Contains(pkg.SatelliteValue, IgnoreCase)))).ToList());
				list3 = list3.Except(list3.Where((PublishingPackageInfo pkg) => pkg.UpdateType == PublishingPackageInfo.UpdateTypes.Canonical && pkg.OwnerType != OwnerType.Microsoft)).ToList();
			}
			list2 = ChangeToCanonicals(list2);
			list3 = ChangeToPKRs(list3);
			list.AddRange(list2);
			list.AddRange(list3);
			if (forOwner != 0)
			{
				list = list.Where((PublishingPackageInfo pkg) => pkg.OwnerType == forOwner).ToList();
			}
			return RemoveDuplicatesPkgs(list);
		}

		private string NormalizeFM(string fm)
		{
			return fm.ToUpper(CultureInfo.InvariantCulture).Replace("SKU", "FM", StringComparison.OrdinalIgnoreCase);
		}

		public UpdateOSInput GetUpdateInput(string build1OEMInput, string build2OEMInput, string msPackageRoot, CpuId cpuType, IULogger logger)
		{
			UpdateOSInput updateOSInput = new UpdateOSInput();
			List<string> list = new List<string>();
			char[] trimChars = new char[1] { '\\' };
			msPackageRoot = msPackageRoot.Trim(trimChars);
			OEMInput xmlInput = new OEMInput();
			OEMInput xmlInput2 = new OEMInput();
			OEMInput.ValidateInput(ref xmlInput, build1OEMInput, logger, msPackageRoot, cpuType.ToString());
			OEMInput.ValidateInput(ref xmlInput2, build2OEMInput, logger, msPackageRoot, cpuType.ToString());
			list = (from pkg in GetUpdatePackageList(xmlInput, xmlInput2, (OEMInput.OEMFeatureTypes)268435455)
				select pkg.Path).ToList();
			list = list.Distinct(IgnoreCase).ToList();
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = Path.Combine(msPackageRoot, list[i]);
			}
			updateOSInput.PackageFiles = list;
			updateOSInput.Description = "(Updating to)" + xmlInput2.Description;
			return updateOSInput;
		}

		public List<PublishingPackageInfo> GetPackageListForPOP(OEMInput oemInput1, OEMInput oemInput2)
		{
			List<PublishingPackageInfo> list = new List<PublishingPackageInfo>();
			list.AddRange(GetMSPackageListForPOP(oemInput1, oemInput2));
			list.AddRange(GetOEMPackageListForPOP(oemInput1, oemInput2));
			return list;
		}

		private List<string> GetDepricatedFeatures(List<string> featureIDs)
		{
			List<string> list = new List<string>();
			if (!IsUpdateList)
			{
				return list;
			}
			foreach (string featureID in featureIDs)
			{
				if ((from pkg in GetAllPackagesForFeature(featureID, OwnerType.Invalid)
					where pkg.UpdateType != PublishingPackageInfo.UpdateTypes.PKR
					select pkg).Count() == 0)
				{
					list.Add(featureID);
				}
			}
			return list;
		}

		private List<string> GetDepricatedLangs(List<string> Langs)
		{
			List<string> list = new List<string>();
			if (!IsUpdateList)
			{
				return list;
			}
			foreach (string lang in Langs)
			{
				if (Packages.Where((PublishingPackageInfo pkg) => pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && pkg.SatelliteValue.Equals(lang, StringComparison.OrdinalIgnoreCase) && pkg.UpdateType != PublishingPackageInfo.UpdateTypes.PKR).Count() == 0)
				{
					list.Add(lang);
				}
			}
			return list;
		}

		public List<PublishingPackageInfo> GetMSPackageListForPOP(OEMInput orgOemInput, OEMInput newOemInput)
		{
			List<string> featureList = orgOemInput.GetFeatureList((OEMInput.OEMFeatureTypes)268433407);
			List<string> featureList2 = newOemInput.GetFeatureList((OEMInput.OEMFeatureTypes)268433407);
			List<string> fMs = orgOemInput.GetFMs();
			List<string> fMs2 = newOemInput.GetFMs();
			List<string> userInterface = orgOemInput.SupportedLanguages.UserInterface;
			List<string> userInterface2 = newOemInput.SupportedLanguages.UserInterface;
			featureList = featureList.Except(GetDepricatedFeatures(featureList)).ToList();
			userInterface = userInterface.Except(GetDepricatedLangs(userInterface)).ToList();
			bool doOnlyChanges = true;
			List<PublishingPackageInfo> list = GetUpdatePackageList(featureList, featureList2, userInterface, userInterface2, newOemInput.Resolutions, fMs, fMs2, OwnerType.Microsoft, doOnlyChanges);
			if (IsUpdateList)
			{
				list = list.Except(GetSourceOnlyPkgs(list.Where((PublishingPackageInfo pkg) => pkg.UpdateType == PublishingPackageInfo.UpdateTypes.PKR).ToList())).ToList();
			}
			return list;
		}

		private List<PublishingPackageInfo> GetSourceOnlyPkgs(List<PublishingPackageInfo> list)
		{
			List<string> second = (from pkg in Packages
				where pkg.UpdateType == PublishingPackageInfo.UpdateTypes.Diff || pkg.UpdateType == PublishingPackageInfo.UpdateTypes.Canonical
				select pkg.ID).Distinct().ToList();
			List<string> first = Packages.Select((PublishingPackageInfo pkg) => pkg.ID).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			List<string> inSourceOnlyPackageIDs = first.Except(second, IgnoreCase).Distinct(IgnoreCase).ToList();
			return list.Where((PublishingPackageInfo pkg) => inSourceOnlyPackageIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
		}

		private List<PublishingPackageInfo> RemoveDuplicatesPkgs(List<PublishingPackageInfo> pkgList)
		{
			List<PublishingPackageInfo> list = new List<PublishingPackageInfo>();
			List<string> excludePackages = new List<string>();
			List<PublishingPackageInfo> source = pkgList.Where((PublishingPackageInfo pkg) => pkg.UpdateType == PublishingPackageInfo.UpdateTypes.PKR).ToList();
			List<PublishingPackageInfo> source2 = pkgList.Where((PublishingPackageInfo pkg) => pkg.UpdateType == PublishingPackageInfo.UpdateTypes.Canonical).ToList();
			List<PublishingPackageInfo> collection = pkgList.Where((PublishingPackageInfo pkg) => pkg.UpdateType == PublishingPackageInfo.UpdateTypes.Diff).ToList();
			excludePackages = source.Select((PublishingPackageInfo pkgID) => pkgID.ID).Intersect(source2.Select((PublishingPackageInfo dupPkg) => dupPkg.ID), IgnoreCase).ToList();
			list.AddRange(source.Where((PublishingPackageInfo pkg) => !excludePackages.Contains(pkg.ID, IgnoreCase)).ToList());
			list.AddRange(source2.Where((PublishingPackageInfo pkg) => !excludePackages.Contains(pkg.ID, IgnoreCase)).ToList());
			list.AddRange(collection);
			return list.Distinct().ToList();
		}

		private List<PublishingPackageInfo> ChangeToCanonicals(List<PublishingPackageInfo> pkgs)
		{
			List<PublishingPackageInfo> list = new List<PublishingPackageInfo>();
			foreach (PublishingPackageInfo pkg in pkgs)
			{
				PublishingPackageInfo item = ChangeToCanonical(pkg);
				list.Add(item);
			}
			return list.Distinct().ToList();
		}

		private PublishingPackageInfo ChangeToCanonical(PublishingPackageInfo pkg)
		{
			PublishingPackageInfo publishingPackageInfo = new PublishingPackageInfo(pkg);
			if (publishingPackageInfo.UpdateType != PublishingPackageInfo.UpdateTypes.Canonical)
			{
				publishingPackageInfo.UpdateType = PublishingPackageInfo.UpdateTypes.Canonical;
			}
			return publishingPackageInfo;
		}

		private List<PublishingPackageInfo> ChangeToPKRs(List<PublishingPackageInfo> pkgs)
		{
			List<PublishingPackageInfo> list = new List<PublishingPackageInfo>();
			foreach (PublishingPackageInfo pkg in pkgs)
			{
				PublishingPackageInfo item = ChangeToPKR(pkg);
				list.Add(item);
			}
			return list.Distinct().ToList();
		}

		private PublishingPackageInfo ChangeToPKR(PublishingPackageInfo pkg)
		{
			PublishingPackageInfo publishingPackageInfo = new PublishingPackageInfo(pkg);
			if (publishingPackageInfo.UpdateType != 0)
			{
				string originalString = publishingPackageInfo.Path.ToLower(CultureInfo.InvariantCulture);
				originalString = originalString.Replace(PkgConstants.c_strPackageExtension, PkgConstants.c_strRemovalPkgExtension, StringComparison.OrdinalIgnoreCase);
				originalString = originalString.Replace(PkgConstants.c_strDiffPackageExtension, PkgConstants.c_strRemovalPkgExtension, StringComparison.OrdinalIgnoreCase);
				originalString = originalString.Replace(".spkg.cab", ".spkr.cab", StringComparison.OrdinalIgnoreCase);
				originalString = originalString.ToLower(CultureInfo.InvariantCulture).Replace(".spku.cab", ".spkr.cab", StringComparison.OrdinalIgnoreCase);
				publishingPackageInfo.Path = originalString;
				publishingPackageInfo.UpdateType = PublishingPackageInfo.UpdateTypes.PKR;
			}
			return publishingPackageInfo;
		}

		public List<PublishingPackageInfo> GetOEMPackageListForPOP(OEMInput oemInput1, OEMInput oemInput2)
		{
			List<PublishingPackageInfo> list = GetUpdatePackageList(oemInput1, oemInput2, (OEMInput.OEMFeatureTypes)268434431, OwnerType.OEM);
			if (IsUpdateList)
			{
				list = list.Except(GetTargetOnlyPkgs(list.Where((PublishingPackageInfo pkg) => pkg.UpdateType == PublishingPackageInfo.UpdateTypes.PKR).ToList())).ToList();
			}
			return list;
		}

		private List<PublishingPackageInfo> GetTargetOnlyPkgs(List<PublishingPackageInfo> list)
		{
			List<string> sourcePackageIDs = (from pkg in Packages
				where pkg.UpdateType == PublishingPackageInfo.UpdateTypes.Diff || pkg.UpdateType == PublishingPackageInfo.UpdateTypes.PKR
				select pkg.ID).Distinct().ToList();
			return list.Where((PublishingPackageInfo pkg) => !sourcePackageIDs.Contains(pkg.ID, StringComparer.OrdinalIgnoreCase)).ToList();
		}

		public List<PublishingPackageInfo> GetAllPackagesForImage(OEMInput oemInput)
		{
			return GetAllPackagesForImage(oemInput, OwnerType.Invalid);
		}

		public List<PublishingPackageInfo> GetAllPackagesForImage(OEMInput oemInput, OwnerType forOwnerType)
		{
			List<PublishingPackageInfo> list = new List<PublishingPackageInfo>();
			OEMInput.OEMFeatureTypes forFeatures = (OEMInput.OEMFeatureTypes)268435455;
			switch (forOwnerType)
			{
			case OwnerType.Microsoft:
				forFeatures = (OEMInput.OEMFeatureTypes)268433407;
				break;
			case OwnerType.OEM:
				forFeatures = (OEMInput.OEMFeatureTypes)268434431;
				break;
			default:
				return list;
			case OwnerType.Invalid:
				break;
			}
			List<string> featureList = oemInput.GetFeatureList(forFeatures);
			List<string> fMs = oemInput.GetFMs();
			List<string> langs = oemInput.SupportedLanguages.UserInterface;
			List<string> res = oemInput.Resolutions;
			List<PublishingPackageInfo> allPackagesForFeaturesAndFMs = GetAllPackagesForFeaturesAndFMs(featureList, fMs, forOwnerType);
			list.AddRange(allPackagesForFeaturesAndFMs.Where((PublishingPackageInfo pkg) => pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Base).ToList());
			list.AddRange(allPackagesForFeaturesAndFMs.Where((PublishingPackageInfo pkg) => pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Language && langs.Contains(pkg.SatelliteValue, IgnoreCase)).ToList());
			list.AddRange(allPackagesForFeaturesAndFMs.Where((PublishingPackageInfo pkg) => pkg.SatelliteType == PublishingPackageInfo.SatelliteTypes.Resolution && res.Contains(pkg.SatelliteValue, IgnoreCase)).ToList());
			return list.Distinct().ToList();
		}

		public void ValidateConstraints()
		{
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (IGrouping<string, PublishingPackageInfo> item in from pkg in Packages
				where pkg.OwnerType == OwnerType.Microsoft
				group pkg by pkg.ID)
			{
				if (item.Count() <= 1)
				{
					continue;
				}
				IEnumerable<string> enumerable = item.Select((PublishingPackageInfo pkg) => pkg.FeatureID).Distinct(IgnoreCase);
				if (item.Count() != enumerable.Count())
				{
					foreach (IGrouping<string, PublishingPackageInfo> item2 in from pkg in item
						group pkg by pkg.FeatureID into gp
						where gp.Count() > 1
						select gp)
					{
						stringBuilder.AppendLine("\t" + item2.First().FeatureID + ": (" + item2.First().ID + " Count=" + item2.Count() + ")\n");
					}
				}
				if (!IsTargetFeatureEnabled)
				{
					continue;
				}
				List<List<string>> list = (from fGroup in OEMFeatureGroups
					where fGroup.Constraint == FMFeatureGrouping.FeatureConstraints.OneAndOnlyOne || fGroup.Constraint == FMFeatureGrouping.FeatureConstraints.ZeroOrOne
					select fGroup.AllFeatureIDs).ToList();
				list.AddRange(from fGroup in MSFeatureGroups
					where fGroup.Constraint == FMFeatureGrouping.FeatureConstraints.OneAndOnlyOne || fGroup.Constraint == FMFeatureGrouping.FeatureConstraints.ZeroOrOne
					select fGroup.AllFeatureIDs);
				list.AddRange(GetImplicitConstraints());
				bool flag = false;
				foreach (List<string> item3 in list)
				{
					if (enumerable.Except(item3).Count() == 0)
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					stringBuilder2.AppendLine("\t" + item.First().ID + ": (" + string.Join(", ", enumerable.ToArray()) + ")\n");
				}
			}
			StringBuilder stringBuilder3 = new StringBuilder();
			if (stringBuilder.Length != 0)
			{
				stringBuilder3.AppendLine("The following Features have packages listed more than once:\n" + stringBuilder.ToString());
			}
			if (stringBuilder2.Length != 0)
			{
				stringBuilder3.AppendLine("The following package is included in multiple features without constraints preventing them from being included in the same image:\n" + stringBuilder2.ToString());
			}
			if (stringBuilder3.Length != 0)
			{
				throw new ImageCommonException(stringBuilder3.ToString());
			}
		}

		public List<List<string>> GetImplicitConstraints()
		{
			List<List<string>> list = new List<List<string>>();
			foreach (FeatureManifest.PackageGroups fmGroup in from pkg in Packages
				where pkg.FMGroup != FeatureManifest.PackageGroups.MSFEATURE && pkg.FMGroup != FeatureManifest.PackageGroups.OEMFEATURE && pkg.FMGroup != FeatureManifest.PackageGroups.BASE
				select pkg.FMGroup)
			{
				list.Add((from pkg in Packages
					where pkg.FMGroup == fmGroup
					select pkg.FeatureID.ToString()).Distinct(IgnoreCase).ToList());
			}
			return list;
		}

		public static PublishingPackageList ValidateAndLoad(string xmlFile, IULogger logger)
		{
			PublishingPackageList publishingPackageList = new PublishingPackageList();
			string text = string.Empty;
			string publishingPackageInfoSchema = BuildPaths.PublishingPackageInfoSchema;
			Assembly executingAssembly = Assembly.GetExecutingAssembly();
			string[] manifestResourceNames = executingAssembly.GetManifestResourceNames();
			foreach (string text2 in manifestResourceNames)
			{
				if (text2.Contains(publishingPackageInfoSchema))
				{
					text = text2;
					break;
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				throw new ImageCommonException("ImageCommon!ValidateAndLoad: XSD resource was not found: " + publishingPackageInfoSchema);
			}
			using (Stream xsdStream = executingAssembly.GetManifestResourceStream(text))
			{
				XsdValidator xsdValidator = new XsdValidator();
				try
				{
					xsdValidator.ValidateXsd(xsdStream, xmlFile, logger);
				}
				catch (XsdValidatorException innerException)
				{
					throw new ImageCommonException("ImageCommon!ValidateAndLoad: Unable to validate Publishing Package Info XSD for file '" + xmlFile + "'.", innerException);
				}
			}
			logger.LogInfo("ImageCommon: Successfully validated the Publishing Package Info XML: {0}", xmlFile);
			TextReader textReader = new StreamReader(LongPathFile.OpenRead(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PublishingPackageList));
			try
			{
				return (PublishingPackageList)xmlSerializer.Deserialize(textReader);
			}
			catch (Exception innerException2)
			{
				throw new ImageCommonException("ImageCommon!ValidateAndLoad: Unable to parse Publishing Package Info XML file '" + xmlFile + "'", innerException2);
			}
			finally
			{
				textReader.Close();
			}
		}

		public void WriteToFile(string xmlFile)
		{
			TextWriter textWriter = new StreamWriter(LongPathFile.OpenWrite(xmlFile));
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(PublishingPackageList));
			try
			{
				xmlSerializer.Serialize(textWriter, this);
			}
			catch (Exception innerException)
			{
				throw new ImageCommonException("ImageCommon!WriteToFile: Unable to write Publishing Package List XML file '" + xmlFile + "'", innerException);
			}
			finally
			{
				textWriter.Close();
			}
		}
	}
}
