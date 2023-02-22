using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class PkgMerger : IDisposable
	{
		private bool _hasError;

		private string _tmpDir;

		private VersionInfo _finalVersion = new VersionInfo(0, 0, 0, 0);

		private CpuId _cpuType;

		private BuildType _buildType;

		private ReleaseType _releaseType;

		private Dictionary<string, string> _allPackages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		private Dictionary<string, Tuple<IPkgBuilder, StringBuilder>> _allMergedPackages = new Dictionary<string, Tuple<IPkgBuilder, StringBuilder>>(StringComparer.OrdinalIgnoreCase);

		private static string BuildComponentString(IPkgInfo pkgInfo)
		{
			if (string.IsNullOrEmpty(pkgInfo.Partition))
			{
				throw new PackageException("Unexpected empty partition for package '{0}'", pkgInfo.Name);
			}
			if (string.IsNullOrEmpty(pkgInfo.Platform))
			{
				return pkgInfo.Partition;
			}
			return $"{pkgInfo.Platform}_{pkgInfo.Partition}";
		}

		private static string BuildSubComponentString(IPkgInfo pkgInfo)
		{
			if (string.IsNullOrEmpty(pkgInfo.GroupingKey))
			{
				return pkgInfo.ReleaseType.ToString();
			}
			return $"{pkgInfo.GroupingKey}_{pkgInfo.ReleaseType}";
		}

		private void Error(string format, params object[] args)
		{
			LogUtil.Error(format, args);
			_hasError = true;
		}

		private Tuple<IPkgBuilder, StringBuilder> FindMergedPackage(IPkgInfo pkg)
		{
			string key = $"{pkg.Owner}.{pkg.Partition}.{pkg.Platform}.{pkg.GroupingKey}.{pkg.ReleaseType}.{pkg.BuildType}.{pkg.CpuType}.{pkg.Culture}.{pkg.Resolution}";
			Tuple<IPkgBuilder, StringBuilder> value = null;
			if (!_allMergedPackages.TryGetValue(key, out value))
			{
				IPkgBuilder pkgBuilder = Package.Create();
				pkgBuilder.Owner = pkg.Owner;
				pkgBuilder.OwnerType = pkg.OwnerType;
				pkgBuilder.Component = BuildComponentString(pkg);
				pkgBuilder.SubComponent = BuildSubComponentString(pkg);
				pkgBuilder.Resolution = pkg.Resolution;
				pkgBuilder.Culture = pkg.Culture;
				pkgBuilder.Partition = pkg.Partition;
				pkgBuilder.Platform = pkg.Platform;
				pkgBuilder.CpuType = pkg.CpuType;
				pkgBuilder.ReleaseType = pkg.ReleaseType;
				pkgBuilder.BuildType = pkg.BuildType;
				pkgBuilder.GroupingKey = pkg.GroupingKey;
				value = Tuple.Create(pkgBuilder, new StringBuilder());
				_allMergedPackages.Add(key, value);
			}
			return value;
		}

		private void AddPackage(string pkgFile)
		{
			if (!LongPathFile.Exists(pkgFile))
			{
				Error("Package '{0}' doesn't exist", pkgFile);
				return;
			}
			string outputDir = Path.Combine(_tmpDir, _allPackages.Count().ToString());
			WPExtractedPackage wPExtractedPackage = WPCanonicalPackage.ExtractAndLoad(pkgFile, outputDir);
			if (wPExtractedPackage.IsBinaryPartition)
			{
				LogUtil.Warning("Package '{0}' skipped because it's a binary partition package", pkgFile);
				return;
			}
			if (wPExtractedPackage.Version != _finalVersion)
			{
				if (_finalVersion != default(VersionInfo))
				{
					LogUtil.Message("Package '{0}' has inconsistent version '{1}', expecting '{2}', choosing higher one", pkgFile, wPExtractedPackage.Version, _finalVersion);
				}
				if (wPExtractedPackage.Version > _finalVersion)
				{
					_finalVersion = wPExtractedPackage.Version;
				}
			}
			if (string.IsNullOrEmpty(wPExtractedPackage.Partition))
			{
				Error("Package '{0}' contains empty partition name", pkgFile);
				return;
			}
			if (wPExtractedPackage.ReleaseType != _releaseType)
			{
				Error("Package '{0}' has unexpected release type '{1}', expecting '{2}'", pkgFile, wPExtractedPackage.ReleaseType, _releaseType);
				return;
			}
			if (wPExtractedPackage.CpuType != _cpuType)
			{
				Error("Package '{0}' has unexpected cpu type '{1}', expecting '{2}'", pkgFile, wPExtractedPackage.CpuType, _cpuType);
				return;
			}
			if (wPExtractedPackage.BuildType != _buildType)
			{
				Error("Package '{0}' has unexpected build type '{1}', expecting '{2}'", pkgFile, wPExtractedPackage.BuildType, _buildType);
				return;
			}
			string key = $"Name:{wPExtractedPackage.Name}, Partition:{wPExtractedPackage.Partition}";
			if (_allPackages.ContainsKey(key))
			{
				Error("Package '{0}' and package '{1}' have same package name and target partition", pkgFile, _allPackages[key]);
				return;
			}
			_allPackages.Add(key, pkgFile);
			Tuple<IPkgBuilder, StringBuilder> tuple = FindMergedPackage(wPExtractedPackage);
			IPkgBuilder item = tuple.Item1;
			if (item.OwnerType != wPExtractedPackage.OwnerType)
			{
				Error("Package '{0}' has inconsistent owner type '{1}', expecting '{2}'", pkgFile, wPExtractedPackage.OwnerType, item.OwnerType);
				return;
			}
			foreach (FileEntry file in wPExtractedPackage.Files)
			{
				if (file.FileType != FileType.Manifest && file.FileType != FileType.Catalog)
				{
					IFileEntry fileEntry2 = item.FindFile(file.DevicePath);
					if (fileEntry2 != null)
					{
						Error("Package '{0}' and package with name '{1}' both contain file with same device path '{2}'", pkgFile, fileEntry2.SourcePackage, file.DevicePath);
					}
					else
					{
						item.AddFile(file.FileType, file.SourcePath, file.DevicePath, file.Attributes, wPExtractedPackage.Name);
					}
				}
			}
			tuple.Item2.AppendLine($"{pkgFile},{wPExtractedPackage.Version}");
		}

		private void Save(string outputDir, VersionInfo? outputVersion, bool compress)
		{
			if (_hasError)
			{
				throw new PackageException("Errors occurred during merging, check console output for details");
			}
			if (outputVersion.HasValue)
			{
				_finalVersion = outputVersion.Value;
			}
			foreach (Tuple<IPkgBuilder, StringBuilder> value in _allMergedPackages.Values)
			{
				IPkgBuilder item = value.Item1;
				item.Version = _finalVersion;
				item.SaveCab(Path.Combine(outputDir, item.Name + PkgConstants.c_strPackageExtension), compress);
				StringBuilder item2 = value.Item2;
				LongPathFile.WriteAllText(Path.Combine(outputDir, item.Name + ".merged.txt"), item2.ToString());
			}
		}

		private PkgMerger(ReleaseType releaseType, CpuId cpuType, BuildType buildType)
		{
			_releaseType = releaseType;
			_cpuType = cpuType;
			_buildType = buildType;
			_tmpDir = FileUtils.GetTempDirectory();
			_allPackages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			_allMergedPackages = new Dictionary<string, Tuple<IPkgBuilder, StringBuilder>>(StringComparer.OrdinalIgnoreCase);
		}

		internal static void Merge(IEnumerable<string> inputPkgs, VersionInfo? outputVersion, ReleaseType expectedReleaseType, CpuId expectedCpuType, BuildType expectedBuildType, string outputDir, bool compress, IDeploymentLogger _logger)
		{
			using (PkgMerger pkgMerger = new PkgMerger(expectedReleaseType, expectedCpuType, expectedBuildType))
			{
				foreach (string inputPkg in inputPkgs)
				{
					_logger.LogInfo("Adding package '{0}' ...", inputPkg);
					pkgMerger.AddPackage(inputPkg);
				}
				_logger.LogInfo("Merging and saving results to directory '{0}' ...", outputDir);
				pkgMerger.Save(outputDir, outputVersion, compress);
				_logger.LogInfo("Done.");
			}
		}

		public void Dispose()
		{
			foreach (Tuple<IPkgBuilder, StringBuilder> value in _allMergedPackages.Values)
			{
				value.Item1.Dispose();
			}
			try
			{
				FileUtils.DeleteTree(_tmpDir);
			}
			catch (IOException)
			{
			}
		}
	}
}
