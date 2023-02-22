using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon;
using Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon.BaseComponents;
using Microsoft.WindowsPhone.MCSF.Offline;

namespace Microsoft.WindowsPhone.DeviceManagement.MCSF
{
	public class PkgRegWriter
	{
		private static class Strings
		{
			public static class Elements
			{
				public const string AccessType = "AccessType";

				public const string Asset = "Asset";

				public const string Constraints = "Constraints";

				public const string CspSource = "CspSource";

				public const string MultiStringList = "MultiStringList";

				public const string Option = "Option";

				public const string RegistrySource = "RegistrySource";

				public const string SettingsGroup = "SettingsGroup";

				public const string Setting = "Setting";

				public const string Validate = "Validate";

				public const string ValueList = "ValueList";
			}

			public static class Attributes
			{
				public const string All = "All";

				public const string Create = "Create";

				public const string Default = "Default";

				public const string Delete = "Delete";

				public const string Description = "Description";

				public const string Get = "Get";

				public const string ImageTimeOnly = "ImageTimeOnly";

				public const string Key = "Key";

				public const string Max = "Max";

				public const string MaxLength = "MaxLength";

				public const string Min = "Min";

				public const string MinLength = "MinLength";

				public const string MOKey = "MOKey";

				public const string Name = "Name";

				public const string OEMKey = "OEMKey";

				public const string Path = "Path";

				public const string Replace = "Replace";

				public const string Type = "Type";

				public const string Value = "Value";
			}

			public static class Values
			{
				public const string AllowedAccess = "AllowedAccess";

				public const string Options = "Options";

				public const string Source = "Source";

				public const string SourcePath = "SourcePath";

				public const string SourceType = "SourceType";

				public const string ValidateMax = "ValidateMax";

				public const string ValidateMin = "ValidateMin";

				public const string ValidationMethod = "ValidationMethod";
			}

			public static class Macros
			{
				public const string ICCID = "__ICCID";

				public const string IMSI = "__IMSI";

				public const string CANINDEX = "__CANINDEX";

				public const string MVID = "__MVID";

				public const string SIMSLOT = "__SIMSLOT";

				public const string URISegmentPattern = "^[a-zA-Z0-9]*$";

				public const string MacroPattern = "^\\$\\(.*\\)$";

				public const string NestedPattern = "\\$\\(.*\\)";

				public const string VariableNamePattern = "^[a-zA-Z][a-zA-Z_0-9.]*$";

				public const string MCSFFormat = "~{0}~";
			}

			public static class Policy
			{
				public const string RootNodeName = "CustomizationPolicy";

				public const string FileExtension = "policy.xml";

				public const string LocalPolicyPath = "CustomizationPolicy";

				public const string DevicePolicyPath = "$(runtime.windows)\\CustomizationPolicy";
			}

			public static class Messages
			{
				public const string DefaultOnMultiSetting = "error: Multi-settings may not supply a default value, as they do not point to an explicit registry location.";

				public const string DuplicateName = "error: The asset or setting name \"{0}\" has already been used within this Settings Group.";

				public const string FirstSegmentMacro = "error: When specifying a Multi-Setting group path, there must be at least one non-macro segment at the beginning of the path. ({0})";

				public const string IncompleteSegmentMacro = "error: When specifying a Multi-Setting macro, it must be the entire URI segment (no other characters between slashes). ({0})";

				public const string IntegerMinMaxLength = "error: The MinLength and MaxLength attributes should only be used for validation of String Settings.";

				public const string InvalidCharactersInURI = "error: The SettingsGroup Path and Setting Name should contain only alphanumeric characters and forward slashes. ({0})";

				public const string InvalidInteger = "error: Unable to parse the input string \"{0}\" as an integer.";

				public const string InvalidMinMaxValue = "error: invalid Min or Max value ({0}).";

				public const string InvalidMinMaxLengthValue = "error: invalid MinLength or MaxLength value ({0}).";

				public const string InvalidPath = "error: invalid Source Path ({0}).";

				public const string InvalidType = "error: The specified type is an unknown or unsupported type. ({0})";

				public const string InvalidVariableName = "error: invalid characters in the group path's Multi-Setting macro (variable name). ({0})";

				public const string NestedMacros = "error: nesting macros is not allowed in the group path's Multi-Setting macro (variable name). ({0})";

