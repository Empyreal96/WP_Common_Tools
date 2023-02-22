using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents.Internal
{
	public class PackageSetBuilderBase
	{
		private string _partition;

		private string _driveLetter;

		protected List<KeyValuePair<SatelliteId, FileInfo>> _allFiles = new List<KeyValuePair<SatelliteId, FileInfo>>();

		public string Owner { get; set; }

		public string Component { get; set; }

		public string SubComponent { get; set; }

		public string Name => PackageTools.BuildPackageName(Owner, Component, SubComponent);

		public OwnerType OwnerType { get; set; }

		public ReleaseType ReleaseType { get; set; }

		public string Partition
		{
			get
			{
				return _partition;
			}
			set
			{
				_partition = value;
				_driveLetter = PackageTools.GetDefaultDriveLetter(value);
			}
		}

		public string Platform { get; set; }

		public string GroupingKey { get; set; }

		public string Description { get; set; }

		public List<SatelliteId> Resolutions { get; }

		public CpuId CpuType { get; private set; }

		public BuildType BuildType { get; private set; }

		public VersionInfo Version { get; private set; }

		protected PackageSetBuilderBase(CpuId cpuType, BuildType bldType, VersionInfo version)
		{
			CpuType = cpuType;
			BuildType = bldType;
			Version = version;
			Resolutions = new List<SatelliteId>();
		}

		protected IPkgBuilder CreatePackage(SatelliteId satelliteId)
		{
			IPkgBuilder pkgBuilder = Package.Create();
			pkgBuilder.BuildType = BuildType;
			pkgBuilder.CpuType = CpuType;
			pkgBuilder.Version = Version;
			pkgBuilder.Owner = Owner;
			pkgBuilder.OwnerType = OwnerType;
			pkgBuilder.Component = Component;
			pkgBuilder.SubComponent = SubComponent;
			pkgBuilder.ReleaseType = ReleaseType;
			pkgBuilder.Partition = Partition;
			pkgBuilder.Platform = Platform;
			pkgBuilder.GroupingKey = GroupingKey;
			pkgBuilder.BuildString = Description;
			pkgBuilder.Resolution = satelliteId.Resolution;
			pkgBuilder.Culture = satelliteId.Culture;
			return pkgBuilder;
		}

		public void AddFile(SatelliteId satelliteId, FileInfo fileInfo)
		{
			if (fileInfo.DevicePath != null)
			{
				if (satelliteId.SatType == SatelliteType.Language && fileInfo.DevicePath.IndexOf(satelliteId.Id, StringComparison.InvariantCultureIgnoreCase) == -1)
				{
					throw new PkgGenException("A file is added as a langauge specific file, but the destination path '{0}' is not langauge specific", fileInfo.DevicePath);
				}
				if (satelliteId.SatType == SatelliteType.Resolution)
				{
					bool flag = false;
					foreach (SatelliteId resolution in Resolutions)
					{
						if (satelliteId.Id == resolution.Id)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						throw new PkgGenException("A file is added as a resolution specific file, but the destination path '{0}' is not resolution specific", fileInfo.DevicePath);
					}
				}
				if (!Path.IsPathRooted(fileInfo.DevicePath))
				{
					throw new PkgGenException("Invalid device path '{0}', absolute path is required", fileInfo.DevicePath);
				}
				string pathRoot = Path.GetPathRoot(fileInfo.DevicePath.Substring(0, Math.Min(fileInfo.DevicePath.Length, 200)));
				if (!pathRoot.EndsWith("\\", StringComparison.InvariantCultureIgnoreCase))
				{
					throw new PkgGenException("Invalid device path '{0}', absolute path is required", fileInfo.DevicePath);
				}
				if (!pathRoot.StartsWith("\\", StringComparison.InvariantCultureIgnoreCase))
				{
					if (!pathRoot.StartsWith(_driveLetter, StringComparison.OrdinalIgnoreCase))
					{
						throw new PkgGenException("Invalid device path '{0}', only drive '{1}' can be used", fileInfo.DevicePath, _driveLetter);
					}
					fileInfo.DevicePath = fileInfo.DevicePath.Substring(_driveLetter.Length);
				}
			}
			_allFiles.Add(new KeyValuePair<SatelliteId, FileInfo>(satelliteId, fileInfo));
		}
	}
}
