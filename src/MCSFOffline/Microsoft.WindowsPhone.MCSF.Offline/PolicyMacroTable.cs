using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.MCSF.Offline
{
	public class PolicyMacroTable
	{
		private const char PathSeperator = '/';

		private const string MacroRegexTilde = "~(?<macroName>[A-Za-z0-9_]*)~";

		private const string MacroRegexDollar = "\\$\\((?<macroName>[A-Za-z0-9_]*)\\)";

		private const string MacroExactRegex = "^~(?<macroName>[A-Za-z0-9_]*)~$";

		private const string MacroName = "macroName";

		public Dictionary<string, string> macros;

		private static bool IsBuiltinMacro(string macroName)
		{
			return macroName.StartsWith("__", StringComparison.OrdinalIgnoreCase);
		}

		public static List<string> OEMMacroList(string macroContainingString)
		{
			List<string> list = new List<string>();
			string[] array = macroContainingString.Split('/');
			foreach (string text in array)
			{
				Match match = Regex.Match(text, "^~(?<macroName>[A-Za-z0-9_]*)~$");
				if (match.Success && !IsBuiltinMacro(match.Groups["macroName"].Value))
				{
					list.Add(text);
				}
			}
			return list;
		}

		public static bool IsMatch(string macroContainingString, string expandedString, StringComparison comparisonType)
		{
			string[] array = macroContainingString.Split('/');
			string[] array2 = expandedString.Split('/');
			if (array.Length != array2.Length)
			{
				return false;
			}
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i];
				Match match = Regex.Match(text, "^~(?<macroName>[A-Za-z0-9_]*)~$");
				if (!match.Success)
				{
					if (!text.Equals(array2[i], comparisonType))
					{
						return false;
					}
					continue;
				}
				string value = match.Groups["macroName"].Value;
				if (IsBuiltinMacro(value) && !string.Equals(array2[i], $"$({value})", comparisonType) && !string.Equals(array2[i], $"~{value}~", comparisonType))
				{
					return false;
				}
			}
			return true;
		}

		public PolicyMacroTable(string macroDefiningString, string expandedString)
		{
			macros = new Dictionary<string, string>();
			AddMacros(macroDefiningString, expandedString);
		}

		public string ReplaceMacros(string inputString)
		{
			if (string.IsNullOrEmpty(inputString))
			{
				return inputString;
			}
			return Regex.Replace(inputString, "~(?<macroName>[A-Za-z0-9_]*)~", delegate(Match match)
			{
				string value = match.Groups[1].Value;
				return macros[value];
			});
		}

		internal void AddMacros(PolicyMacroTable otherTable)
		{
			foreach (KeyValuePair<string, string> macro in otherTable.macros)
			{
				if (!macros.ContainsKey(macro.Key))
				{
					macros.Add(macro.Key, macro.Value);
				}
			}
		}

		internal void AddMacros(string macroDefiningString, string expandedString)
		{
			string[] array = macroDefiningString.Split('/');
			string[] array2 = expandedString.Split('/');
			if (!IsMatch(macroDefiningString, expandedString, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException("macroDefiningString and expandedString must be the same path or setting name.");
			}
			for (int i = 0; i < array.Length; i++)
			{
				Match match = Regex.Match(array[i], "^~(?<macroName>[A-Za-z0-9_]*)~$");
				if (match.Success)
				{
					string value = match.Groups[1].Value;
					string value2 = array2[i];
					macros.Add(value, value2);
				}
			}
		}

		public static string MacroTildeToDollar(string input)
		{
			return Regex.Replace(input, "~(?<macroName>[A-Za-z0-9_]*)~", (Match x) => string.Format("$({0})", x.Groups["macroName"].Value));
		}

		public static string MacroDollarToTilde(string input)
		{
			return Regex.Replace(input, "\\$\\((?<macroName>[A-Za-z0-9_]*)\\)", (Match x) => string.Format("~{0}~", x.Groups["macroName"].Value));
		}
	}
}