				public const string NoSource = "error: You must specify a valid *Source Element for each non-Asset Setting.";

				public const string NoValidatorMinMax = "error: A Validate Element for an Integer Setting must contain Option Elements or specify a Min and/or Max value.";

				public const string NoValidatorMinMaxLength = "error: A Validate Element for a String Setting must contain Option Elements or specify a MinLength and/or MaxLength value.";

				public const string PartnerVariableWithUnderscore = "error: Multi-Setting macros (variable names) must begin with alphabetic characters only.";

				public const string RunTimeMacroInImageTimeSetting = "error: The {0} macro is available for RunTime Settings only.";

				public const string StringMinMax = "error: The Min and Max attributes should only be used for validation of Integer Settings.";

				public const string ValidationUnsupportedType = "error: Validation is not valid for the type {0}.";
			}

			public const string AssetMacro = "~AssetName~";

			public const string OEMAssetSuffix = ".OEMAssets";

			public const string MOAssetSuffix = ".MOAssets";

			public const string DatastorePath = "$(hklm.software)\\Microsoft\\MCSF\\Settings";
		}

		private enum Option
		{
			No,
			Yes
		}

		private enum SourceType
		{
			MCSF_SETTINGSOURCE_REGISTRY = 1
		}

		private enum ValidationMethodType
		{
			MCSF_VALIDATIONMETHOD_NONE,
			MCSF_VALIDATIONMETHOD_RANGE,
			MCSF_VALIDATIONMETHOD_OPTION
		}

		private enum RegValueType
		{
			REG_SZ = 1,
			REG_BINARY = 3,
			REG_DWORD = 4,
			REG_MULTI_SZ = 7
		}

		private enum CfgDataType
		{
			CFG_DATATYPE_INTEGER = 0,
			CFG_DATATYPE_STRING = 1,
			CFG_DATATYPE_BOOLEAN = 5,
			CFG_DATATYPE_BINARY = 6,
			CFG_DATATYPE_MULTIPLE_STRING = 7
		}

		[Flags]
		private enum AccessType
		{
			None = 0,
			Create = 1,
			Delete = 2,
			Get = 4,
			Replace = 8,
			All = 0xF
		}

		private class Setting
		{
			public string URI { get; set; }

			public AccessType Access { get; set; }

			public SourceType Source { get; set; }

			public string SourcePath { get; set; }

			public uint SourceType { get; set; }

			public ValidationMethodType Validation { get; set; }

			public int Min { get; set; }

			public int Max { get; set; }

			public List<string> Options { get; set; }

			public bool IsString { get; set; }

			public Setting()
			{
				Access = AccessType.All;
				Source = PkgRegWriter.SourceType.MCSF_SETTINGSOURCE_REGISTRY;
				Validation = ValidationMethodType.MCSF_VALIDATIONMETHOD_NONE;
				Min = int.MinValue;
				Max = int.MaxValue;
				Options = new List<string>();
			}

			public Setting(string uri, string sourcePath, uint sourceType)
			{
				URI = uri;
				Access = AccessType.All;
				Source = PkgRegWriter.SourceType.MCSF_SETTINGSOURCE_REGISTRY;
				SourceType = sourceType;
				SourcePath = sourcePath;
				Validation = ValidationMethodType.MCSF_VALIDATIONMETHOD_NONE;
				Min = int.MinValue;
				Max = int.MaxValue;
				Options = new List<string>();
			}

