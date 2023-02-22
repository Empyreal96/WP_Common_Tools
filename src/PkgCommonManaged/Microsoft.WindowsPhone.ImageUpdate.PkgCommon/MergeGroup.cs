using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class MergeGroup
	{
		private List<string> _basePkgs = new List<string>();

		private Dictionary<string, List<string>> _langPkgs = new Dictionary<string, List<string>>();

		private Dictionary<string, List<string>> _resPkgs = new Dictionary<string, List<string>>();

		public string Owner { get; set; }

		public OwnerType OwnerType { get; set; }

		public string Component { get; set; }

		public string SubComponent { get; set; }

		public string Partition { get; set; }

		public string Platform { get; }

		public string GroupingKey { get; set; }

		public ReleaseType ReleaseType { get; set; }

		public BuildType BuildType { get; set; }

		public CpuId CpuType { get; set; }

		public VersionInfo Version { get; set; }

		public bool IsFeatureIdentifier { get; set; }

		private IPkgBuilder GetResultBuilder()
		{
			IPkgBuilder pkgBuilder = Package.Create();
			pkgBuilder.Owner = Owner;
			pkgBuilder.OwnerType = OwnerType;
			pkgBuilder.Component = Component;
			pkgBuilder.SubComponent = SubComponent;
			pkgBuilder.Partition = Partition;
			pkgBuilder.Platform = Platform;
			pkgBuilder.GroupingKey = GroupingKey;
			pkgBuilder.ReleaseType = ReleaseType;
			pkgBuilder.CpuType = CpuType;
			pkgBuilder.BuildType = BuildType;
			pkgBuilder.Version = Version;
			return pkgBuilder;
		}

		public void AddPkg(KeyValuePair<string, IPkgInfo> pkg)
		{
			List<string> value = null;
			if (!string.IsNullOrEmpty(pkg.Value.Culture))
			{
				if (!_langPkgs.TryGetValue(pkg.Value.Culture, out value))
				{
					value = new List<string>();
					_langPkgs.Add(pkg.Value.Culture, value);
				}
			}
			else if (!string.IsNullOrEmpty(pkg.Value.Resolution))
			{
				if (!_resPkgs.TryGetValue(pkg.Value.Resolution, out value))
				{
					value = new List<string>();
					_resPkgs.Add(pkg.Value.Resolution, value);
				}
			}
			else
			{
				value = _basePkgs;
			}
			value.Add(pkg.Key);
		}

		public MergeResult Merge(string outputDir, bool compress, bool incremental)
		{
			MergeResult mergeResult = new MergeResult();
			using (IPkgBuilder pkgBuilder = GetResultBuilder())
			{
				mergeResult.FilePath = Path.Combine(outputDir, pkgBuilder.Name + PkgConstants.c_strPackageExtension);
				MergeWorker.Merge(pkgBuilder, _basePkgs, mergeResult.FilePath, compress, incremental);
			}
			mergeResult.Languages = _langPkgs.Keys.ToArray();
			foreach (KeyValuePair<string, List<string>> langPkg in _langPkgs)
			{
				using (IPkgBuilder pkgBuilder2 = GetResultBuilder())
				{
					pkgBuilder2.Culture = langPkg.Key;
					MergeWorker.Merge(pkgBuilder2, langPkg.Value, Path.Combine(outputDir, pkgBuilder2.Name + PkgConstants.c_strPackageExtension), compress, incremental);
				}
			}
			mergeResult.Resolutions = _resPkgs.Keys.ToArray();
			foreach (KeyValuePair<string, List<string>> resPkg in _resPkgs)
			{
				using (IPkgBuilder pkgBuilder3 = GetResultBuilder())
				{
					pkgBuilder3.Resolution = resPkg.Key;
					MergeWorker.Merge(pkgBuilder3, resPkg.Value, Path.Combine(outputDir, pkgBuilder3.Name + PkgConstants.c_strPackageExtension), compress, incremental);
				}
			}
			mergeResult.FeatureIdentifierPackage = IsFeatureIdentifier;
			return mergeResult;
		}
	}
}
