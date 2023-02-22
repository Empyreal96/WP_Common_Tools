using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class Settings
	{
		public class PackageAttributes
		{
			public static readonly string DeviceRegFilePath = PkgConstants.c_strRguDeviceFolder + "\\";

			public static readonly string DeviceLogFilePath = "\\Windows\\Logs\\OemCustomizationTool\\";

			private static string owner = "UnknownOwner";

			public static OwnerType OwnerType = OwnerType.Invalid;

			public static ReleaseType ReleaseType = ReleaseType.Invalid;

			public const string Component = "ImageCustomization";

			public const string SubComponent = "RegistryCustomization";

			public static readonly string mainOSPartitionString = PkgConstants.c_strMainOsPartition;

			public static readonly string updateOSPartitionString = PkgConstants.c_strUpdateOsPartition;

			public static readonly string efiPartitionString = PkgConstants.c_strEfiPartition;

			public static CpuId CpuType = CpuId.ARM;

			public static VersionInfo Version = new VersionInfo(0, 0, 0, 0);

			public static string Owner
			{
				get
				{
					return owner;
				}
				set
				{
					owner = value;
				}
			}

			public static string OwnerTypeString
			{
				get
				{
					return OwnerType.ToString();
				}
				set
				{
					if (value.Equals(OwnerType.OEM.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						OwnerType = OwnerType.OEM;
						return;
					}
					if (value.Equals(OwnerType.MobileOperator.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						OwnerType = OwnerType.MobileOperator;
						return;
					}
					if (value.Equals(OwnerType.Microsoft.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						OwnerType = OwnerType.Microsoft;
						return;
					}
					if (value.Equals(OwnerType.SiliconVendor.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						OwnerType = OwnerType.SiliconVendor;
						return;
					}
					throw new CustomizationXmlException("OwnerType attribute is invalid. Expecting 'Microsoft', 'OEM', 'SiliconVendor' or 'MobileOperator'. Received " + value);
				}
			}

			public static string ReleaseTypeString
			{
				get
				{
					return ReleaseType.ToString();
				}
				set
				{
					if (value.Equals(ReleaseType.Production.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						ReleaseType = ReleaseType.Production;
						return;
					}
					if (value.Equals(ReleaseType.Test.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						ReleaseType = ReleaseType.Test;
						return;
					}
					throw new CustomizationXmlException("ReleaseType attribute is invalid. Expecting 'Production' or 'Test'. Received " + value);
				}
			}

			public static string CpuTypeString
			{
				get
				{
					return CpuType.ToString();
				}
				set
				{
					if (value.Equals(CpuId.X86.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						CpuType = CpuId.X86;
						return;
					}
					if (value.Equals(CpuId.ARM.ToString(), StringComparison.OrdinalIgnoreCase))
					{
						CpuType = CpuId.ARM;
						return;
					}
					throw new ArgumentException("CpuType attribute is invalid. Expecting 'X86' or 'ARM'. Received " + value);
				}
			}

			public static string VersionString
			{
				get
				{
					return Version.ToString();
				}
				set
				{
					if (!new Regex("^[0-9]*[.][0-9]*[.][0-9]*[.][0-9]*$").IsMatch(value))
					{
						throw new ArgumentException("Unexpected version string: '" + value + "'");
					}
					string[] array = value.Split('.');
					Version.Major = ushort.Parse(array[0]);
					Version.Minor = ushort.Parse(array[1]);
					Version.QFE = ushort.Parse(array[2]);
					Version.Build = ushort.Parse(array[3]);
				}
			}

			public static string MainOSPkgFilename => Owner + "." + mainOSPartitionString + ".ImageCustomization.RegistryCustomization" + PkgConstants.c_strPackageExtension;

			public static string UpdateOSPkgFilename => Owner + "." + updateOSPartitionString + ".ImageCustomization.RegistryCustomization" + PkgConstants.c_strPackageExtension;

			public static string EfiPkgFilename => Owner + "." + efiPartitionString + ".ImageCustomization.RegistryCustomization" + PkgConstants.c_strPackageExtension;

			public static BuildType BuildType
			{
				get
				{
					string environmentVariable = Environment.GetEnvironmentVariable("_BuildType");
					if (environmentVariable != null && environmentVariable.Equals("chk", StringComparison.OrdinalIgnoreCase))
					{
						return BuildType.Checked;
					}
					if (environmentVariable != null && environmentVariable.Equals("fre", StringComparison.OrdinalIgnoreCase))
					{
						return BuildType.Retail;
					}
					TraceLogger.LogMessage(TraceLevel.Warn, "Environment variable %_BuildType% not set. Using 'fre'.");
					return BuildType.Retail;
				}
			}
		}

		private static string configSchema = string.Empty;

		private static string customizationSchema = string.Empty;

		private static string registrySchema = string.Empty;

		private const string c_strPkgGenCfgXmlName = "pkggen.cfg.xml";

		private static Stream pkgGenCfgXml = null;

		public const string MagicRegFilename = "OEMSettings.reg";

		private static string customizationIncludeDirectory = string.Empty;

		private static List<XmlFile> customizationFiles = null;

		private static List<XmlFile> configFiles = null;

		private static bool warnOnMappingNotFound = false;

		private static bool diagnostics = false;

		private static string outputDirectoryPath = ".\\";

		private static string tempDirectoryPath = string.Empty;

		private static string mergeFilePath = string.Empty;

		public static string ConfigSchema
		{
			get
			{
				if (configSchema == string.Empty)
				{
					string[] manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
					TraceLogger.LogMessage(TraceLevel.Info, "Looking for Config schema in resource list:");
					string[] array = manifestResourceNames;
					foreach (string text in array)
					{
						TraceLogger.LogMessage(TraceLevel.Info, text);
						if (text.EndsWith("Config.xsd", StringComparison.OrdinalIgnoreCase))
						{
							configSchema = text;
							return configSchema;
						}
					}
					throw new SystemException("Could not find the Embedded Config schema resource.");
				}
				return configSchema;
			}
		}

		public static string CustomizationSchema
		{
			get
			{
				if (customizationSchema == string.Empty)
				{
					string[] manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
					TraceLogger.LogMessage(TraceLevel.Info, "Looking for Customization schema in resource list:");
					string[] array = manifestResourceNames;
					foreach (string text in array)
					{
						TraceLogger.LogMessage(TraceLevel.Info, text);
						if (text.EndsWith("Customization.xsd", StringComparison.OrdinalIgnoreCase))
						{
							customizationSchema = text;
							return customizationSchema;
						}
					}
					throw new SystemException("Could not find the Embedded Customization schema resource.");
				}
				return customizationSchema;
			}
		}

		public static string RegistrySchema
		{
			get
			{
				if (registrySchema == string.Empty)
				{
					string[] manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
					TraceLogger.LogMessage(TraceLevel.Info, "Looking for Registry schema in resource list:");
					string[] array = manifestResourceNames;
					foreach (string text in array)
					{
						TraceLogger.LogMessage(TraceLevel.Info, text);
						if (text.EndsWith("Registry.xsd", StringComparison.OrdinalIgnoreCase))
						{
							registrySchema = text;
							return registrySchema;
						}
					}
					throw new SystemException("Could not find the Embedded Registry schema resource.");
				}
				return registrySchema;
			}
		}

		public static Stream PkgGenCfgXml
		{
			get
			{
				if (pkgGenCfgXml == null)
				{
					pkgGenCfgXml = Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Program).Namespace + ".pkggen.cfg.xml");
				}
				return pkgGenCfgXml;
			}
		}

		public static string CustomizationIncludeDirectory
		{
			get
			{
				return customizationIncludeDirectory;
			}
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (value.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
					{
						customizationIncludeDirectory = value;
					}
					else
					{
						customizationIncludeDirectory = value + "\\";
					}
				}
			}
		}

		public static List<XmlFile> CustomizationFiles
		{
			get
			{
				return customizationFiles;
			}
			set
			{
				customizationFiles = value;
			}
		}

		public static List<XmlFile> ConfigFiles
		{
			get
			{
				return configFiles;
			}
			set
			{
				configFiles = value;
			}
		}

		public static bool WarnOnMappingNotFound
		{
			get
			{
				return warnOnMappingNotFound;
			}
			set
			{
				warnOnMappingNotFound = value;
			}
		}

		public static bool Diagnostics
		{
			get
			{
				return diagnostics;
			}
			set
			{
				diagnostics = value;
				if (diagnostics)
				{
					TraceLogger.TraceLevel = TraceLevel.Info;
				}
			}
		}

		public static string OutputDirectoryPath
		{
			get
			{
				return outputDirectoryPath;
			}
			set
			{
				outputDirectoryPath = Environment.ExpandEnvironmentVariables(value);
			}
		}

		public static string OutputPkgFilePath
		{
			get
			{
				if (OutputDirectoryPath.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
				{
					return Path.GetFullPath(OutputDirectoryPath);
				}
				return Path.GetFullPath(OutputDirectoryPath) + "\\";
			}
			set
			{
				TraceLogger.LogMessage(TraceLevel.Warn, "Trying to set output package filename to '" + value + "'. Ignoring because pkg filenames are fixed.");
			}
		}

		public static string TempDirectoryPath
		{
			get
			{
				if (string.IsNullOrEmpty(tempDirectoryPath))
				{
					tempDirectoryPath = FileUtils.GetTempDirectory();
					TraceLogger.LogMessage(TraceLevel.Info, "Temp Directory: " + tempDirectoryPath);
				}
				return tempDirectoryPath;
			}
		}

		public static string MergeFilePath
		{
			get
			{
				if (string.IsNullOrEmpty(mergeFilePath))
				{
					mergeFilePath = FileUtils.GetTempFile(TempDirectoryPath);
					TraceLogger.LogMessage(TraceLevel.Info, "Merge file: " + mergeFilePath);
				}
				return mergeFilePath;
			}
		}
	}
}