			public void BuildRegKey(RegistryKeyGroupBuilder groupBuilder)
			{
				string text = "$(hklm.software)\\Microsoft\\MCSF\\Settings\\" + URI;
				RegistryKeyBuilder registryKeyBuilder = groupBuilder.AddRegistryKey(text);
				registryKeyBuilder.AddValue("AllowedAccess", RegValueType.REG_DWORD.ToString(), Convert.ToString((uint)Access, 16));
				registryKeyBuilder.AddValue("Source", RegValueType.REG_DWORD.ToString(), Convert.ToString((uint)Source, 16));
				registryKeyBuilder.AddValue("SourcePath", RegValueType.REG_SZ.ToString(), SourcePath);
				registryKeyBuilder.AddValue("SourceType", RegValueType.REG_DWORD.ToString(), Convert.ToString(SourceType, 16));
				registryKeyBuilder.AddValue("ValidationMethod", RegValueType.REG_DWORD.ToString(), Convert.ToString((uint)Validation, 16));
				switch (Validation)
				{
				case ValidationMethodType.MCSF_VALIDATIONMETHOD_OPTION:
				{
					RegistryKeyBuilder registryKeyBuilder2 = groupBuilder.AddRegistryKey(text + "\\Options");
					uint num = 0u;
					{
						foreach (string option in Options)
						{
							if (IsString)
							{
								registryKeyBuilder2.AddValue(Convert.ToString(num, 10), RegValueType.REG_SZ.ToString(), option);
							}
							else
							{
								registryKeyBuilder2.AddValue(Convert.ToString(num, 10), RegValueType.REG_DWORD.ToString(), option);
							}
							num++;
						}
						break;
					}
				}
				case ValidationMethodType.MCSF_VALIDATIONMETHOD_RANGE:
					registryKeyBuilder.AddValue("ValidateMin", RegValueType.REG_DWORD.ToString(), Convert.ToString((uint)Min, 16));
					registryKeyBuilder.AddValue("ValidateMax", RegValueType.REG_DWORD.ToString(), Convert.ToString((uint)Max, 16));
					break;
				}
			}
		}

		public bool GenerateReadablePolicyXML { get; set; }

		private IMacroResolver MacroResolver { get; set; }

		private SortedSet<string> LocalMacros { get; set; }

		private string PackageName { get; set; }

		private string TempPath { get; set; }

		private XDocument PolicyDoc { get; set; }

		private XElement PolicyRoot { get; set; }

		private OSComponentBuilder OSBuilder { get; set; }

		private static int StringToInteger(string integerAsString, XElement element)
		{
			try
			{
				string text = integerAsString.Trim();
				if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					return int.Parse(text.Substring(2), NumberStyles.HexNumber, null);
				}
				return int.Parse(text, null);
			}
			catch (Exception ex)
			{
				if (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
				{
					throw new PkgXmlException(element, "error: Unable to parse the input string \"{0}\" as an integer.", integerAsString);
				}
				throw;
			}
		}

		private string OutputFile(string outputDir, XmlWriterSettings writerSettings)
		{
			Directory.CreateDirectory(outputDir);
			string text = Path.Combine(outputDir, PackageName + ".policy.xml");
			using (XmlWriter writer = XmlWriter.Create(text, writerSettings))
			{
				PolicyDoc.WriteTo(writer);
				return text;
			}
		}

		public PkgRegWriter(string packageName, string tempPath, IMacroResolver macroResolver)
		{
			PackageName = packageName;
			TempPath = tempPath;
			MacroResolver = macroResolver;
			OSBuilder = new OSComponentBuilder();
			LocalMacros = new SortedSet<string>();
			OSBuilder.RegisterMacro("__IMSI", string.Format(null, "~{0}~", new object[1] { "__IMSI" }));
			OSBuilder.RegisterMacro("__ICCID", string.Format(null, "~{0}~", new object[1] { "__ICCID" }));
			MacroResolver.Register("__IMSI", string.Format(null, "~{0}~", new object[1] { "__IMSI" }));
			MacroResolver.Register("__ICCID", string.Format(null, "~{0}~", new object[1] { "__ICCID" }));
			PolicyDoc = new XDocument();
			PolicyRoot = new XElement("CustomizationPolicy");
			PolicyDoc.Add(PolicyRoot);
			GenerateReadablePolicyXML = false;
		}

		public PkgObject ToPkgObject()
		{
			new PolicyStore().LoadPolicyXML(PolicyDoc);
			XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
			if (GenerateReadablePolicyXML)
			{
				xmlWriterSettings.NewLineChars = "\r\n";
				xmlWriterSettings.Indent = true;
				xmlWriterSettings.IndentChars = "  ";
			}
			string outputDir = Path.Combine(TempPath, "CustomizationPolicy");
			outputDir = OutputFile(outputDir, xmlWriterSettings);
			string environmentVariable = Environment.GetEnvironmentVariable("BINARY_ROOT");
			if (environmentVariable != null)
			{
				string outputDir2 = Path.Combine(environmentVariable, "files", "OEMCustomizations", "generatedPolicy");
				OutputFile(outputDir2, xmlWriterSettings);
			}
			OSBuilder.AddFileGroup().AddFile(outputDir, "$(runtime.windows)\\CustomizationPolicy");
			return OSBuilder.ToPkgObject();
		}

