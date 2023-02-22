using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Phone.TestInfra
{
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("0.14")]
		public string Version
		{
			get
			{
				return (string)this["Version"];
			}
			set
			{
				this["Version"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("logs")]
		public string DefaultLogDirectory
		{
			get
			{
				return (string)this["DefaultLogDirectory"];
			}
			set
			{
				this["DefaultLogDirectory"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("DeployTest.log")]
		public string DefaultLogName
		{
			get
			{
				return (string)this["DefaultLogName"];
			}
			set
			{
				this["DefaultLogName"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("24")]
		public int DefaultFileExpiresInHours
		{
			get
			{
				return (int)this["DefaultFileExpiresInHours"];
			}
			set
			{
				this["DefaultFileExpiresInHours"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("testdeployroot=\\data\\test;testshare=\\\\winphonelabs\\securestorage\\Blue\\TestData;multimediashare=\\\\wdcmmcontent\\blue; testlabconfigdirectory=Internal;OSGTestContentShare=\\\\redmond\\1windows\\testcontent")]
		public string DefaultMacros
		{
			get
			{
				return (string)this["DefaultMacros"];
			}
			set
			{
				this["DefaultMacros"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3600000")]
		public int CacheTimeoutInMs
		{
			get
			{
				return (int)this["CacheTimeoutInMs"];
			}
			set
			{
				this["CacheTimeoutInMs"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("1")]
		public int CacheExpirationInDays
		{
			get
			{
				return (int)this["CacheExpirationInDays"];
			}
			set
			{
				this["CacheExpirationInDays"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("4")]
		public int MaxConcurrentDownloads
		{
			get
			{
				return (int)this["MaxConcurrentDownloads"];
			}
			set
			{
				this["MaxConcurrentDownloads"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("8")]
		public int MaxConcurrentLocalCopies
		{
			get
			{
				return (int)this["MaxConcurrentLocalCopies"];
			}
			set
			{
				this["MaxConcurrentLocalCopies"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3")]
		public int CopyRetryCount
		{
			get
			{
				return (int)this["CopyRetryCount"];
			}
			set
			{
				this["CopyRetryCount"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3000")]
		public int CopyRetryDelayInMs
		{
			get
			{
				return (int)this["CopyRetryDelayInMs"];
			}
			set
			{
				this["CopyRetryDelayInMs"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("96")]
		public int MaxConcurrentReaders
		{
			get
			{
				return (int)this["MaxConcurrentReaders"];
			}
			set
			{
				this["MaxConcurrentReaders"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("Microsoft.Phone.CacheManager.DownloadSemaphoreName")]
		public string DownloadSemaphoreName
		{
			get
			{
				return (string)this["DownloadSemaphoreName"];
			}
			set
			{
				this["DownloadSemaphoreName"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("Microsoft.Phone.CacheManager.LocalCopySemaphoreName")]
		public string LocalCopySemaphoreName
		{
			get
			{
				return (string)this["LocalCopySemaphoreName"];
			}
			set
			{
				this["LocalCopySemaphoreName"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("\\\\winbuilds\\release\\")]
		public string WinBuildSharePrefix
		{
			get
			{
				return (string)this["WinBuildSharePrefix"];
			}
			set
			{
				this["WinBuildSharePrefix"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("\\\\build\\release\\")]
		public string PhoneBuildSharePrefix
		{
			get
			{
				return (string)this["PhoneBuildSharePrefix"];
			}
			set
			{
				this["PhoneBuildSharePrefix"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("3")]
		public int ShareAccessRetryCount
		{
			get
			{
				return (int)this["ShareAccessRetryCount"];
			}
			set
			{
				this["ShareAccessRetryCount"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("1000")]
		public int ShareAccessRetryDelayInMs
		{
			get
			{
				return (int)this["ShareAccessRetryDelayInMs"];
			}
			set
			{
				this["ShareAccessRetryDelayInMs"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("bldinfo;sdp")]
		public string ExcludedSubDirsUnderPrebuiltPath
		{
			get
			{
				return (string)this["ExcludedSubDirsUnderPrebuiltPath"];
			}
			set
			{
				this["ExcludedSubDirsUnderPrebuiltPath"] = value;
			}
		}

		[UserScopedSetting]
		[DebuggerNonUserCode]
		[DefaultSettingValue("30")]
		public int LocationCacheExpiresInDays
		{
			get
			{
				return (int)this["LocationCacheExpiresInDays"];
			}
			set
			{
				this["LocationCacheExpiresInDays"] = value;
			}
		}
	}
}
