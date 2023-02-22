using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	internal static class RegHelpers
	{
		private static Dictionary<string, string> regKeyNameToMacro = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services", "$(hklm.services)" },
			{ "HKEY_LOCAL_MACHINE\\SYSTEM\\ControlSet001\\Services", "$(hklm.services)" },
			{ "HKEY_LOCAL_MACHINE\\SYSTEM", "$(hklm.system)" },
			{ "HKEY_LOCAL_MACHINE\\SOFTWARE", "$(hklm.software)" },
			{ "HKEY_LOCAL_MACHINE\\HARDWARE", "$(hklm.hardware)" },
			{ "HKEY_LOCAL_MACHINE\\SAM", "$(hklm.sam)" },
			{ "HKEY_LOCAL_MACHINE\\Security", "$(hklm.security)" },
			{ "HKEY_LOCAL_MACHINE\\BCD", "$(hklm.bcd)" },
			{ "HKEY_LOCAL_MACHINE\\Drivers", "$(hklm.drivers)" },
			{ "HKEY_CLASSES_ROOT", "$(hkcr.root)" },
			{ "HKEY_CURRENT_USER", "$(hkcu.root)" },
			{ "HKEY_USERS\\.DEFAULT", "$(hkuser.default)" },
			{ "HKEY_LOCAL_MACHINE\\COMPONENTS", "" }
		};

		public static XElement PkgRegValue(string name, string valueType, string value)
		{
			XNamespace xNamespace = "urn:Microsoft.WindowsPhone/PackageSchema.v8.00";
			string text = null;
			string text2 = null;
			string text3 = null;
			text = ((!string.IsNullOrEmpty(name)) ? name : "@");
			text2 = valueType.ToUpperInvariant();
			text3 = value.Trim();
			switch (text2)
			{
			case "REG_DWORD":
			case "REG_QWORD":
				if (text3.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
				{
					text3 = text3.Substring(2);
				}
				break;
			case "REG_BINARY":
				text3 = SeperateRegBinaryWithCommas(text3.ToLowerInvariant());
				break;
			case "REG_RESOURCE_REQUIREMENTS_LIST":
			case "REG_RESOURCE_LIST":
				Console.WriteLine("warning: ignoring {0}", text2);
				return null;
			case "REG_NONE":
				text2 = "REG_SZ";
				break;
			}
			XElement xElement = new XElement(xNamespace + "RegValue");
			xElement.Add(new XAttribute("Name", text));
			xElement.Add(new XAttribute("Type", text2));
			xElement.Add(new XAttribute("Value", text3));
			return xElement;
		}

		public static XElement PkgRegKey(string name)
		{
			XElement xElement = new XElement((XNamespace)"urn:Microsoft.WindowsPhone/PackageSchema.v8.00" + "RegKey");
			xElement.Add(new XAttribute("KeyName", name));
			return xElement;
		}

		private static string SeperateRegBinaryWithCommas(string RegBinary)
		{
			if (RegBinary == null)
			{
				return null;
			}
			if (RegBinary == "")
			{
				return "00";
			}
			if (RegBinary.Split(',').Length > 1)
			{
				return RegBinary;
			}
			string text = "";
			if (RegBinary.Length % 2 != 0)
			{
				return null;
			}
			for (int i = 0; i < RegBinary.Length; i++)
			{
				if (i % 2 == 0)
				{
					text += ",";
				}
				text += RegBinary[i];
			}
			return text.Trim(',');
		}

		public static string RegKeyNameToMacro(string RegKeyName)
		{
			if (RegKeyName.StartsWith("HKLM", StringComparison.OrdinalIgnoreCase))
			{
				RegKeyName = Regex.Replace(RegKeyName, "HKLM", "HKEY_LOCAL_MACHINE\\SYSTEM");
			}
			if (RegKeyName.StartsWith("HKCU", StringComparison.OrdinalIgnoreCase))
			{
				RegKeyName = "";
				return null;
			}
			foreach (KeyValuePair<string, string> item in regKeyNameToMacro)
			{
				if (RegKeyName.StartsWith(item.Key, StringComparison.OrdinalIgnoreCase))
				{
					if (item.Value.Equals("", StringComparison.OrdinalIgnoreCase))
					{
						Console.WriteLine("warning: could not resolve {0}", item.Value);
						return null;
					}
					string text = RegKeyName.Remove(0, item.Key.Length);
					RegKeyName = item.Value + text;
					return RegKeyName;
				}
			}
			if (!RegKeyName.StartsWith("$(", StringComparison.OrdinalIgnoreCase))
			{
				return null;
			}
			return RegKeyName;
		}

		public static string RegMacroToKeyName(string RegKeyName)
		{
			foreach (KeyValuePair<string, string> item in regKeyNameToMacro)
			{
				if (RegKeyName.StartsWith(item.Value, StringComparison.OrdinalIgnoreCase))
				{
					string text = RegKeyName.Remove(0, item.Value.Length);
					RegKeyName = item.Key + text;
					return RegKeyName;
				}
			}
			return RegKeyName;
		}
	}
}
