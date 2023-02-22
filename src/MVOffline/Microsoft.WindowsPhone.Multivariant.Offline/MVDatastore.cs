using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	public class MVDatastore
	{
		private class ProvXmlInfo
		{
			private Tuple<MVCondition, string, string, string> t;

			public MVCondition condition => t.Item1;

			public string settingsGroupPath => t.Item2;

			public string productID => t.Item3;

			public string provXmlPath => t.Item4;

			public ProvXmlInfo(MVCondition condition, string settingsGroupPath, string productID, string provXmlPath)
			{
				t = new Tuple<MVCondition, string, string, string>(condition, settingsGroupPath, productID, provXmlPath);
			}
		}

		private class AppInfo
		{
			private Tuple<MVCondition, Guid, XElement> t;

			public MVCondition condition => t.Item1;

			public Guid productID => t.Item2;

			public XElement provXML => t.Item3;

			public AppInfo(MVCondition condition, Guid productID, XElement provXML)
			{
				t = new Tuple<MVCondition, Guid, XElement>(condition, productID, provXML);
			}
		}

		private const string c_strPkgGenCfgXmlName = "PkgGen.cfg.xml";

		public const string MasterFileName = "MasterDatastore.xml";

		public const string DefaultDatastoreRoot = "\\Programs\\CommonFiles\\ADC\\Microsoft\\";

		public const string DatastoreRegKey = "$(hklm.software)\\Microsoft\\Multivariant";

		public const string ADCRegKey = "$(hklm.software)\\Microsoft\\ADC";

		public const string DatastoreCabFileName = "ProvisionData.cab";

		public const string CriticalSettingsCabFileName = "ProvisionDataCriticalSettings.cab";

		public const string StaticProvXMLPrefix = "static_settings_group";

		public const string MxipUpdatePrefix = "mxipupdate";

		public const string DefaultDatastoreRootWithMacro = "$(_adc)\\Microsoft\\";

		public const string DefaultCabstoreRootWithMacro = "$(_provisionCab)\\";

		public const string DefaultCriticalCabstoreRootWithMacro = "$(_provisionCriticalSettingsCab)\\";

		public const string StaticDatastoreRoot = "\\Programs\\PhoneProvisioner_OEM\\OEM\\";

		public const string MxipUpdateRoot = "\\Windows\\System32\\Migrators\\DuMigrationProvisionerOEM\\provxml";

		public const char MultiStringSeparator = '\uf000';

		public const string MVMultiStringSeparator = "&#xF000;";

		private const string ProvXmlExtension = ".provxml";

		private const string ApplicationSourceName = "Applications";

		private const string ApplicationSourceFile = "Applications.xml";

		private const string DatastoreVersion = "1.0";

		private const string ProvisioningDocumentRootName = "wap-provisioningdoc";

		public List<MVVariant> Variants { get; private set; }

		public static IEnumerable<RegValueInfo> GetDefaultDatastoreRegistration(bool provisionCab, bool criticalCab)
		{
			RegValueInfo regValueInfo = new RegValueInfo();
			regValueInfo.Type = RegValueType.String;
			regValueInfo.KeyName = "$(hklm.software)\\Microsoft\\Multivariant";
			regValueInfo.ValueName = "DatastoreRoot";
			regValueInfo.Value = Path.Combine("\\Programs\\CommonFiles\\ADC\\Microsoft\\", "MasterDatastore.xml");
			yield return regValueInfo;
			regValueInfo = new RegValueInfo();
			regValueInfo.Type = RegValueType.DWord;
			regValueInfo.KeyName = "$(hklm.software)\\Microsoft\\Multivariant";
			regValueInfo.ValueName = "Enable";
			regValueInfo.Value = "1";
			yield return regValueInfo;
			regValueInfo = new RegValueInfo();
			regValueInfo.Type = RegValueType.DWord;
			regValueInfo.KeyName = "$(hklm.software)\\Microsoft\\ADC";
			regValueInfo.ValueName = "RunADC";
			regValueInfo.Value = "0";
			yield return regValueInfo;
			if (provisionCab)
			{
				regValueInfo = new RegValueInfo();
				regValueInfo.Type = RegValueType.String;
				regValueInfo.KeyName = "$(hklm.software)\\Microsoft\\Multivariant";
				regValueInfo.ValueName = "ProvisionDataCABPath";
				regValueInfo.Value = Path.Combine("\\Programs\\CommonFiles\\ADC\\Microsoft\\", "ProvisionData.cab");
				yield return regValueInfo;
			}
			if (criticalCab)
			{
				regValueInfo = new RegValueInfo();
				regValueInfo.Type = RegValueType.String;
				regValueInfo.KeyName = "$(hklm.software)\\Microsoft\\Multivariant";
				regValueInfo.ValueName = "ProvisionDataCriticalSettingsCABPath";
				regValueInfo.Value = Path.Combine("\\Programs\\CommonFiles\\ADC\\Microsoft\\", "ProvisionDataCriticalSettings.cab");
				yield return regValueInfo;
			}
		}

		public MVDatastore()
		{
			Variants = new List<MVVariant>();
		}

		private static bool Is_DeviceStatic(MVSetting setting)
		{
			if (setting.ProvisioningTime == MVSettingProvisioning.Static)
			{
				return string.IsNullOrWhiteSpace(setting.RegistryKey);
			}
			return false;
		}

		private static bool Is_UiccConnectivity(MVCondition condition)
		{
			return condition.IsValidCondition();
		}

		private static bool Is_UiccConnectivity(MVSetting setting)
		{
			return setting.ProvisioningTime == MVSettingProvisioning.Connectivity;
		}

		private static bool Is_UiccGeneral(MVCondition condition)
		{
			return condition.IsValidCondition();
		}

		private static bool Is_UiccGeneral(MVSetting setting)
		{
			return setting.ProvisioningTime == MVSettingProvisioning.General;
		}

		private static bool Is_UiccRunOnce(MVCondition condition)
		{
			return condition.IsValidCondition();
		}

		private static bool Is_UiccRunOnce(MVSetting setting)
		{
			return setting.ProvisioningTime == MVSettingProvisioning.RunOnce;
		}

		public void SaveStaticDatastore(string tempStaticDatastoreRoot)
		{
			Directory.CreateDirectory(tempStaticDatastoreRoot);
			WriteStaticSettings(tempStaticDatastoreRoot, (MVSetting setting) => Is_DeviceStatic(setting));
		}

		public void SaveDatastore(string tempDatastoreRoot, string tempProvisioningOutputRoot, string tempCriticalDatastoreRoot)
		{
			Directory.CreateDirectory(tempDatastoreRoot);
			Directory.CreateDirectory(tempProvisioningOutputRoot);
			Directory.CreateDirectory(tempCriticalDatastoreRoot);
			IEnumerable<KeyValuePair<VariantEvent, string>> sources = WriteDataStoreByEvent(tempProvisioningOutputRoot, tempCriticalDatastoreRoot);
			string outputPath = Path.Combine(tempDatastoreRoot, "MasterDatastore.xml");
			WriteSourceListXml(sources, outputPath, "$(_adc)\\Microsoft\\");
		}

		public IEnumerable<RegFilePartitionInfo> SaveShadowRegistry(string shadowRegRoot, IEnumerable<RegValueInfo> datastoreRegEntries)
		{
			if (!Directory.Exists(shadowRegRoot))
			{
				Directory.CreateDirectory(shadowRegRoot);
			}
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PkgGen.cfg.xml");
			RegFileHandler regFileHandler = new RegFileHandler(shadowRegRoot, manifestResourceStream);
			IEnumerable<RegValueInfo> staticRegistryValues = GetStaticRegistryValues();
			foreach (RegValueInfo item in datastoreRegEntries.Concat(staticRegistryValues))
			{
				regFileHandler.AddRegValue(item);
			}
			return regFileHandler.Write();
		}

		private IEnumerable<KeyValuePair<VariantEvent, string>> WriteDataStoreByEvent(string tempProvisioningOutputRoot, string criticalTempDatastoreRoot)
		{
			yield return WriteApplicationProvisioning(tempProvisioningOutputRoot);
			yield return WriteVariantSettings(VariantEvent.Uicc_Connectivity, criticalTempDatastoreRoot, (MVCondition cond) => Is_UiccConnectivity(cond), (MVSetting setting) => Is_UiccConnectivity(setting), "$(_provisionCriticalSettingsCab)\\");
			yield return WriteVariantSettings(VariantEvent.Uicc_RunOnce, tempProvisioningOutputRoot, (MVCondition cond) => Is_UiccRunOnce(cond), (MVSetting setting) => Is_UiccRunOnce(setting), "$(_provisionCab)\\");
			yield return WriteVariantSettings(VariantEvent.Uicc_General, tempProvisioningOutputRoot, (MVCondition cond) => Is_UiccGeneral(cond), (MVSetting setting) => Is_UiccGeneral(setting), "$(_provisionCab)\\");
		}

		private KeyValuePair<VariantEvent, string> WriteStaticSettings(string tempDatastoreRoot, Func<MVSetting, bool> settingPredicate)
		{
			if (!Variants.Any())
			{
				return new KeyValuePair<VariantEvent, string>(VariantEvent.Device_Static, null);
			}
			int num = 0;
			Func<MVSetting, bool> func = default(Func<MVSetting, bool>);
			foreach (MVSettingGroup settingsGroup in Variants.SingleOrDefault().SettingsGroups)
			{
				List<MVSetting> settings = settingsGroup.Settings;
				Func<MVSetting, bool> func2 = func;
				if (func2 == null)
				{
					func2 = (func = (MVSetting x) => settingPredicate(x));
				}
				IEnumerable<MVSetting> enumerable = settings.Where(func2);
				if (enumerable.Count() != 0)
				{
					string arg = formatPathForFileName(settingsGroup.Path);
					string path = string.Format("{0}_{1}_{2}.provxml", "static_settings_group", arg, num);
					num++;
					string outputPath = Path.Combine(tempDatastoreRoot, path);
					WriteProvXml(enumerable, outputPath);
				}
			}
			return new KeyValuePair<VariantEvent, string>(VariantEvent.Device_Static, null);
		}

		private KeyValuePair<VariantEvent, string> WriteVariantSettings(VariantEvent variantEvent, string tempDatastoreRoot, Func<MVCondition, bool> conditionPredicate, Func<MVSetting, bool> settingPredicate, string cabstore)
		{
			string text = null;
			int num = 0;
			List<ProvXmlInfo> list = new List<ProvXmlInfo>();
			Func<MVCondition, bool> func = default(Func<MVCondition, bool>);
			Func<MVSetting, bool> func3 = default(Func<MVSetting, bool>);
			foreach (MVVariant variant in Variants)
			{
				List<MVCondition> conditions = variant.Conditions;
				Func<MVCondition, bool> func2 = func;
				if (func2 == null)
				{
					func2 = (func = (MVCondition x) => conditionPredicate(x));
				}
				IEnumerable<MVCondition> source = conditions.Where(func2);
				if (source.Count() == 0)
				{
					continue;
				}
				foreach (IGrouping<string, MVSettingGroup> group in from x in variant.SettingsGroups
					group x by x.PolicyPath)
				{
					IEnumerable<MVSetting> source2 = group.SelectMany((MVSettingGroup x) => x.Settings);
					Func<MVSetting, bool> func4 = func3;
					if (func4 == null)
					{
						func4 = (func3 = (MVSetting x) => settingPredicate(x));
					}
					IEnumerable<MVSetting> enumerable = source2.Where(func4);
					if (enumerable.Count() != 0)
					{
						string arg = formatPathForFileName(group.Key);
						string path = $"{variantEvent.ToString()}_{arg}_{num}.provxml";
						num++;
						string provxmlPath = Path.Combine(tempDatastoreRoot, path);
						WriteProvXml(enumerable, provxmlPath);
						list.AddRange(source.Select((MVCondition x) => new ProvXmlInfo(x, group.Key, null, provxmlPath)));
					}
				}
			}
			if (list.Count == 0)
			{
				return new KeyValuePair<VariantEvent, string>(variantEvent, null);
			}
			text = Path.Combine(tempDatastoreRoot, variantEvent.ToString() + ".xml");
			WriteSourceXml(list, variantEvent.ToString(), text, cabstore);
			return new KeyValuePair<VariantEvent, string>(variantEvent, text);
		}

		private string formatPathForFileName(string original)
		{
			string text = original;
			string text2 = text;
			for (int i = 0; i < text2.Length; i++)
			{
				char c = text2[i];
				if (c > '~' || !char.IsLetterOrDigit(c))
				{
					text = text.Replace(c.ToString(), "");
				}
			}
			if (text.Length > 40)
			{
				text = text.Substring(0, 40);
			}
			return text;
		}

		private IEnumerable<AppInfo> ApplicationsCartesian()
		{
			foreach (MVVariant variant in Variants)
			{
				IEnumerable<AppInfo> enumerable = variant.Conditions.SelectMany((MVCondition x) => variant.Applications.Select((KeyValuePair<Guid, XElement> y) => new AppInfo(x, y.Key, y.Value)));
				foreach (AppInfo item in enumerable)
				{
					yield return item;
				}
			}
		}

		private KeyValuePair<VariantEvent, string> WriteApplicationProvisioning(string tempDatastoreRoot)
		{
			if (!Variants.SelectMany((MVVariant x) => x.Conditions).Any())
			{
				return new KeyValuePair<VariantEvent, string>(VariantEvent.Uicc_Apps, null);
			}
			IEnumerable<IGrouping<XElement, AppInfo>> enumerable = from x in ApplicationsCartesian()
				group x by x.provXML;
			if (enumerable.Count() == 0)
			{
				return new KeyValuePair<VariantEvent, string>(VariantEvent.Uicc_Apps, null);
			}
			List<ProvXmlInfo> list = new List<ProvXmlInfo>();
			foreach (IGrouping<XElement, AppInfo> item in enumerable)
			{
				XElement key = item.Key;
				string text = Guid.NewGuid().ToString() + ".provxml";
				string text2 = Path.Combine(tempDatastoreRoot, text);
				if (!File.Exists(text2))
				{
					key.Save(text2);
				}
				foreach (MVCondition item2 in item.Select((AppInfo x) => x.condition))
				{
					list.Add(new ProvXmlInfo(item2, null, item.First().productID.ToString(), text));
				}
			}
			string text3 = Path.Combine(tempDatastoreRoot, "Applications.xml");
			WriteSourceXml(list, "Applications", text3, "$(_provisionCab)\\");
			return new KeyValuePair<VariantEvent, string>(VariantEvent.Uicc_Apps, text3);
		}

		private void WriteSourceListXml(IEnumerable<KeyValuePair<VariantEvent, string>> sources, string outputPath, string devicePath)
		{
			XElement xElement = new XElement("ConfigurationSourceList");
			xElement.Add(new XAttribute("Version", "1.0"));
			foreach (KeyValuePair<VariantEvent, string> source in sources)
			{
				if (source.Value != null)
				{
					XElement xElement2 = new XElement("ConfigurationSource");
					xElement2.Add(new XAttribute("Name", Path.GetFileNameWithoutExtension(source.Value)));
					xElement2.Add(new XAttribute("Path", Path.Combine(devicePath, Path.GetFileName(source.Value))));
					XElement xElement3 = new XElement("EventList");
					XElement xElement4 = new XElement("Event");
					xElement4.Add(new XAttribute("Name", source.Key.ToString()));
					xElement3.Add(xElement4);
					xElement2.Add(xElement3);
					xElement.Add(xElement2);
				}
			}
			xElement.Save(outputPath);
		}

		private void WriteSourceXml(IEnumerable<ProvXmlInfo> provXmls, string eventName, string outputPath, string devicePath)
		{
			XElement xElement = new XElement("ConfigurationSource");
			xElement.Add(new XAttribute("Version", "1.0"));
			foreach (ProvXmlInfo provXml in provXmls)
			{
				XElement xElement2 = new XElement("ConfigurationSet");
				xElement2.Add(new XAttribute("Type", "provxml"));
				if (provXml.settingsGroupPath != null)
				{
					xElement2.Add(new XAttribute("SettingsGroup", provXml.settingsGroupPath));
				}
				if (provXml.productID != null)
				{
					xElement2.Add(new XAttribute("ProductID", provXml.productID));
				}
				xElement2.Add(new XAttribute("Data", Path.Combine(devicePath, Path.GetFileName(provXml.provXmlPath))));
				if (provXml.condition != null)
				{
					MVCondition condition = provXml.condition;
					foreach (string key in condition.KeyValues.Keys)
					{
						if (!key.Equals("UIOrder", StringComparison.OrdinalIgnoreCase) || eventName.Equals(VariantEvent.Uicc_Connectivity.ToString()))
						{
							if (condition.KeyValues[key].IsWildCard)
							{
								xElement2.Add(new XAttribute(key.ToLower(CultureInfo.InvariantCulture), "*"));
							}
							else
							{
								xElement2.Add(new XAttribute(key.ToLower(CultureInfo.InvariantCulture), condition.KeyValues[key].KeyValue));
							}
						}
					}
				}
				xElement.Add(xElement2);
			}
			xElement.Save(outputPath);
		}

		private void WriteProvXml(IEnumerable<MVSetting> settings, string outputPath)
		{
			XElement xElement = new XElement("wap-provisioningdoc");
			foreach (MVSetting setting in settings)
			{
				if (setting.DataType.Equals("multiplestring"))
				{
					setting.Value = setting.Value.Replace(";", "&#xF000;");
					string value = "&#xF000;&#xF000;";
					while (!setting.Value.EndsWith(value, StringComparison.OrdinalIgnoreCase))
					{
						setting.Value += "&#xF000;";
					}
				}
				XElement xElement2 = new XElement("parm");
				xElement2.Add(new XAttribute("name", setting.ProvisioningPath.Last()));
				xElement2.Add(new XAttribute("value", setting.Value));
				xElement2.Add(new XAttribute("datatype", setting.DataType));
				List<string> list = setting.ProvisioningPath.ToList();
				list.RemoveAt(list.Count() - 1);
				XElement xElement3 = xElement;
				foreach (string item in list)
				{
					XElement xElement4 = xElement3.XPathSelectElement($"characteristic[@type=\"{item}\"]");
					if (xElement4 == null)
					{
						xElement4 = new XElement("characteristic");
						xElement4.Add(new XAttribute("type", item));
						xElement3.Add(xElement4);
					}
					xElement3 = xElement4;
				}
				xElement3.Add(xElement2);
			}
			new XDocument(new XDeclaration("1.0", "utf-8", "yes"), xElement).Save(outputPath);
			string text = File.ReadAllText(outputPath);
			text = text.Replace("&amp;#xF000;", "&#xF000;");
			File.WriteAllText(outputPath, text);
		}

		private IEnumerable<RegValueInfo> GetStaticRegistryValues()
		{
			if (!Variants.Any())
			{
				yield break;
			}
			IEnumerable<MVSetting> enumerable = from x in Variants.Single().SettingsGroups.SelectMany((MVSettingGroup x) => x.Settings)
				where !string.IsNullOrWhiteSpace(x.RegistryKey)
				select x;
			foreach (MVSetting item in enumerable)
			{
				RegValueInfo regValueInfo = new RegValueInfo();
				regValueInfo.SetRegValueType(item.RegType);
				regValueInfo.KeyName = item.RegistryKey;
				regValueInfo.ValueName = item.RegistryValue;
				regValueInfo.Partition = item.Partition;
				if (regValueInfo.Type == RegValueType.MultiString)
				{
					regValueInfo.Value = item.Value.TrimEnd('\uf000').Replace('\uf000', ';');
				}
				else if (regValueInfo.Type == RegValueType.DWord)
				{
					regValueInfo.Value = int.Parse(item.Value).ToString("X8");
				}
				else
				{
					regValueInfo.Value = item.Value;
				}
				yield return regValueInfo;
			}
		}
	}
}