		private static bool IsBuiltInMacro(string varName)
		{
			switch (varName)
			{
			case "__ICCID":
			case "__IMSI":
			case "__CANINDEX":
			case "__MVID":
			case "__SIMSLOT":
				return true;
			default:
				return false;
			}
		}

		private static bool IsRunTimeOnlyMacro(string varName)
		{
			switch (varName)
			{
			case "__ICCID":
			case "__IMSI":
			case "__CANINDEX":
			case "__MVID":
			case "__SIMSLOT":
				return true;
			default:
				return false;
			}
		}

		private bool RegisterVariableMacros(XElement element, string path, bool isImageTimeOnly = false)
		{
			bool result = false;
			string[] array = path.Split('/');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (Regex.IsMatch(text, "^\\$\\(.*\\)$"))
				{
					result = true;
					if (text == array[0])
					{
						throw new PkgXmlException(element, "error: When specifying a Multi-Setting group path, there must be at least one non-macro segment at the beginning of the path. ({0})", path);
					}
					string text2 = text.Substring(2, text.Length - 3);
					if (Regex.IsMatch(text2, "\\$\\(.*\\)"))
					{
						throw new PkgXmlException(element, "error: nesting macros is not allowed in the group path's Multi-Setting macro (variable name). ({0})", path);
					}
					if (!IsBuiltInMacro(text2))
					{
						if (!Regex.IsMatch(text2, "^[a-zA-Z][a-zA-Z_0-9.]*$"))
						{
							throw new PkgXmlException(element, "error: invalid characters in the group path's Multi-Setting macro (variable name). ({0})", path);
						}
						if (!LocalMacros.Contains(text2))
						{
							OSBuilder.RegisterMacro(text2, string.Format(null, "~{0}~", new object[1] { text2 }));
							MacroResolver.Register(text2, string.Format(null, "~{0}~", new object[1] { text2 }));
							LocalMacros.Add(text2);
						}
					}
					else if (isImageTimeOnly && IsRunTimeOnlyMacro(text2))
					{
						throw new PkgXmlException(element, "error: The {0} macro is available for RunTime Settings only.", text2);
					}
				}
				else
				{
					if (Regex.IsMatch(text, "\\$\\(.*\\)"))
					{
						throw new PkgXmlException(element, "error: When specifying a Multi-Setting macro, it must be the entire URI segment (no other characters between slashes). ({0})", text);
					}
					if (!Regex.IsMatch(text, "^[a-zA-Z0-9]*$"))
					{
						throw new PkgXmlException(element, "error: The SettingsGroup Path and Setting Name should contain only alphanumeric characters and forward slashes. ({0})", text);
					}
				}
			}
			return result;
		}

		private bool ValidateRegistryPath(XElement element, string path, bool isImageTimeOnly = false)
		{
			string[] array = path.Split('\\');
			foreach (string text in array)
			{
				if (Regex.IsMatch(text, "^\\$\\(.*\\)$"))
				{
					string text2 = text.Substring(2, text.Length - 3);
					if (Regex.IsMatch(text2, "\\$\\(.*\\)"))
					{
						throw new PkgXmlException(element, "error: nesting macros is not allowed in the group path's Multi-Setting macro (variable name). ({0})", path);
					}
					if (IsBuiltInMacro(text2) && isImageTimeOnly && IsRunTimeOnlyMacro(text2))
					{
						throw new PkgXmlException(element, "error: The {0} macro is available for RunTime Settings only.", text2);
					}
				}
				else if (Regex.IsMatch(text, "\\$\\(.*\\)"))
				{
					throw new PkgXmlException(element, "error: When specifying a Multi-Setting macro, it must be the entire URI segment (no other characters between slashes). ({0})", text);
				}
			}
			return true;
		}

