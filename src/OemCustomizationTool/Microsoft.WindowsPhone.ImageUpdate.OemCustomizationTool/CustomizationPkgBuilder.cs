using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class CustomizationPkgBuilder
	{
		public const string c_strOemCustomizationToolRegkeys = "OemCustomizationToolRegkeys";

		public const string c_strRegKeys = "RegKeys";

		public const string c_strRegKey = "RegKey";

		public const string c_strKeyName = "KeyName";

		public const string c_strRegValue = "RegValue";

		public const string c_strName = "Name";

		public const string c_strValue = "Value";

		public const string c_strType = "Type";

		private Customization customization;

		private Configuration config;

		private RegFileHandler regFileHandler;

		public CustomizationPkgBuilder(Customization cust, Configuration conf)
		{
			customization = cust;
			config = conf;
			regFileHandler = new RegFileHandler();
		}

		internal void GenerateCustomizationPackage()
		{
			try
			{
				foreach (RegFilePartitionInfo item in GenerateCustomizationRegFiles())
				{
					using (IPkgBuilder pkgBuilder = Package.Create())
					{
						pkgBuilder.Owner = Settings.PackageAttributes.Owner;
						pkgBuilder.OwnerType = Settings.PackageAttributes.OwnerType;
						pkgBuilder.Component = item.partition + ".ImageCustomization";
						pkgBuilder.SubComponent = "RegistryCustomization";
						pkgBuilder.Partition = item.partition;
						pkgBuilder.ReleaseType = Settings.PackageAttributes.ReleaseType;
						pkgBuilder.CpuType = Settings.PackageAttributes.CpuType;
						pkgBuilder.Version = Settings.PackageAttributes.Version;
						pkgBuilder.BuildType = Settings.PackageAttributes.BuildType;
						pkgBuilder.AddFile(FileType.Registry, item.regFilename, Settings.PackageAttributes.DeviceRegFilePath + "OEMSettings.reg", FileAttributes.System, null);
						foreach (XmlFile xmlFile in customization.XmlFiles)
						{
							pkgBuilder.AddFile(FileType.Regular, xmlFile.Filename, Settings.PackageAttributes.DeviceLogFilePath + Path.GetFileName(xmlFile.Filename), FileAttributes.System, null);
						}
						foreach (XmlFile xmlFile2 in config.XmlFiles)
						{
							pkgBuilder.AddFile(FileType.Regular, xmlFile2.Filename, Settings.PackageAttributes.DeviceLogFilePath + Path.GetFileName(xmlFile2.Filename), FileAttributes.System, null);
						}
						TraceLogger.LogMessage(TraceLevel.Info, "Output Directory: " + Settings.OutputPkgFilePath);
						TraceLogger.LogMessage(TraceLevel.Info, "Output File Name: " + pkgBuilder.Name + PkgConstants.c_strPackageExtension);
						string cabPath = Path.Combine(Path.GetFullPath(Settings.OutputPkgFilePath), pkgBuilder.Name + PkgConstants.c_strPackageExtension);
						pkgBuilder.SaveCab(cabPath);
					}
				}
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Error, ex.ToString());
			}
		}

		private List<RegFilePartitionInfo> GenerateCustomizationRegFiles()
		{
			XDocument customizationXmlDoc = customization.CustomizationXmlDoc;
			XDocument configXmlDoc = config.ConfigXmlDoc;
			IEnumerable<XElement> enumerable = customizationXmlDoc.Descendants("Component");
			foreach (XElement item in enumerable)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item.ToString());
			}
			foreach (string item2 in enumerable.Select((XElement comp) => comp.Attribute("ComponentName").Value))
			{
				TraceLogger.LogMessage(TraceLevel.Info, item2.ToString());
			}
			IEnumerable<XElement> enumerable2 = enumerable.Descendants("SettingsGroup");
			foreach (XElement item3 in enumerable2)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item3.ToString());
			}
			foreach (var custSetting4 in from custSetting in enumerable2.Descendants("Setting")
				select new
				{
					Name = custSetting.Attribute("Name").Value,
					Value = custSetting.Attribute("Value").Value,
					CustomValueName = ((custSetting.Attribute("CustomName") == null) ? string.Empty : custSetting.Attribute("CustomName").Value),
					CustomKeyName = ((custSetting.Parent.Attribute("Key") == null) ? string.Empty : custSetting.Parent.Attribute("Key").Value),
					Component = custSetting.Ancestors("Component").First().Attribute("ComponentName")
						.Value,
					Partition = (string)custSetting.Parent.Attribute("Partition")
				})
			{
				var enumerable3 = from confSetting in configXmlDoc.Descendants("SettingMapping")
					where confSetting.Attribute("Name").Value == custSetting4.Name && confSetting.Ancestors("ComponentMapping").First().Attribute("ComponentName")
						.Value == custSetting4.Component && (string)confSetting.Parent.Attribute("Partition") == custSetting4.Partition && confSetting.Attribute("RegKeyName") != null && confSetting.Attribute("RegName") != null
					select new
					{
						RegKeyName = confSetting.Attribute("RegKeyName").Value,
						RegName = ((confSetting.Attribute("RegName") != null) ? confSetting.Attribute("RegName").Value : confSetting.Attribute("Name").Value),
						RegType = confSetting.Attribute("RegType").Value,
						Partition = custSetting4.Partition
					};
				if (enumerable3 == null || enumerable3.Count() == 0)
				{
					if (custSetting4.CustomValueName.Equals(string.Empty) && custSetting4.CustomKeyName.Equals(string.Empty))
					{
						string text = "There is no config entry for the specified customization: CustSettingName = " + custSetting4.Name + "; CustSettingValue = " + custSetting4.Value + ". Are you sure this customization is allowed?";
						if (!Settings.WarnOnMappingNotFound)
						{
							throw new ConfigXmlException(text);
						}
						TraceLogger.LogMessage(TraceLevel.Warn, text);
					}
					continue;
				}
				foreach (var item4 in enumerable3.Distinct())
				{
					TraceLogger.LogMessage(TraceLevel.Info, "CustSettingName = " + custSetting4.Name + "; CustSettingValue = " + custSetting4.Value + "; ConfRegKeyName = " + item4.RegKeyName + "; ConfRegName = " + item4.RegName + "; ConfRegType = " + item4.RegType);
					RegValueInfo regValueInfo = new RegValueInfo
					{
						KeyName = item4.RegKeyName,
						ValueName = item4.RegName,
						Value = custSetting4.Value
					};
					regValueInfo.SetRegValueType(item4.RegType);
					regFileHandler.AddRegValue(regValueInfo, item4.Partition);
				}
			}
			foreach (var custSetting3 in from setting in enumerable2.Descendants("Setting")
				where setting.Attribute("CustomName") != null
				select new
				{
					Name = setting.Attribute("Name").Value,
					Value = setting.Attribute("Value").Value,
					CustomValueName = setting.Attribute("CustomName").Value,
					CustomKeyName = ((setting.Parent.Attribute("Key") == null) ? string.Empty : setting.Parent.Attribute("Key").Value),
					Component = setting.Ancestors("Component").First().Attribute("ComponentName")
						.Value,
					Partition = (string)setting.Parent.Attribute("Partition")
				})
			{
				var enumerable4 = from confSetting in configXmlDoc.Descendants("SettingMapping")
					where confSetting.Attribute("Name").Value == custSetting3.Name && confSetting.Ancestors("ComponentMapping").First().Attribute("ComponentName")
						.Value == custSetting3.Component && (string)confSetting.Parent.Attribute("Partition") == custSetting3.Partition && confSetting.Attribute("RegKeyName") != null && confSetting.Attribute("RegName") == null
					select new
					{
						RegKeyName = confSetting.Attribute("RegKeyName").Value,
						RegName = custSetting3.CustomValueName,
						RegType = confSetting.Attribute("RegType").Value,
						Partition = custSetting3.Partition
					};
				if (enumerable4 == null || enumerable4.Count() == 0)
				{
					if (custSetting3.CustomKeyName.Equals(string.Empty))
					{
						string text2 = "There is no config entry for the specified customization: CustSettingName = " + custSetting3.Name + "; CustSettingValue = " + custSetting3.Value + ". Are you sure this customization is allowed?";
						if (!Settings.WarnOnMappingNotFound)
						{
							throw new ConfigXmlException(text2);
						}
						TraceLogger.LogMessage(TraceLevel.Warn, text2);
					}
					continue;
				}
				foreach (var item5 in enumerable4.Distinct())
				{
					TraceLogger.LogMessage(TraceLevel.Info, "CustSettingName = " + custSetting3.Name + "; CustSettingValue = " + custSetting3.Value + "; ConfRegKeyName = " + item5.RegKeyName + "; ConfRegName = " + item5.RegName + "; ConfRegType = " + item5.RegType);
					RegValueInfo regValueInfo2 = new RegValueInfo
					{
						KeyName = item5.RegKeyName,
						ValueName = item5.RegName,
						Value = custSetting3.Value
					};
					regValueInfo2.SetRegValueType(item5.RegType);
					regFileHandler.AddRegValue(regValueInfo2, item5.Partition);
				}
			}
			foreach (var custSetting2 in from setting in enumerable2.Descendants("Setting")
				where setting.Parent.Attribute("Key") != null
				select new
				{
					Name = setting.Attribute("Name").Value,
					Value = setting.Attribute("Value").Value,
					CustomKeyName = setting.Parent.Attribute("Key").Value,
					Component = setting.Ancestors("Component").First().Attribute("ComponentName")
						.Value,
					Partition = (string)setting.Parent.Attribute("Partition")
				})
			{
				var enumerable5 = from confSetting in configXmlDoc.Descendants("SettingMapping")
					where confSetting.Attribute("Name").Value == custSetting2.Name && confSetting.Ancestors("ComponentMapping").First().Attribute("ComponentName")
						.Value == custSetting2.Component && (string)confSetting.Parent.Attribute("Partition") == custSetting2.Partition && confSetting.Parent.Attribute("RegKeyBaseName") != null && confSetting.Parent.Attribute("RegKeyBaseName").Value.Contains("%%KEY%%") && confSetting.Attribute("RegKeyName") == null
					select new
					{
						RegKeyName = confSetting.Parent.Attribute("RegKeyBaseName").Value.Replace("%%KEY%%", custSetting2.CustomKeyName),
						RegName = ((confSetting.Attribute("RegName") != null) ? confSetting.Attribute("RegName").Value : confSetting.Attribute("Name").Value),
						RegType = confSetting.Attribute("RegType").Value,
						Partition = custSetting2.Partition
					};
				if (enumerable5 == null || enumerable5.Count() == 0)
				{
					string text3 = "There is no config entry for the specified customization: CustSettingName = " + custSetting2.Name + "; CustSettingValue = " + custSetting2.Value + ". Are you sure this customization is allowed?";
					if (Settings.WarnOnMappingNotFound)
					{
						TraceLogger.LogMessage(TraceLevel.Warn, text3);
						continue;
					}
					throw new ConfigXmlException(text3);
				}
				foreach (var item6 in enumerable5.Distinct())
				{
					TraceLogger.LogMessage(TraceLevel.Info, "CustSettingName = " + custSetting2.Name + "; CustSettingValue = " + custSetting2.Value + "; ConfRegKeyName = " + item6.RegKeyName + "; ConfRegName = " + item6.RegName + "; ConfRegType = " + item6.RegType);
					RegValueInfo regValueInfo3 = new RegValueInfo
					{
						KeyName = item6.RegKeyName,
						ValueName = item6.RegName,
						Value = custSetting2.Value
					};
					regValueInfo3.SetRegValueType(item6.RegType);
					regFileHandler.AddRegValue(regValueInfo3, item6.Partition);
				}
			}
			return regFileHandler.Write();
		}
	}
}
