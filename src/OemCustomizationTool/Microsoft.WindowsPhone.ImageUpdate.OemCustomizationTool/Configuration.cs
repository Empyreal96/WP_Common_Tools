using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	internal class Configuration
	{
		private class SettingMappingComparer : IEqualityComparer<XElement>
		{
			public bool Equals(XElement x, XElement y)
			{
				bool flag = false;
				bool flag2 = false;
				if (x.Attribute("RegKeyName") != null && y.Attribute("RegKeyName") != null)
				{
					flag2 = x.Attribute("RegKeyName").Value == y.Attribute("RegKeyName").Value;
				}
				else if (x.Ancestors("SettingsGroupMapping").First().Attribute("RegKeyBaseName") != null && y.Ancestors("SettingsGroupMapping").First().Attribute("RegKeyBaseName") != null)
				{
					flag2 = x.Ancestors("SettingsGroupMapping").First().Attribute("RegKeyBaseName")
						.Value == y.Ancestors("SettingsGroupMapping").First().Attribute("RegKeyBaseName")
						.Value;
				}
				else
				{
					flag = false;
				}
				if (flag2)
				{
					flag = ((x.Attribute("RegName") == null || y.Attribute("RegName") == null) ? (x.Attribute("Name").Value == y.Attribute("Name").Value) : (x.Attribute("RegName").Value == y.Attribute("RegName").Value));
				}
				if (!flag && x.Attribute("Name").Value == y.Attribute("Name").Value)
				{
					flag = x.Ancestors("ComponentMapping").First().Attribute("ComponentName")
						.Value == y.Ancestors("ComponentMapping").First().Attribute("ComponentName")
						.Value;
				}
				if (flag)
				{
					string obj = ((x.Ancestors("SettingsGroupMapping").First().Attribute("Partition") != null) ? x.Ancestors("SettingsGroupMapping").First().Attribute("Partition")
						.Value : Settings.PackageAttributes.mainOSPartitionString);
					string text = ((y.Ancestors("SettingsGroupMapping").First().Attribute("Partition") != null) ? y.Ancestors("SettingsGroupMapping").First().Attribute("Partition")
						.Value : Settings.PackageAttributes.mainOSPartitionString);
					flag = obj == text;
				}
				return flag;
			}

			public int GetHashCode(XElement obj)
			{
				return base.GetHashCode();
			}
		}

		public const string c_strMapping = "Mapping";

		public const string c_strSettingMapping = "SettingMapping";

		public const string c_strRegKeyName = "RegKeyName";

		public const string c_strSettingsGroup = "SettingsGroup";

		public const string c_strSettingsGroupMapping = "SettingsGroupMapping";

		public const string c_strRegKeyBaseName = "RegKeyBaseName";

		public const string c_strRegName = "RegName";

		public const string c_strRegType = "RegType";

		public const string c_strName = "Name";

		public const string c_strComponentMapping = "ComponentMapping";

		public const string c_strComponentName = "ComponentName";

		public const string c_strPartition = "Partition";

		private List<XmlFile> xmlFiles;

		private XDocument configXmlDoc;

		public bool IsConfigValid;

		public List<XmlFile> XmlFiles
		{
			get
			{
				return xmlFiles;
			}
			set
			{
				xmlFiles = value;
			}
		}

		public XDocument ConfigXmlDoc
		{
			get
			{
				return configXmlDoc;
			}
			set
			{
				configXmlDoc = value;
			}
		}

		public Configuration(List<XmlFile> files)
		{
			if (files == null || files.Count == 0)
			{
				TraceLogger.LogMessage(TraceLevel.Error, "Received an empty config file list.");
				return;
			}
			try
			{
				List<XmlFile> files2 = files;
				ConfigXmlDoc = XmlFileHandler.LoadXmlDoc(ref files2);
				XmlFiles = files2;
				FindDuplicateEntries();
				IsConfigValid = true;
			}
			catch (Exception ex)
			{
				TraceLogger.LogMessage(TraceLevel.Error, ex.ToString());
			}
		}

		private void FindDuplicateEntries()
		{
			TraceLogger.LogMessage(TraceLevel.Info, "Checking for duplicates in Config files.");
			IEnumerable<XElement> enumerable = configXmlDoc.Descendants("Mapping");
			foreach (XElement item in enumerable)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item.ToString());
			}
			IEnumerable<XElement> enumerable2 = enumerable.Descendants("SettingMapping");
			SettingMappingComparer comparer = new SettingMappingComparer();
			IEnumerable<XElement> enumerable3 = enumerable2.Distinct(comparer);
			foreach (XElement item2 in enumerable3)
			{
				TraceLogger.LogMessage(TraceLevel.Info, item2.ToString());
			}
			if (enumerable2.Count() == enumerable3.Count())
			{
				return;
			}
			IEnumerable<XElement> enumerable4 = enumerable2.Except(enumerable3);
			TraceLogger.LogMessage(TraceLevel.Error, "Found duplicate settings in config:");
			foreach (XElement item3 in enumerable4)
			{
				TraceLogger.LogMessage(TraceLevel.Error, item3.ToString());
			}
			throw new CustomizationXmlException("There are duplicate SettingMapping entries in your config.");
		}
	}
}