		public void WriteSettingsGroup(XElement group)
		{
			bool flag = false;
			LocalMacros.Clear();
			MacroResolver.BeginLocal();
			string groupPath = null;
			group.WithLocalAttribute("Path", delegate(XAttribute x)
			{
				groupPath = x.Value;
			});
			bool flag2 = RegisterVariableMacros(group, groupPath);
			bool imageTimeOnly = false;
			group.LocalElement("Constraints")?.WithLocalAttribute("ImageTimeOnly", delegate(XAttribute x)
			{
				imageTimeOnly = x.Value.Equals("Yes", StringComparison.Ordinal);
			});
			List<string> list = new List<string>();
			foreach (XElement item in group.LocalElements("Asset"))
			{
				string value = item.LocalAttribute("Name").Value;
				if (list.Contains(value))
				{
					throw new PkgXmlException(item, "error: The asset or setting name \"{0}\" has already been used within this Settings Group.", value);
				}
				list.Add(value);
				if (item.LocalElement("ValueList") != null || item.LocalElement("MultiStringList") != null)
				{
					flag = true;
				}
			}
			list = new List<string>();
			foreach (XElement item2 in group.LocalElements("Setting"))
			{
				string value2 = item2.LocalAttribute("Name").Value;
				if (list.Contains(value2))
				{
					throw new PkgXmlException(item2, "error: The asset or setting name \"{0}\" has already been used within this Settings Group.", value2);
				}
				list.Add(value2);
				if (item2.LocalElement("RegistrySource") != null)
				{
					flag = true;
				}
			}
			if (flag && !imageTimeOnly)
			{
				RegistryKeyGroupBuilder groupBuilder = OSBuilder.AddRegistryGroup();
				foreach (XElement item3 in group.LocalElements("Asset"))
				{
					string assetName2 = null;
					item3.WithLocalAttribute("Name", delegate(XAttribute x)
					{
						assetName2 = x.Value;
					});
					RegisterVariableMacros(item3, assetName2);
					string text = groupPath + "/" + assetName2;
					XElement xElement = item3.LocalElement("MultiStringList");
					if (xElement != null)
					{
						string value3 = xElement.LocalAttribute("Key").Value;
						string value4 = xElement.LocalAttribute("Value").Value;
						new Setting(text + ".OEMAssets", value3 + "\\" + value4, 7u).BuildRegKey(groupBuilder);
					}
					XElement xElement2 = item3.LocalElement("ValueList");
					if (xElement2 != null)
					{
						string oemKey = null;
						string operatorKey = null;
						xElement2.WithLocalAttribute("OEMKey", delegate(XAttribute x)
						{
							oemKey = x.Value;
						});
						xElement2.WithLocalAttribute("MOKey", delegate(XAttribute x)
						{
							operatorKey = x.Value;
						});
						if (oemKey != null)
						{
							new Setting(text + ".OEMAssets/~AssetName~", oemKey + "\\~AssetName~", 1u).BuildRegKey(groupBuilder);
						}
						if (operatorKey != null)
						{
							new Setting(text + ".MOAssets/~AssetName~", operatorKey + "\\~AssetName~", 1u).BuildRegKey(groupBuilder);
						}
					}
				}
				foreach (XElement item4 in group.LocalElements("Setting"))
				{
					string settingName2 = null;
					item4.WithLocalAttribute("Name", delegate(XAttribute x)
					{
						settingName2 = x.Value;
					});
					bool flag3 = RegisterVariableMacros(item4, settingName2);
					XElement xElement3 = item4.LocalElement("RegistrySource");
					if (xElement3 == null)
					{
						xElement3 = item4.LocalElement("CspSource");
						if (xElement3 == null)
						{
							throw new PkgXmlException(item4, "error: You must specify a valid *Source Element for each non-Asset Setting.");
						}
						continue;
					}
					Setting setting = new Setting();
					setting.URI = groupPath + "/" + settingName2;
					XElement xElement4 = item4.LocalElement("AccessType");
					if (xElement4 != null)
					{
						xElement4.WithLocalAttribute("Create", delegate(XAttribute x)
						{
							setting.Access &= (AccessType)(~(((Option)Enum.Parse(typeof(Option), x.Value) == Option.No) ? 1 : 0));
						});
						xElement4.WithLocalAttribute("Delete", delegate(XAttribute x)
						{
							setting.Access &= (AccessType)(~(((Option)Enum.Parse(typeof(Option), x.Value) == Option.No) ? 2 : 0));
						});
						xElement4.WithLocalAttribute("Get", delegate(XAttribute x)
						{
							setting.Access &= (AccessType)(~(((Option)Enum.Parse(typeof(Option), x.Value) == Option.No) ? 4 : 0));
						});
						xElement4.WithLocalAttribute("Replace", delegate(XAttribute x)
						{
							setting.Access &= (AccessType)(~(((Option)Enum.Parse(typeof(Option), x.Value) == Option.No) ? 8 : 0));
						});
						xElement4.WithLocalAttribute("All", delegate(XAttribute x)
						{
							setting.Access &= (AccessType)(~(((Option)Enum.Parse(typeof(Option), x.Value) == Option.No) ? 15 : 0));
						});
					}
					setting.SourcePath = xElement3.LocalAttribute("Path").Value;
					string value5 = xElement3.LocalAttribute("Type").Value;
					XAttribute xAttribute = xElement3.LocalAttribute("Default");
					XElement xElement5 = item4.LocalElement("RegistrySource");
					if (xElement5 != null)
					{
						ValidateRegistryPath(xElement3, setting.SourcePath, imageTimeOnly);
					}
					string localName = xElement3.Name.LocalName;
					if (localName == "RegistrySource")
					{
						setting.Source = SourceType.MCSF_SETTINGSOURCE_REGISTRY;
						int num = setting.SourcePath.LastIndexOf('\\');
						if (-1 == num || int.MaxValue == num)
						{
							throw new PkgXmlException(xElement3, "error: invalid Source Path ({0}).", setting.SourcePath);
						}
						if (xAttribute != null && (flag2 || flag3))
						{
							throw new PkgXmlException(xElement3, "error: Multi-settings may not supply a default value, as they do not point to an explicit registry location.");
						}
						setting.SourceType = (uint)(RegValueType)Enum.Parse(typeof(RegValueType), value5);
						switch (setting.SourceType)
						{
						case 3u:
						case 4u:
							setting.IsString = false;
							break;
						case 1u:
						case 7u:
							setting.IsString = true;
							break;
						default:
							throw new PkgXmlException(xElement3, "error: The specified type is an unknown or unsupported type. ({0})", value5);
						}
					}
					XElement xElement6 = item4.LocalElement("Validate");
					if (xElement6 != null)
					{
						SourceType source = setting.Source;
						if (source == SourceType.MCSF_SETTINGSOURCE_REGISTRY)
						{
							uint sourceType = setting.SourceType;
							if (sourceType == 3)
							{
								throw new PkgXmlException(xElement6, "error: Validation is not valid for the type {0}.", Enum.GetName(typeof(RegValueType), RegValueType.REG_BINARY));
							}
						}
						if (xElement6.LocalElements("Option").Count() == 0)
						{
							setting.Validation = ValidationMethodType.MCSF_VALIDATIONMETHOD_RANGE;
							XAttribute xAttribute2 = null;
							XAttribute xAttribute3 = null;
							if (!setting.IsString)
							{
								xAttribute2 = xElement6.LocalAttribute("Min");
								xAttribute3 = xElement6.LocalAttribute("Max");
								if (xAttribute2 == null && xAttribute3 == null)
								{
									throw new PkgXmlException(xElement6, "error: A Validate Element for an Integer Setting must contain Option Elements or specify a Min and/or Max value.");
								}
								if (xElement6.LocalAttribute("MinLength") != null || xElement6.LocalAttribute("MaxLength") != null)
								{
									throw new PkgXmlException(xElement6, "error: The MinLength and MaxLength attributes should only be used for validation of String Settings.");
								}
							}
							else
							{
								xAttribute2 = xElement6.LocalAttribute("MinLength");
								xAttribute3 = xElement6.LocalAttribute("MaxLength");
								if (xAttribute2 == null && xAttribute3 == null)
								{
									throw new PkgXmlException(xElement6, "error: A Validate Element for a String Setting must contain Option Elements or specify a MinLength and/or MaxLength value.");
								}
								if (xElement6.LocalAttribute("Min") != null || xElement6.LocalAttribute("Max") != null)
								{
									throw new PkgXmlException(xElement6, "error: The Min and Max attributes should only be used for validation of Integer Settings.");
								}
							}
							if (xAttribute2 != null)
							{
								setting.Min = StringToInteger(xAttribute2.Value, xElement6);
								if (setting.IsString && setting.Min < 0)
								{
									throw new PkgXmlException(xElement6, "error: invalid Min or Max value ({0}).");
								}
							}
							else if (setting.IsString)
							{
								setting.Min = 0;
							}
							if (xAttribute3 != null)
							{
								setting.Max = StringToInteger(xAttribute3.Value, xElement6);
							}
						}
						else
						{
							setting.Validation = ValidationMethodType.MCSF_VALIDATIONMETHOD_OPTION;
							XmlToLinqExtensions.WithEntityDelegate<XAttribute> withEntityDelegate = default(XmlToLinqExtensions.WithEntityDelegate<XAttribute>);
							foreach (XElement option in xElement6.LocalElements("Option"))
							{
								if (option == null)
								{
									continue;
								}
								if (setting.IsString)
								{
									XElement source2 = option;
									XmlToLinqExtensions.WithEntityDelegate<XAttribute> withEntityDelegate2 = withEntityDelegate;
									if (withEntityDelegate2 == null)
									{
										withEntityDelegate2 = (withEntityDelegate = delegate(XAttribute x)
										{
											setting.Options.Add(x.Value);
										});
									}
									source2.WithLocalAttribute("Value", withEntityDelegate2);
								}
								else
								{
									uint dwordValue = 0u;
									option.WithLocalAttribute("Value", delegate(XAttribute x)
									{
										dwordValue = (uint)StringToInteger(x.Value, option);
									});
									setting.Options.Add(Convert.ToString(dwordValue, 16));
								}
							}
						}
					}
					setting.BuildRegKey(groupBuilder);
				}
			}
			else
			{
				foreach (XElement item5 in group.LocalElements("Asset"))
				{
					string assetName = null;
					item5.WithLocalAttribute("Name", delegate(XAttribute x)
					{
						assetName = x.Value;
					});
					RegisterVariableMacros(item5, assetName);
				}
				foreach (XElement item6 in group.LocalElements("Setting"))
				{
					string settingName = null;
					item6.WithLocalAttribute("Name", delegate(XAttribute x)
					{
						settingName = x.Value;
					});
					RegisterVariableMacros(item6, settingName, imageTimeOnly);
					if (imageTimeOnly)
					{
						XElement xElement7 = item6.LocalElement("RegistrySource");
						if (xElement7 != null)
						{
							ValidateRegistryPath(xElement7, xElement7.LocalAttribute("Path").Value, imageTimeOnly);
						}
					}
				}
			}
			foreach (XElement item7 in group.LocalElements("Setting"))
			{
				XElement xElement8 = item7.LocalElement("RegistrySource");
				if (xElement8 == null)
				{
					continue;
				}
				XAttribute xAttribute4 = xElement8.LocalAttribute("Default");
				if (xAttribute4 != null)
				{
					int num2 = xElement8.LocalAttribute("Path").Value.LastIndexOf('\\');
					if (-1 == num2 || int.MaxValue == num2)
					{
						throw new PkgXmlException(xElement8, "error: invalid Source Path ({0}).", xElement8.LocalAttribute("Path").Value);
					}
					string keyName = xElement8.LocalAttribute("Path").Value.Substring(0, num2);
					string name = xElement8.LocalAttribute("Path").Value.Substring(num2 + 1);
					string value6 = xElement8.LocalAttribute("Type").Value;
					RegistryKeyBuilder registryKeyBuilder = OSBuilder.AddRegistryGroup().AddRegistryKey(keyName);
					if ((RegValueType)Enum.Parse(typeof(RegValueType), value6) == RegValueType.REG_DWORD)
					{
						uint num3 = (uint)StringToInteger(xAttribute4.Value, xElement8);
						registryKeyBuilder.AddValue(name, value6, Convert.ToString(num3, 16));
					}
					else
					{
						registryKeyBuilder.AddValue(name, value6, xAttribute4.Value);
					}
				}
			}
			XElement xElement9 = new XElement(group);
			foreach (XElement item8 in xElement9.DescendantsAndSelf())
			{
				foreach (XAttribute item9 in item8.Attributes())
				{
					item9.Value = MacroResolver.Resolve(item9.Value);
				}
			}
			PolicyRoot.Add(xElement9);
			LocalMacros.Clear();
			MacroResolver.EndLocal();
		}
	}
}
