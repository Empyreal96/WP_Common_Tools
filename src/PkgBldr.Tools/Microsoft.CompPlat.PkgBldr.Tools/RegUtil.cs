using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class RegUtil
	{
		private const string c_strDefaultValueName = "@";

		private const int c_iBinaryStringLengthPerByte = 3;

		public const string c_strRegHeader = "Windows Registry Editor Version 5.00";

		public static int BinaryLineLength = 120;

		private static string QuoteString(string input)
		{
			return "\"" + input.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
		}

		private static string NormalizeValueName(string name)
		{
			if (name == "@")
			{
				return "@";
			}
			return QuoteString(name);
		}

		private static byte[] RegStringToBytes(string value)
		{
			return Encoding.Unicode.GetBytes(value);
		}

		public static RegValueType RegValueTypeForString(string strType)
		{
			FieldInfo[] fields = typeof(RegValueType).GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				object[] customAttributes = fieldInfo.GetCustomAttributes(typeof(XmlEnumAttribute), false);
				if (customAttributes.Length == 1 && strType.Equals(((XmlEnumAttribute)customAttributes[0]).Name, StringComparison.OrdinalIgnoreCase))
				{
					return (RegValueType)fieldInfo.GetRawConstantValue();
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Unknown Registry value type: {0}", new object[1] { strType }));
		}

		public static byte[] HexStringToByteArray(string hexString)
		{
			List<byte> list = new List<byte>();
			if (hexString != string.Empty)
			{
				string[] array = hexString.Split(',');
				foreach (string s in array)
				{
					byte result = 0;
					if (!byte.TryParse(s, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat, out result))
					{
						throw new IUException("Invalid hex string: {0}", hexString);
					}
					list.Add(result);
				}
			}
			return list.ToArray();
		}

		[SuppressMessage("Microsoft.Design", "CA1026")]
		public static void ByteArrayToRegString(StringBuilder output, byte[] data, int maxOnALine = int.MaxValue)
		{
			int num = 0;
			int num2 = data.Length;
			while (num2 > 0)
			{
				int num3 = Math.Min(num2, maxOnALine);
				string text = BitConverter.ToString(data, num, num3);
				text = text.Replace('-', ',');
				output.Append(text);
				num += num3;
				num2 -= num3;
				if (num2 > 0)
				{
					output.AppendLine(",\\");
				}
			}
		}

		public static void RegOutput(StringBuilder output, string name, IEnumerable<string> values)
		{
			string arg = NormalizeValueName(name);
			output.AppendFormat(";Value:{0}", string.Join(";", values.Select((string x) => x.Replace(";", "\\;"))));
			output.AppendLine();
			output.AppendFormat("{0}=hex(7):", arg);
			ByteArrayToRegString(output, RegStringToBytes(string.Join("\0", values) + "\0\0"), BinaryLineLength / 3);
			output.AppendLine();
		}

		public static void RegOutput(StringBuilder output, string name, string value, bool expandable)
		{
			string arg = NormalizeValueName(name);
			if (expandable)
			{
				output.AppendFormat(";Value:{0}", value);
				output.AppendLine();
				output.AppendFormat("{0}=hex(2):", arg);
				ByteArrayToRegString(output, RegStringToBytes(value + "\0"), BinaryLineLength / 3);
			}
			else
			{
				output.AppendFormat("{0}={1}", arg, QuoteString(value));
			}
			output.AppendLine();
		}

		public static void RegOutput(StringBuilder output, string name, ulong value)
		{
			string arg = NormalizeValueName(name);
			output.AppendFormat(";Value:0X{0:X16}", value);
			output.AppendLine();
			output.AppendFormat("{0}=hex(b):", arg);
			ByteArrayToRegString(output, BitConverter.GetBytes(value));
			output.AppendLine();
		}

		public static void RegOutput(StringBuilder output, string name, uint value)
		{
			string arg = NormalizeValueName(name);
			output.AppendFormat("{0}=dword:{1:X8}", arg, value);
			output.AppendLine();
		}

		public static void RegOutput(StringBuilder output, string name, byte[] value)
		{
			string arg = NormalizeValueName(name);
			output.AppendFormat("{0}=hex:", arg);
			ByteArrayToRegString(output, value.ToArray(), BinaryLineLength / 3);
			output.AppendLine();
		}

		public static void RegOutput(StringBuilder output, string name, int type, byte[] value)
		{
			string arg = NormalizeValueName(name);
			output.AppendFormat("{0}=hex({1:x}):", arg, type);
			ByteArrayToRegString(output, value.ToArray(), BinaryLineLength / 3);
			output.AppendLine();
		}
	}
}
