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
	public class PkgFile
	{
		public static readonly string DefaultLanguagePattern = "_Lang_";

		public static readonly string DefaultResolutionPattern = "_Res_";

		public static readonly string DefaultWowPattern = "_Wow_";

		private string _ID;

		[XmlAttribute("Path")]
		public string Directory;

		[XmlAttribute("LangPath")]
		public string LangDirectory;

		[XmlAttribute("Name")]
		public string Name;

		[XmlAttribute("NoBasePackage")]
		[DefaultValue(false)]
		public bool NoBasePackage;

		[XmlAttribute("FeatureIdentifierPackage")]
		[DefaultValue(false)]
		public bool FeatureIdentifierPackage;

		[XmlAttribute("Resolution")]
		public string Resolution;

		[XmlAttribute("Language")]
		public string Language;

		[XmlAttribute("Wow")]
		public string Wow;

		[XmlAttribute("LangWow")]
		public string LangWow;

		[XmlAttribute("ResWow")]
		public string ResWow;

		[XmlAttribute("CPUType")]
		[DefaultValue(null)]
		public string CPUType;

		[XmlAttribute("Partition")]
		[DefaultValue(null)]
		public string Partition;

		private VersionInfo? _version;

		[XmlAttribute("PublicKey")]
		[DefaultValue(null)]
		public string PublicKey;

		[DefaultValue(false)]
		[XmlAttribute]
		public bool BinaryPartition;

		[XmlIgnore]
		public FeatureManifest.PackageGroups FMGroup;

		private List<string> _groupValues;

		[XmlIgnore]
		public OEMInput OemInput;

		private static List<CpuId> _supportedCPUIds = null;

		private List<CpuId> _cpuIds;

		[XmlAttribute("ID")]
		public string ID
		{
			get
			{
				if (string.IsNullOrEmpty(_ID))
				{
					return Path.GetFileNameWithoutExtension(Name);
				}
				return _ID;
			}
			set
			{
				_ID = value;
			}
		}

		[XmlAttribute("Version")]
		[DefaultValue(null)]
		public string Version
		{
			get
			{
				if (!_version.HasValue || string.IsNullOrEmpty(_version.ToString()))
				{
					return null;
				}
				return _version.ToString();
			}
			set
			{
				if (string.IsNullOrEmpty(value))
				{
					_version = null;
					return;
				}
				string[] array = value.Split('.');
				ushort[] array2 = new ushort[4];
				for (int i = 0; i < Math.Min(array.Count(), 4); i++)
				{
					if (string.IsNullOrEmpty(array[i]))
					{
						array2[i] = 0;
					}
					else
					{
						array2[i] = ushort.Parse(array[i]);
					}
				}
				if (array.Count() != 4)
				{
					_version = null;
				}
				else
				{
					_version = new VersionInfo(array2[0], array2[1], array2[2], array2[3]);
				}
			}
		}

		[XmlIgnore]
		public virtual string GroupValue
		{
			get
			{
				return null;
			}
			set
			{
				if (value != null)
				{
					_groupValues = value.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				}
				else
				{
					_groupValues = null;
				}
			}
		}

		[XmlIgnore]
		public List<string> GroupValues
		{
			get
			{
				if (_groupValues == null && !string.IsNullOrWhiteSpace(GroupValue))
				{
					_groupValues = GroupValue.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
				}
				return _groupValues;
			}
		}

		[XmlIgnore]
		public virtual bool IncludeInImage
		{
			get
			{
				if (OemInput == null)
				{
					return false;
				}
				return true;
			}
		}

		[XmlIgnore]
		public string PackagePath
		{
			get
			{
				string text = RawPackagePath;
				if (OemInput != null)
				{
					text = OemInput.ProcessOEMInputVariables(text);
				}
				return Environment.ExpandEnvironmentVariables(text);
			}
		}

		[XmlIgnore]
		public string RawPackagePath => Path.Combine(Directory, Name);

		public static List<CpuId> SupportedCPUIds
		{
			get
			{
				if (_supportedCPUIds == null)
				{
					_supportedCPUIds = Enum.GetValues(typeof(CpuId)).Cast<CpuId>().ToList();
					_supportedCPUIds = _supportedCPUIds.Where((CpuId id) => id != CpuId.Invalid).ToList();
				}
				return _supportedCPUIds;
			}
		}

		[XmlIgnore]
		public List<CpuId> CPUIds
		{
			get
			{
				if (_cpuIds == null)
				{
					if (string.IsNullOrWhiteSpace(CPUType) || CPUType.Equals("*"))
					{
						_cpuIds = SupportedCPUIds;
					}
					else
					{
						List<string> supportedList = GetSupportedList(CPUType);
						_cpuIds = new List<CpuId>();
						bool ignoreCase = true;
						foreach (string item in supportedList)
						{
							CpuId result;
							if (Enum.TryParse<CpuId>(item, ignoreCase, out result))
							{
								_cpuIds.Add(result);
							}
						}
					}
				}
				return _cpuIds;
			}
		}

		[XmlIgnore]
		public string RawLanguagePackagePath
		{
			get
			{
				string path = (string.IsNullOrEmpty(LangDirectory) ? Directory : LangDirectory);
				string extension = Path.GetExtension(Name);
				return Path.Combine(path, Name.Replace(extension, DefaultLanguagePattern + "$(langid)" + extension, StringComparison.OrdinalIgnoreCase));
			}
		}

		[XmlIgnore]
		public string RawWowPackagePath
		{
			get
			{
				string extension = Path.GetExtension(Name);
				return Path.Combine(Directory, Name.Replace(extension, DefaultWowPattern + "$(cputype).$(guestcputype)" + extension, StringComparison.OrdinalIgnoreCase));
			}
		}

		[XmlIgnore]
		public string RawResolutionPackagePath
		{
			get
			{
				string extension = Path.GetExtension(Name);
				return Path.Combine(Directory, Name.Replace(extension, DefaultResolutionPattern + "$(resid)" + extension, StringComparison.OrdinalIgnoreCase));
			}
		}

		public PkgFile()
		{
		}

		public PkgFile(FeatureManifest.PackageGroups fmGroup)
		{
			FMGroup = fmGroup;
		}

		public PkgFile(PkgFile srcPkg)
		{
			CopyPkgFile(srcPkg);
		}

		public bool IsValidGroupValue(string groupValue)
		{
			bool result = true;
			if (!string.IsNullOrWhiteSpace(GroupValue))
			{
				result = GroupValues.Contains(groupValue, StringComparer.OrdinalIgnoreCase);
			}
			return result;
		}

		public bool IncludesCPUType(string cpuType)
		{
			bool ignoreCase = true;
			CpuId result;
			if (!Enum.TryParse<CpuId>(cpuType, ignoreCase, out result))
			{
				return false;
			}
			if (result != 0)
			{
				return CPUIds.Contains(result);
			}
			return false;
		}

		public string GetLanguagePackagePath(string language)
		{
			string rawLanguagePackagePath = RawLanguagePackagePath;
			rawLanguagePackagePath = rawLanguagePackagePath.Replace("$(langid)", language, StringComparison.OrdinalIgnoreCase);
			if (OemInput != null)
			{
				rawLanguagePackagePath = OemInput.ProcessOEMInputVariables(rawLanguagePackagePath);
			}
			return Environment.ExpandEnvironmentVariables(rawLanguagePackagePath);
		}

		public string GetWowPackagePath(string guestCpuType)
		{
			string rawWowPackagePath = RawWowPackagePath;
			rawWowPackagePath = rawWowPackagePath.Replace("$(guestcputype)", guestCpuType, StringComparison.OrdinalIgnoreCase);
			if (OemInput != null)
			{
				rawWowPackagePath = OemInput.ProcessOEMInputVariables(rawWowPackagePath);
			}
			return Environment.ExpandEnvironmentVariables(rawWowPackagePath);
		}

		public string GetResolutionPackagePath(string resolution)
		{
			string rawResolutionPackagePath = RawResolutionPackagePath;
			rawResolutionPackagePath = rawResolutionPackagePath.Replace("$(resid)", resolution, StringComparison.OrdinalIgnoreCase);
			if (OemInput != null)
			{
				rawResolutionPackagePath = OemInput.ProcessOEMInputVariables(rawResolutionPackagePath);
			}
			return Environment.ExpandEnvironmentVariables(rawResolutionPackagePath);
		}

		public void ProcessVariables()
		{
			if (OemInput != null)
			{
				Directory = OemInput.ProcessOEMInputVariables(Directory);
			}
		}

		public static List<string> GetSupportedList(string list)
		{
			char[] separator = new char[1] { ';' };
			List<string> list2 = new List<string>();
			list = list.Trim();
			list = list.Replace("(", "", StringComparison.OrdinalIgnoreCase);
			list = list.Replace(")", "", StringComparison.OrdinalIgnoreCase);
			list = list.Replace("!", "", StringComparison.OrdinalIgnoreCase);
			list2.AddRange(list.Split(separator));
			return list2;
		}

		public static string GetSupportedListString(List<string> list, List<string> supportedList)
		{
			string result = null;
			if (list.Count() > 0)
			{
				result = ((list.Except(supportedList, StringComparer.OrdinalIgnoreCase).Count() != 0 || supportedList.Except(list, StringComparer.OrdinalIgnoreCase).Count() != 0) ? ("(" + string.Join(";", list) + ")") : "*");
			}
			return result;
		}

		public static List<string> GetSupportedList(string list, List<string> supportedValues)
		{
			List<string> list2 = new List<string>();
			list = list.Trim();
			if (string.Compare(list, "*", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return supportedValues;
			}
			if (list.Contains("*"))
			{
				throw new FeatureAPIException("FeatureAPI!GetSupportedList: Supported values list '" + list + "' cannot include '*' unless it is alone (\"*\" to specify all supported values)");
			}
			List<string> supportedList = GetSupportedList(list);
			if (list.Contains("!"))
			{
				int num = list.IndexOf("!", StringComparison.OrdinalIgnoreCase);
				if (list.LastIndexOf("!", StringComparison.OrdinalIgnoreCase) != num || num != 0)
				{
					throw new FeatureAPIException("FeatureAPI!GetSupportedList: Supported values list '" + list + "' cannot contain both include and exclude values.  Exclude lists must contain a '!' at the beginning.");
				}
				{
					foreach (string supportedValue in supportedValues)
					{
						bool flag = false;
						foreach (string item in supportedList)
						{
							if (string.Equals(supportedValue, item, StringComparison.OrdinalIgnoreCase))
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							list2.Add(supportedValue);
						}
					}
					return list2;
				}
			}
			foreach (string item2 in supportedList)
			{
				foreach (string supportedValue2 in supportedValues)
				{
					if (string.Equals(supportedValue2, item2, StringComparison.OrdinalIgnoreCase))
					{
						list2.Add(supportedValue2);
						break;
					}
				}
			}
			return list2;
		}

		public virtual void CopyPkgFile(PkgFile srcPkgFile)
		{
			Directory = srcPkgFile.Directory;
			Language = srcPkgFile.Language;
			Wow = srcPkgFile.Wow;
			LangWow = srcPkgFile.LangWow;
			ResWow = srcPkgFile.ResWow;
			Name = srcPkgFile.Name;
			NoBasePackage = srcPkgFile.NoBasePackage;
			OemInput = srcPkgFile.OemInput;
			Resolution = srcPkgFile.Resolution;
			FeatureIdentifierPackage = srcPkgFile.FeatureIdentifierPackage;
			CPUType = srcPkgFile.CPUType;
		}

		public virtual void InitializeWithMergeResult(MergeResult result, FeatureManifest.PackageGroups fmGroup, string groupValue, List<string> supportedLanguages, List<string> supportedResolutions)
		{
			Directory = LongPath.GetDirectoryName(result.FilePath);
			Name = Path.GetFileName(result.FilePath);
			_ID = result.PkgInfo.Name;
			FeatureIdentifierPackage = result.FeatureIdentifierPackage;
			Partition = result.PkgInfo.Partition;
			_version = result.PkgInfo.Version;
			PublicKey = result.PkgInfo.PublicKey;
			BinaryPartition = result.PkgInfo.IsBinaryPartition;
			FMGroup = fmGroup;
			if (result.Languages != null)
			{
				Language = GetSupportedListString(result.Languages.ToList(), supportedLanguages);
			}
			if (result.Resolutions != null)
			{
				Resolution = GetSupportedListString(result.Resolutions.ToList(), supportedResolutions);
			}
			GroupValue = groupValue;
		}

		public override string ToString()
		{
			return ID;
		}
	}
}
