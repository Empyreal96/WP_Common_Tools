using System.Collections.Generic;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public interface IPackageSetBuilder
	{
		string Owner { get; set; }

		string Component { get; set; }

		string SubComponent { get; set; }

		string Name { get; }

		OwnerType OwnerType { get; set; }

		ReleaseType ReleaseType { get; set; }

		string Partition { get; set; }

		string Platform { get; set; }

		string GroupingKey { get; set; }

		string Description { get; set; }

		List<SatelliteId> Resolutions { get; }

		BuildType BuildType { get; }

		CpuId CpuType { get; }

		VersionInfo Version { get; }

		void AddFile(SatelliteId satelliteId, FileInfo fileInfo);

		void AddRegValue(SatelliteId satelliteId, RegValueInfo valueInfo);

		void AddMultiSzSegment(string keyName, string valueName, params string[] valueSegments);

		void Save(string outputDir);
	}
}
