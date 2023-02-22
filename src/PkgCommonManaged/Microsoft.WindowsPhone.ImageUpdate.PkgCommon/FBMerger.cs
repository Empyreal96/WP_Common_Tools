using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	public class FBMerger
	{
		private VersionInfo _finalVersion = VersionInfo.Empty;

		private CpuId _expectedCpu;

		private ReleaseType _expectedRelease;

		private BuildType _expectedBuild;

		private string _ownerOverride;

		private OwnerType _ownerTypeOverride;

		private string _featureKey;

		private Dictionary<string, MergeGroup> _allGroups = new Dictionary<string, MergeGroup>(StringComparer.OrdinalIgnoreCase);

		private static string[] _specialPackages = new string[4] { "Microsoft.BCD.bootlog.winload", "Microsoft.BCD.bootlog.bootmgr", "Microsoft.Net.FakeModem", "Microsoft.Net.FakeWwan" };

		public FBMerger()
		{
		}

		private MergeGroup NewGroup(string partition, bool isFeatureIdentifierGroup = false)
		{
			MergeGroup mergeGroup = new MergeGroup();
			if (_ownerTypeOverride == OwnerType.Microsoft && _expectedRelease == ReleaseType.Production && string.Equals(_featureKey, "BASE", StringComparison.OrdinalIgnoreCase))
			{
				mergeGroup.Component = partition;
				mergeGroup.SubComponent = _expectedRelease.ToString();
			}
			else
			{
				mergeGroup.Component = _featureKey;
				mergeGroup.SubComponent = partition;
			}
			mergeGroup.Owner = _ownerOverride;
			mergeGroup.OwnerType = _ownerTypeOverride;
			mergeGroup.Partition = partition;
			mergeGroup.GroupingKey = _featureKey;
			mergeGroup.ReleaseType = _expectedRelease;
			mergeGroup.BuildType = _expectedBuild;
			mergeGroup.CpuType = _expectedCpu;
			mergeGroup.Version = _finalVersion;
			mergeGroup.IsFeatureIdentifier = isFeatureIdentifierGroup;
			return mergeGroup;
		}

		private MergeGroup FindMergeGroup(IPkgInfo pkgInfo)
		{
			MergeGroup value = null;
			string partition = pkgInfo.Partition;
			if (!_allGroups.TryGetValue(partition, out value))
			{
				value = NewGroup(pkgInfo.Partition);
				_allGroups.Add(partition, value);
			}
			return value;
		}

		public bool IsSpecialPkg(KeyValuePair<string, IPkgInfo> pkg)
		{
			if (pkg.Value.IsBinaryPartition)
			{
				return true;
			}
			if (Path.GetExtension(pkg.Key).Equals(PkgConstants.c_strCBSPackageExtension, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (pkg.Value.OwnerType == OwnerType.Microsoft)
			{
				if ("MobileCore".Equals(pkg.Value.GroupingKey, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if ("MobileCoreMFGOS".Equals(pkg.Value.GroupingKey, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if ("FactoryOS".Equals(pkg.Value.GroupingKey, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if ("Andromeda".Equals(pkg.Value.GroupingKey, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if ("OneCore".Equals(pkg.Value.GroupingKey, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if ("IoTUAP".Equals(pkg.Value.GroupingKey, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
				if (_specialPackages.Contains(pkg.Value.Name, StringComparer.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			if (pkg.Value.OwnerType != OwnerType.Microsoft && "RegistryCustomization".Equals(pkg.Value.SubComponent, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (pkg.Value.OwnerType != OwnerType.Microsoft && pkg.Value.SubComponent.StartsWith("Customizations.", StringComparison.OrdinalIgnoreCase) && pkg.Value.SubComponent.EndsWith("." + pkg.Value.Partition, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (pkg.Value.Partition.Equals(PkgConstants.c_strUpdateOsPartition, StringComparison.OrdinalIgnoreCase))
			{
				if (pkg.Value.OwnerType != OwnerType.Microsoft)
				{
					return true;
				}
				if (!string.IsNullOrEmpty(_ownerOverride) && _ownerTypeOverride != OwnerType.Microsoft)
				{
					return true;
				}
			}
			return false;
		}

		private bool Validate(IEnumerable<KeyValuePair<string, IPkgInfo>> allPkgs)
		{
			bool result = true;
			if (string.IsNullOrEmpty(_ownerOverride))
			{
				IEnumerable<IGrouping<string, KeyValuePair<string, IPkgInfo>>> enumerable = from x in allPkgs
					group x by x.Value.Owner;
				if (enumerable.Count() > 1)
				{
					MergeErrors.Instance.Add("Packages for feature '{0}' having multiple 'Owner' values are not allowed to be merged:", _featureKey);
					foreach (IGrouping<string, KeyValuePair<string, IPkgInfo>> item in enumerable)
					{
						MergeErrors.Instance.Add("\t {0}", item.Key);
						foreach (KeyValuePair<string, IPkgInfo> item2 in item)
						{
							MergeErrors.Instance.Add("\t\t{0}", item2.Key);
						}
					}
					result = false;
				}
				IEnumerable<IGrouping<OwnerType, KeyValuePair<string, IPkgInfo>>> enumerable2 = from x in allPkgs
					group x by x.Value.OwnerType;
				if (enumerable2.Count() > 1)
				{
					MergeErrors.Instance.Add("Packages having multiple 'OwnerType' values are not allowed to be merged:");
					foreach (IGrouping<OwnerType, KeyValuePair<string, IPkgInfo>> item3 in enumerable2)
					{
						MergeErrors.Instance.Add("\t {0}", item3.Key);
						foreach (KeyValuePair<string, IPkgInfo> item4 in item3)
						{
							MergeErrors.Instance.Add("\t\t{0}", item4.Key);
						}
					}
					result = false;
				}
				_ownerOverride = enumerable.First().Key;
				_ownerTypeOverride = enumerable2.First().Key;
			}
			IEnumerable<KeyValuePair<string, IPkgInfo>> enumerable3 = allPkgs.Where((KeyValuePair<string, IPkgInfo> x) => x.Value.CpuType != _expectedCpu);
			if (enumerable3.Count() > 0)
			{
				foreach (KeyValuePair<string, IPkgInfo> item5 in enumerable3)
				{
					MergeErrors.Instance.Add("Unexpected CPU type '{0}' in package '{1}', expecting '{2}'", item5.Value.CpuType, item5.Key, _expectedCpu);
				}
				result = false;
			}
			if (_ownerTypeOverride != OwnerType.OEM || _expectedBuild == BuildType.Retail)
			{
				IEnumerable<KeyValuePair<string, IPkgInfo>> enumerable4 = allPkgs.Where((KeyValuePair<string, IPkgInfo> x) => x.Value.BuildType != _expectedBuild);
				if (enumerable4.Count() > 0)
				{
					foreach (KeyValuePair<string, IPkgInfo> item6 in enumerable4)
					{
						MergeErrors.Instance.Add("Unexpected Build type '{0}' in package '{1}', expecting '{2}'", item6.Value.BuildType, item6.Key, _expectedBuild);
					}
					result = false;
				}
			}
			if (_expectedRelease == ReleaseType.Production)
			{
				IEnumerable<KeyValuePair<string, IPkgInfo>> enumerable5 = allPkgs.Where((KeyValuePair<string, IPkgInfo> x) => x.Value.ReleaseType != _expectedRelease);
				if (enumerable5.Count() > 0)
				{
					foreach (KeyValuePair<string, IPkgInfo> item7 in enumerable5)
					{
						MergeErrors.Instance.Add("Unexpected release type '{0}' in package '{1}', expecting '{2}'", item7.Value.ReleaseType, item7.Key, _expectedRelease);
					}
					result = false;
				}
			}
			return result;
		}

		private FBMerger(string featureKey, VersionInfo outputVersion, string ownerOverride, OwnerType ownerTypeOverride, ReleaseType expectedRelease, CpuId expectedCpu, BuildType expectedBuild)
		{
			_finalVersion = outputVersion;
			_expectedRelease = expectedRelease;
			_expectedBuild = expectedBuild;
			_expectedCpu = expectedCpu;
			_ownerOverride = ownerOverride;
			_ownerTypeOverride = ownerTypeOverride;
			_featureKey = featureKey;
		}

		private MergeResult[] ProcessExcludedPackages(IEnumerable<KeyValuePair<string, IPkgInfo>> pkgs, string outputDir)
		{
			List<MergeResult> list = new List<MergeResult>();
			StringBuilder stringBuilder = new StringBuilder();
			foreach (KeyValuePair<string, IPkgInfo> pkg in pkgs)
			{
				MergeResult mergeResult = new MergeResult();
				mergeResult.FilePath = pkg.Key;
				list.Add(mergeResult);
				LogUtil.Message("Skipping special package '{0}'", pkg.Key);
				stringBuilder.AppendFormat("{0},{1}", pkg.Key, pkg.Value.Version);
				stringBuilder.AppendLine();
			}
			if (list.Count > 0)
			{
				File.WriteAllText(Path.Combine(outputDir, $"unmerged_{_featureKey}.txt"), stringBuilder.ToString());
			}
			return list.ToArray();
		}

		private MergeResult[] Merge(IEnumerable<string> inputPkgs, string outputDir, bool compress, bool incremental)
		{
			LogUtil.Message("Merging packages for feature '{0}'", _featureKey);
			List<MergeResult> list = new List<MergeResult>();
			if (inputPkgs.Count() > 0)
			{
				string[] array = (from x in inputPkgs
					group x by x into x
					where x.Count() > 1
					select x.Key).ToArray();
				if (array.Length != 0)
				{
					throw new PackageException("Duplicated packages detected for feature {0}:\n\t{1}", _featureKey, string.Join("\n\t", array));
				}
				Dictionary<string, IPkgInfo> dictionary = inputPkgs.ToDictionary((string x) => x, (string x) => Package.LoadFromCab(x), StringComparer.OrdinalIgnoreCase);
				IEnumerable<KeyValuePair<string, IPkgInfo>> enumerable = dictionary.Where((KeyValuePair<string, IPkgInfo> x) => IsSpecialPkg(x));
				IEnumerable<KeyValuePair<string, IPkgInfo>> enumerable2 = dictionary.Except(enumerable);
				if (!Validate(enumerable2))
				{
					MergeErrors.Instance.Add("Some packages failed to pass global validation");
				}
				list.AddRange(ProcessExcludedPackages(enumerable, outputDir));
				MergeGroup value = NewGroup(PkgConstants.c_strMainOsPartition, true);
				_allGroups.Add(PkgConstants.c_strMainOsPartition, value);
				foreach (KeyValuePair<string, IPkgInfo> item in enumerable2)
				{
					FindMergeGroup(item.Value).AddPkg(item);
				}
				foreach (MergeGroup value2 in _allGroups.Values)
				{
					list.Add(value2.Merge(outputDir, compress, incremental));
				}
				MergeErrors instance = MergeErrors.Instance;
				MergeErrors.Clear();
				instance.CheckResult();
				foreach (MergeResult item2 in list)
				{
					item2.PkgInfo = Package.LoadFromCab(item2.FilePath);
				}
			}
			LogUtil.Message("Done.");
			return list.ToArray();
		}

		public static MergeResult[] Merge(IEnumerable<string> inputPkgs, string featureKey, VersionInfo outputVersion, string ownerOverride, OwnerType ownerTypeOverride, ReleaseType expectedReleaseType, CpuId expectedCpuType, BuildType expectedBuildType, string outputDir, bool compress, bool incremental)
		{
			return new FBMerger(featureKey, outputVersion, ownerOverride, ownerTypeOverride, expectedReleaseType, expectedCpuType, expectedBuildType).Merge(inputPkgs, outputDir, compress, incremental);
		}
	}
}
