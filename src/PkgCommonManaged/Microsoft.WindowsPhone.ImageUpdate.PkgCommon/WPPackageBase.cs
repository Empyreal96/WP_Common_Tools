using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgCommon
{
	internal class WPPackageBase
	{
		private string[] s_emptyTargetGroups = new string[0];

		protected PkgManifest m_pkgManifest;

		public PkgManifest Manifest => m_pkgManifest;

		public ReleaseType ReleaseType
		{
			get
			{
				return m_pkgManifest.ReleaseType;
			}
			set
			{
				if (value == ReleaseType.Invalid)
				{
					throw new PackageException("Invalid release type");
				}
				m_pkgManifest.ReleaseType = value;
			}
		}

		public PackageStyle PackageStyle
		{
			get
			{
				return m_pkgManifest.PackageStyle;
			}
			set
			{
				if (value == PackageStyle.Invalid)
				{
					throw new PackageException("Invalid cab type");
				}
				m_pkgManifest.PackageStyle = value;
			}
		}

		public BuildType BuildType
		{
			get
			{
				return m_pkgManifest.BuildType;
			}
			set
			{
				if (value == BuildType.Invalid)
				{
					throw new PackageException("Invalid build type");
				}
				m_pkgManifest.BuildType = value;
			}
		}

		public CpuId CpuType
		{
			get
			{
				switch (m_pkgManifest.CpuType)
				{
				case CpuId.ARM64_ARM:
					return CpuId.ARM;
				case CpuId.AMD64_X86:
					return CpuId.X86;
				case CpuId.ARM64_X86:
					return CpuId.X86;
				default:
					return m_pkgManifest.CpuType;
				}
			}
			set
			{
				if (value == CpuId.Invalid)
				{
					throw new PackageException("Invalid CPU type");
				}
				m_pkgManifest.CpuType = value;
			}
		}

		public CpuId ComplexCpuType => m_pkgManifest.CpuType;

		public string Keyform => m_pkgManifest.Keyform;

		public OwnerType OwnerType
		{
			get
			{
				return m_pkgManifest.OwnerType;
			}
			set
			{
				if (value == OwnerType.Invalid)
				{
					throw new PackageException("Invalid Owner type");
				}
				m_pkgManifest.OwnerType = value;
			}
		}

		public string Culture
		{
			get
			{
				return m_pkgManifest.Culture;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					m_pkgManifest.Culture = string.Empty;
					return;
				}
				if (!Regex.Match(value, PkgConstants.c_strCultureStringPattern).Success)
				{
					throw new PackageException("Invalid culture string {0}", value);
				}
				m_pkgManifest.Culture = value;
			}
		}

		public string Resolution
		{
			get
			{
				return m_pkgManifest.Resolution;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					m_pkgManifest.Resolution = string.Empty;
					return;
				}
				if (!Regex.Match(value, PkgConstants.c_strResolutionStringPattern).Success)
				{
					throw new PackageException("Invalid resolution string {0}", value);
				}
				m_pkgManifest.Resolution = value;
			}
		}

		public string Owner
		{
			get
			{
				return m_pkgManifest.Owner;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new PackageException("Owner string can't be empty");
				}
				if (value.Length > PkgConstants.c_iMaxPackageString)
				{
					throw new PackageException("Owner string can't be longer than {0} characters", PkgConstants.c_iMaxPackageString);
				}
				if (!Regex.Match(value, PkgConstants.c_strPackageStringPattern).Success)
				{
					throw new PackageException("Invalid owner string '{0}'", value);
				}
				m_pkgManifest.Owner = value;
			}
		}

		public string Component
		{
			get
			{
				return m_pkgManifest.Component;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new PackageException("Component string can't be empty");
				}
				if (value.Length > PkgConstants.c_iMaxPackageString)
				{
					throw new PackageException("Component string can't be longer than {0} characters", PkgConstants.c_iMaxPackageString);
				}
				if (!Regex.Match(value, PkgConstants.c_strPackageStringPattern).Success)
				{
					throw new PackageException("Invalid Component string '{0}'", value);
				}
				m_pkgManifest.Component = value;
			}
		}

		public string SubComponent
		{
			get
			{
				return m_pkgManifest.SubComponent;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (value.Length > PkgConstants.c_iMaxPackageString)
					{
						throw new PackageException("SubComponent string can't be longer than {0} characters", PkgConstants.c_iMaxPackageString);
					}
					if (!Regex.Match(value, PkgConstants.c_strPackageStringPattern).Success)
					{
						throw new PackageException("Invalid SubComponent string '{0}'", value);
					}
					m_pkgManifest.SubComponent = value;
				}
				else
				{
					m_pkgManifest.SubComponent = string.Empty;
				}
			}
		}

		public string Name => m_pkgManifest.Name;

		public string PackageName
		{
			get
			{
				return m_pkgManifest.PackageName;
			}
			set
			{
				m_pkgManifest.PackageName = value;
			}
		}

		public VersionInfo Version
		{
			get
			{
				return m_pkgManifest.Version;
			}
			set
			{
				m_pkgManifest.Version = value;
			}
		}

		public string PublicKey
		{
			get
			{
				return m_pkgManifest.PublicKey;
			}
			set
			{
				m_pkgManifest.PublicKey = value;
			}
		}

		public string Partition
		{
			get
			{
				return m_pkgManifest.Partition;
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					throw new PackageException("Partition string can't be empty");
				}
				if (value.Length > PkgConstants.c_iMaxPackageString)
				{
					throw new PackageException("Partition string can't be longer than {0} characters", PkgConstants.c_iMaxPackageString);
				}
				if (!Regex.Match(value, PkgConstants.c_strPackageStringPattern).Success)
				{
					throw new PackageException("Invalid Partition string '{0}'", value);
				}
				m_pkgManifest.Partition = value;
			}
		}

		public string Platform
		{
			get
			{
				return m_pkgManifest.Platform;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (value.Length > PkgConstants.c_iMaxPackageString)
					{
						throw new PackageException("Platform string can't be longer than {0} characters", PkgConstants.c_iMaxPackageString);
					}
					if (!Regex.Match(value, PkgConstants.c_strPackageStringPattern).Success)
					{
						throw new PackageException("Invalid Platform string '{0}'", value);
					}
					m_pkgManifest.Platform = value;
				}
				else
				{
					m_pkgManifest.Platform = string.Empty;
				}
			}
		}

		public string BuildString
		{
			get
			{
				return m_pkgManifest.BuildString;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (value.Length > PkgConstants.c_iMaxBuildString)
					{
						throw new PackageException("Build string can't be longer than {0} characters", PkgConstants.c_iMaxBuildString);
					}
					m_pkgManifest.BuildString = value;
				}
				else
				{
					m_pkgManifest.BuildString = string.Empty;
				}
			}
		}

		public string GroupingKey
		{
			get
			{
				return m_pkgManifest.GroupingKey;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (value.Length > PkgConstants.c_iMaxPackageString)
					{
						throw new PackageException("GroupingKey string can't be longer than {0} characters", PkgConstants.c_iMaxPackageString);
					}
					if (!Regex.Match(value, PkgConstants.c_strPackageStringPattern).Success)
					{
						throw new PackageException("Invalid GroupingKey string '{0}'", value);
					}
					m_pkgManifest.GroupingKey = value;
				}
				else
				{
					m_pkgManifest.GroupingKey = string.Empty;
				}
			}
		}

		public string[] TargetGroups
		{
			get
			{
				return m_pkgManifest.TargetGroups;
			}
			set
			{
				if (value != null)
				{
					foreach (string text in value)
					{
						if (string.IsNullOrEmpty(text))
						{
							throw new PackageException("Group ID can't be empty or null");
						}
						if (text.Length > PkgConstants.c_iMaxGroupIdString)
						{
							throw new PackageException("Group ID '{0}' can not exceed '{1}' characters", PkgConstants.c_iMaxGroupIdString);
						}
						if (!Regex.Match(text, PkgConstants.c_strGroupIdPattern).Success)
						{
							throw new PackageException("Invalid group ID string '{0}'", text);
						}
					}
					m_pkgManifest.TargetGroups = value;
				}
				else
				{
					m_pkgManifest.TargetGroups = s_emptyTargetGroups;
				}
			}
		}

		public int FileCount => m_pkgManifest.m_files.Count;

		public IEnumerable<IFileEntry> Files => m_pkgManifest.m_files.Values;

		public bool IsWow => ComplexCpuType != CpuType;

		public bool IsBinaryPartition => m_pkgManifest.IsBinaryPartition;

		protected WPPackageBase(PkgManifest pkgManifest)
		{
			m_pkgManifest = pkgManifest;
		}

		public IFileEntry FindFile(string devicePath)
		{
			FileEntry value = null;
			m_pkgManifest.m_files.TryGetValue(devicePath, out value);
			return value;
		}

		public IFileEntry GetDsmFile()
		{
			return m_pkgManifest.m_files.First((KeyValuePair<string, FileEntry> x) => x.Value.FileType == FileType.Manifest).Value;
		}

		public override string ToString()
		{
			return m_pkgManifest.Name;
		}
	}
}
