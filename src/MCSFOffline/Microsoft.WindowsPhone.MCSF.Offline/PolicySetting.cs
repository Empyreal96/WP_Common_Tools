using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.WindowsPhone.ImageUpdate.PkgCommon;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	public class PolicySetting
	{
		private const string CustomIntegerType = "integer";

		private const string CustomStringType = "string";

		private const string CustomBooleanType = "boolean";

		private const string CustomBinaryType = "binary";

		private const string CustomInteger64Type = "integer64";

		private List<PolicyEnum> options;

		public string DefinedIn;

		private List<string> _oemMacros;

		public string Name { get; private set; }

		public string Description { get; private set; }

		public string FieldName { get; private set; }

		public string SampleValue { get; private set; }

		public PolicySettingType SettingType { get; private set; }

		public PolicySettingDestination Destination { get; private set; }

		public PolicyAssetInfo AssetInfo { get; private set; }

		public string DefaultValue { get; private set; }

		public int Min { get; private set; }

		public int Max { get; private set; }

		public string Partition { get; private set; }

		public static IEnumerable<string> ValidPartitions => new List<string>
		{
			PkgConstants.c_strMainOsPartition,
			PkgConstants.c_strEfiPartition,
			PkgConstants.c_strUpdateOsPartition
		};

		public IEnumerable<PolicyEnum> Options => options;

		public List<string> OEMMacros
		{
			get
			{
				if (_oemMacros == null)
				{
					_oemMacros = PolicyMacroTable.OEMMacroList(Name);
				}
				return _oemMacros;
			}
		}

		public bool HasOEMMacros => OEMMacros.Count() > 0;

		internal PolicySetting()
		{
			SettingType = PolicySettingType.String;
			Min = int.MinValue;
			Max = int.MaxValue;
			options = null;
			DefaultValue = null;
			AssetInfo = null;
		}

		public PolicySetting(XElement settingElement, PolicyGroup parent)
			: this(settingElement, parent, null, null)
		{
		}

		public PolicySetting(XElement settingElement, PolicyGroup parent, string definedIn)
			: this(settingElement, parent, definedIn, null)
		{
		}

		public PolicySetting(XElement settingElement, PolicyGroup parent, string definedIn, string partition)
			: this()
		{
			Name = (string)settingElement.LocalAttribute("Name");
			Description = (string)settingElement.LocalAttribute("Description");
			FieldName = (string)settingElement.LocalAttribute("FieldName");
			SampleValue = (string)settingElement.LocalAttribute("SampleValue");
			Partition = partition;
			string text = (string)settingElement.LocalAttribute("Asset");
			if (text != null)
			{
				AssetInfo = parent.AssetByName(text);
			}
			XElement xElement = settingElement.LocalElement("RegistrySource") ?? settingElement.LocalElement("CspSource");
			if (xElement != null)
			{
				Destination = new PolicySettingDestination(xElement, this, parent);
				DefaultValue = (string)xElement.LocalAttribute("Default");
				SettingType = DetermineType(Destination.Type);
			}
			else
			{
				Destination = new PolicySettingDestination(this, parent);
			}
			DefinedIn = definedIn;
			LoadValidation(settingElement);
		}

		public PolicyMacroTable GetMacroTable(PolicyMacroTable parentTable, string name)
		{
			PolicyMacroTable policyMacroTable = new PolicyMacroTable(Name, name);
			if (parentTable != null)
			{
				policyMacroTable.AddMacros(parentTable);
			}
			return policyMacroTable;
		}

		public string expandEnumeration(string value)
		{
			foreach (PolicyEnum option in Options)
			{
				if (value.Equals(option.FriendlyName, StringComparison.Ordinal) || value.Equals(option.Value, StringComparison.Ordinal))
				{
					return option.Value;
				}
				try
				{
					uint num = Extensions.ParseInt(value);
					uint num2 = Extensions.ParseInt(option.Value);
					if (num == num2)
					{
						return option.Value;
					}
				}
				catch
				{
				}
			}
			return null;
		}

		public string TransformValue(string value, string customType)
		{
			PolicySettingType policySettingType = SettingType;
			if (SettingType == PolicySettingType.Unknown && customType != null)
			{
				policySettingType = DetermineType(customType);
			}
			switch (policySettingType)
			{
			case PolicySettingType.Integer:
				return Extensions.ParseSignedInt(value).ToString();
			case PolicySettingType.Integer64:
				return Extensions.ParseSignedInt64(value).ToString();
			case PolicySettingType.String:
				if (AssetInfo == null)
				{
					return value;
				}
				if (AssetInfo.Presets != null && AssetInfo.Presets.Count() != 0 && AssetInfo.PresetsAltDir.ContainsKey(value))
				{
					return Path.Combine(AssetInfo.PresetsAltDir[value], value);
				}
				return Path.Combine(AssetInfo.TargetDir, value);
			case PolicySettingType.Enumeration:
			{
				string text = expandEnumeration(value);
				if (text == null)
				{
					throw new MCSFOfflineException($"Value {value} is not a valid friendly name for setting {Name}.");
				}
				return text;
			}
			case PolicySettingType.Boolean:
				if (Options == null)
				{
					return value;
				}
				goto case PolicySettingType.Enumeration;
			case PolicySettingType.Binary:
				return Convert.ToBase64String(RegUtil.HexStringToByteArray(value));
			default:
				throw new MCSFOfflineException("Attempted to transform an unrecognized type!");
			}
		}

		public bool IsValidValue<T>(T value)
		{
			string value2 = Convert.ToString(value);
			return IsValidValue(value2, null);
		}

		public bool IsValidValue(string value, string customType)
		{
			PolicySettingType policySettingType = SettingType;
			if (SettingType == PolicySettingType.Unknown && customType != null)
			{
				policySettingType = DetermineType(customType);
			}
			switch (policySettingType)
			{
			case PolicySettingType.Integer:
				return IsValidInteger(value);
			case PolicySettingType.String:
				return IsValidString(value);
			case PolicySettingType.Enumeration:
				return IsValidEnumeration(value);
			case PolicySettingType.Boolean:
				return isValidBoolean(value);
			case PolicySettingType.Binary:
				return isValidBinary(value);
			case PolicySettingType.Integer64:
				return IsValidInteger64(value);
			default:
				return false;
			}
		}

		private bool isValidBinary(string value)
		{
			try
			{
				RegUtil.HexStringToByteArray(value);
			}
			catch
			{
				return false;
			}
			return true;
		}

		private bool isValidBoolean(string value)
		{
			string intString = value;
			if (Options != null && IsValidEnumeration(value))
			{
				intString = expandEnumeration(value);
			}
			int num;
			try
			{
				num = Extensions.ParseSignedInt(intString);
			}
			catch (Exception ex)
			{
				if (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
				{
					return false;
				}
				throw;
			}
			if (num < 0 || num > 1)
			{
				return false;
			}
			return true;
		}

		private bool IsValidEnumeration(string value)
		{
			foreach (PolicyEnum option in Options)
			{
				if (value.Equals(option.Value, StringComparison.InvariantCulture) || value.Equals(option.FriendlyName, StringComparison.InvariantCulture))
				{
					return true;
				}
				try
				{
					uint num = Extensions.ParseInt(value);
					uint num2 = Extensions.ParseInt(option.Value);
					if (num == num2)
					{
						return true;
					}
				}
				catch
				{
				}
			}
			return false;
		}

		private bool IsValidString(string value)
		{
			if (value.Length < Min || value.Length > Max)
			{
				return false;
			}
			return true;
		}

		private bool IsValidInteger(string value)
		{
			int num;
			try
			{
				num = Extensions.ParseSignedInt(value);
			}
			catch (Exception ex)
			{
				if (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
				{
					return false;
				}
				throw;
			}
			if (num < Min || num > Max)
			{
				return false;
			}
			return true;
		}

		private bool IsValidInteger64(string value)
		{
			try
			{
				Extensions.ParseSignedInt64(value);
			}
			catch (Exception ex)
			{
				if (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
				{
					return false;
				}
				throw;
			}
			return true;
		}

		private void LoadValidation(XElement settingElement)
		{
			if (SettingType == PolicySettingType.Binary || SettingType == PolicySettingType.Unknown)
			{
				return;
			}
			XElement xElement = settingElement.LocalElement("Validate");
			if (xElement == null)
			{
				return;
			}
			IEnumerable<XElement> enumerable = xElement.LocalElements("Option");
			if (enumerable.Count() > 0)
			{
				options = new List<PolicyEnum>();
				foreach (XElement item in enumerable)
				{
					options.Add(new PolicyEnum(item, SettingType == PolicySettingType.Integer));
				}
				if (SettingType != PolicySettingType.Boolean)
				{
					SettingType = PolicySettingType.Enumeration;
				}
			}
			else if (SettingType == PolicySettingType.Integer)
			{
				Min = Extensions.ParseSignedInt((string)xElement.LocalAttribute("Min"), Min);
				Max = Extensions.ParseSignedInt((string)xElement.LocalAttribute("Max"), Max);
			}
			else if (SettingType == PolicySettingType.String)
			{
				Min = Extensions.ParseSignedInt((string)xElement.LocalAttribute("MinLength"), 0);
				Max = Extensions.ParseSignedInt((string)xElement.LocalAttribute("MaxLength"), Max);
			}
		}

		private PolicySettingType DetermineType(string type)
		{
			switch (type)
			{
			case "REG_DWORD":
			case "CFG_DATATYPE_INTEGER":
			case "integer":
				return PolicySettingType.Integer;
			case "REG_BINARY":
			case "CFG_DATATYPE_BINARY":
			case "binary":
				return PolicySettingType.Binary;
			case "CFG_DATATYPE_BOOLEAN":
			case "boolean":
				return PolicySettingType.Boolean;
			case "integer64":
				return PolicySettingType.Integer64;
			case "REG_SZ":
			case "REG_EXPAND_SZ":
			case "REG_MULTI_SZ":
			case "CFG_DATATYPE_STRING":
			case "CFG_DATATYPE_MULTIPLE_STRING":
			case "string":
				return PolicySettingType.String;
			default:
				return PolicySettingType.Unknown;
			}
		}

		public override string ToString()
		{
			return Name;
		}
	}
}
