using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class PackageTocBuilder : PackageSetBuilderBase, IPackageSetBuilder
	{
		private const string c_strPkgTocExtension = ".TOC";

		private void BuildPackageTOC(SatelliteId satelliteId, IEnumerable<FileInfo> files, string outputDir)
		{
			IPkgBuilder pkgBuilder = CreatePackage(satelliteId);
			string text = Path.Combine(outputDir, pkgBuilder.Name + ".TOC");
			LogUtil.Message("Building package content list '{0}'", text);
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("Partition={0}", pkgBuilder.Partition);
			stringBuilder.AppendLine();
			foreach (FileInfo file in files)
			{
				if (file.Type == FileType.Regular)
				{
					stringBuilder.AppendFormat("{0},{1}", file.SourcePath, file.DevicePath);
					stringBuilder.AppendLine();
				}
			}
			LongPathFile.WriteAllText(text, stringBuilder.ToString());
			LogUtil.Message("Done package content list '{0}'", text);
		}

		public void AddRegValue(SatelliteId satelliteId, RegValueInfo valueInfo)
		{
		}

		public void AddMultiSzSegment(string keyName, string valueName, params string[] valueSegments)
		{
		}

		public void Save(string outputDir)
		{
			foreach (IGrouping<SatelliteId, KeyValuePair<SatelliteId, FileInfo>> item in from x in _allFiles
				group x by x.Key)
			{
				BuildPackageTOC(item.Key, item.Select((KeyValuePair<SatelliteId, FileInfo> x) => x.Value), outputDir);
			}
		}

		public PackageTocBuilder(CpuId cpuType, BuildType bldType, VersionInfo version)
			: base(cpuType, bldType, version)
		{
		}
	}
}
