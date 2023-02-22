using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public static class RegBuilder
	{
		private static void CheckConflicts(IEnumerable<RegValueInfo> values)
		{
			Dictionary<string, RegValueInfo> dictionary = new Dictionary<string, RegValueInfo>();
			foreach (RegValueInfo value2 in values)
			{
				if (value2.ValueName != null)
				{
					RegValueInfo value = null;
					if (dictionary.TryGetValue(value2.ValueName, out value))
					{
						throw new IUException("Registry conflict discovered: keyName: {0}, valueName: {1}, oldValue: {2}, newValue: {3}", value2.KeyName, value2.ValueName, value.Value, value2.Value);
					}
					dictionary.Add(value2.ValueName, value2);
				}
			}
		}

		private static void ConvertRegSz(StringBuilder output, string name, string value)
		{
			RegUtil.RegOutput(output, name, value, false);
		}

		private static void ConvertRegExpandSz(StringBuilder output, string name, string value)
		{
			RegUtil.RegOutput(output, name, value, true);
		}

		private static void ConvertRegMultiSz(StringBuilder output, string name, string value)
		{
			RegUtil.RegOutput(output, name, value.Split(';'));
		}

		private static void ConvertRegDWord(StringBuilder output, string name, string value)
		{
			uint result = 0u;
			if (!uint.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat, out result))
			{
				throw new IUException("Invalid dword string: {0}", value);
			}
			RegUtil.RegOutput(output, name, result);
		}

		private static void ConvertRegQWord(StringBuilder output, string name, string value)
		{
			ulong result = 0uL;
			if (!ulong.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat, out result))
			{
				throw new IUException("Invalid qword string: {0}", value);
			}
			RegUtil.RegOutput(output, name, result);
		}

		private static void ConvertRegBinary(StringBuilder output, string name, string value)
		{
			RegUtil.RegOutput(output, name, RegUtil.HexStringToByteArray(value));
		}

		private static void ConvertRegHex(StringBuilder output, string name, string value)
		{
			Match match = Regex.Match(value, "^hex\\((?<type>[0-9A-Fa-f]+)\\):(?<value>.*)$");
			if (!match.Success)
			{
				throw new IUException("Invalid value '{0}' for REG_HEX type, shoudl be 'hex(<type>):<binary_values>'", value);
			}
			int result = 0;
			if (!int.TryParse(match.Groups["type"].Value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat, out result))
			{
				throw new IUException("Invalid hex type '{0}' in REG_HEX value '{1}'", match.Groups["type"].Value, value);
			}
			string value2 = match.Groups["value"].Value;
			RegUtil.RegOutput(output, name, result, RegUtil.HexStringToByteArray(value2));
		}

		private static void WriteValue(RegValueInfo value, StringBuilder regContent)
		{
			switch (value.Type)
			{
			case RegValueType.String:
				ConvertRegSz(regContent, value.ValueName, value.Value);
				break;
			case RegValueType.ExpandString:
				ConvertRegExpandSz(regContent, value.ValueName, value.Value);
				break;
			case RegValueType.MultiString:
				ConvertRegMultiSz(regContent, value.ValueName, value.Value);
				break;
			case RegValueType.DWord:
				ConvertRegDWord(regContent, value.ValueName, value.Value);
				break;
			case RegValueType.QWord:
				ConvertRegQWord(regContent, value.ValueName, value.Value);
				break;
			case RegValueType.Binary:
				ConvertRegBinary(regContent, value.ValueName, value.Value);
				break;
			case RegValueType.Hex:
				ConvertRegHex(regContent, value.ValueName, value.Value);
				break;
			default:
				throw new IUException("Unknown registry value type '{0}'", value.Type);
			}
		}

		private static void WriteKey(string keyName, IEnumerable<RegValueInfo> values, StringBuilder regContent)
		{
			regContent.AppendFormat("[{0}]", keyName);
			regContent.AppendLine();
			foreach (RegValueInfo value in values)
			{
				if (value.ValueName != null)
				{
					WriteValue(value, regContent);
				}
			}
			regContent.AppendLine();
		}

		public static void Build(IEnumerable<RegValueInfo> values, string outputFile)
		{
			Build(values, outputFile, "");
		}

		public static void Build(IEnumerable<RegValueInfo> values, string outputFile, string headerComment)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Windows Registry Editor Version 5.00");
			if (!string.IsNullOrEmpty(headerComment))
			{
				string[] array = headerComment.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				foreach (string text in array)
				{
					string text2 = text.TrimStart(' ');
					if (text2 != string.Empty && text2[0] == ';')
					{
						stringBuilder.AppendLine(text);
					}
					else
					{
						stringBuilder.AppendLine("; " + text);
					}
				}
				stringBuilder.AppendLine("");
			}
			foreach (IGrouping<string, RegValueInfo> item in from x in values
				group x by x.KeyName)
			{
				CheckConflicts(item);
				WriteKey(item.Key, item, stringBuilder);
			}
			LongPathFile.WriteAllText(outputFile, stringBuilder.ToString(), Encoding.Unicode);
		}
	}
}
