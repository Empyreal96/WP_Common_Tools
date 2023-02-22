using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.FeatureAPI
{
	public class Edition
	{
		public const string MSPACKAGEROOT = "MSPackageRoot";

		public const string PREBUILT = "Prebuilt";

		public const string ENV_OS_ROOT = "OSCONTENTROOT";

		[XmlAttribute]
		public string Name;

		[XmlAttribute]
		public string AlternateName = string.Empty;

		[XmlAttribute]
		public bool AllowOEMCustomizations = true;

		[XmlAttribute]
		public bool RequiresKeyboard;

		[XmlAttribute]
		public ReleaseType ReleaseType = ReleaseType.Test;

		[XmlAttribute]
		[DefaultValue(false)]
		public uint MinimumUserStoreSize;

		[XmlAttribute]
		public string InternalProductDir;

		[XmlArrayItem(ElementName = "Package", Type = typeof(EditionPackage), IsNullable = false)]
		[XmlArray]
		public List<EditionPackage> CoreFeatureManifestPackages;

		[XmlArrayItem(ElementName = "Package", Type = typeof(EditionPackage), IsNullable = false)]
		[XmlArray]
		public List<EditionPackage> OptionalFeatureManifestPackages;

		[XmlArrayItem(ElementName = "CPUType", Type = typeof(SupportedCPUType), IsNullable = false)]
		[XmlArray]
		public List<SupportedCPUType> SupportedCPUTypes;

		private string _msPackageRoot = string.Empty;

		public EditionUISettings UISettings;

		private string _installRoot = string.Empty;

		private List<string> _installedCPUTypes;

		[XmlIgnore]
		public string MSPackageRoot
		{
			get
			{
				if (string.IsNullOrEmpty(_msPackageRoot) && !string.IsNullOrEmpty(InstallRoot))
				{
					string text = Path.Combine(InstallRoot, "MSPackageRoot");
					if (LongPathDirectory.Exists(text))
					{
						_msPackageRoot = text;
					}
					else
					{
						text = Path.Combine(InstallRoot, "Prebuilt");
						if (LongPathDirectory.Exists(text))
						{
							_msPackageRoot = text;
						}
					}
				}
				return _msPackageRoot;
			}
		}

		[XmlIgnore]
		public bool IsInstalled
		{
			get
			{
				if (InstalledCPUTypes != null)
				{
					return InstalledCPUTypes.Count > 0;
				}
				return false;
			}
		}

		[XmlIgnore]
		public string InstallRoot
		{
			get
			{
				if (string.IsNullOrEmpty(_installRoot))
				{
					foreach (EditionLookup lookup in UISettings.Lookups)
					{
						if (string.IsNullOrWhiteSpace(lookup.InstallPath))
						{
							continue;
						}
						foreach (string item in SupportedCPUTypes.Select((SupportedCPUType cpuitem) => cpuitem.CpuType))
						{
							if (FMPackagesFound(Path.Combine(lookup.InstallPath, lookup.MSPackageDirectoryName), item))
							{
								_installRoot = lookup.InstallPath;
								_msPackageRoot = Path.Combine(_installRoot, lookup.MSPackageDirectoryName);
								break;
							}
						}
					}
				}
				return _installRoot;
			}
		}

		[XmlIgnore]
		public List<string> InstalledCPUTypes
		{
			get
			{
				if (_installedCPUTypes == null && !string.IsNullOrWhiteSpace(InstallRoot))
				{
					List<string> list = new List<string>();
					foreach (string item in SupportedCPUTypes.Select((SupportedCPUType cpuitem) => cpuitem.CpuType))
					{
						if (FMPackagesFound(MSPackageRoot, item))
						{
							list.Add(item);
						}
					}
					if (list.Count > 0)
					{
						_installedCPUTypes = list;
					}
				}
				return _installedCPUTypes;
			}
		}

		public List<CpuId> GetSupportedWowCpuTypes(CpuId hostCpuType)
		{
			SupportedCPUType supportedCPUType = SupportedCPUTypes.FirstOrDefault((SupportedCPUType cpuitem) => cpuitem.CpuType.Equals(hostCpuType.ToString(), StringComparison.OrdinalIgnoreCase));
			if (supportedCPUType == null)
			{
				return new List<CpuId>();
			}
			return supportedCPUType.WowGuestCpuIds;
		}

		private bool FMPackagesFound(string msPackageRoot, string cpuType)
		{
			bool result = true;
			foreach (EditionPackage coreFeatureManifestPackage in CoreFeatureManifestPackages)
			{
				if (!coreFeatureManifestPackage.ExistsUnder(msPackageRoot, cpuType, "fre"))
				{
					result = false;
				}
			}
			foreach (EditionPackage optionalFeatureManifestPackage in OptionalFeatureManifestPackages)
			{
				if (!optionalFeatureManifestPackage.ExistsUnder(msPackageRoot, cpuType, "fre"))
				{
					result = false;
				}
			}
			return result;
		}

		public override string ToString()
		{
			return Name;
		}

		public bool IsProduct(string productName)
		{
			if (!productName.Equals(Name, StringComparison.OrdinalIgnoreCase))
			{
				return productName.Equals(AlternateName, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}
}
