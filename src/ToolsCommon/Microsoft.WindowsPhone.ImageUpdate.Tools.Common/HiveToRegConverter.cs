using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class HiveToRegConverter
	{
		private HashSet<string> m_exclusions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private string m_keyPrefix;

		private string m_hiveFile;

		private TextWriter m_writer;

		public HiveToRegConverter(string hiveFile)
		{
			VerifyHiveFileInput(hiveFile);
			m_hiveFile = hiveFile;
			m_keyPrefix = null;
		}

		public HiveToRegConverter(string hiveFile, string keyPrefix)
		{
			VerifyHiveFileInput(hiveFile);
			m_hiveFile = hiveFile;
			m_keyPrefix = keyPrefix;
		}

		public void VerifyHiveFileInput(string hiveFile)
		{
			if (string.IsNullOrEmpty(hiveFile))
			{
				throw new ArgumentNullException("hiveFile", "HiveFile cannot be null.");
			}
			if (!LongPathFile.Exists(hiveFile))
			{
				throw new FileNotFoundException($"Hive file {hiveFile} does not exist or cannot be read");
			}
		}

		public void ConvertToReg(string outputFile)
		{
			ConvertToReg(outputFile, null, false);
		}

		public void ConvertToReg(string outputFile, HashSet<string> exclusions)
		{
			ConvertToReg(outputFile, exclusions, false);
		}

		public void ConvertToReg(string outputFile, HashSet<string> exclusions, bool append)
		{
			if (string.IsNullOrEmpty(outputFile))
			{
				throw new ArgumentNullException("outputFile", "Output file cannot be empty.");
			}
			if (exclusions != null)
			{
				m_exclusions.UnionWith(exclusions);
			}
			FileMode mode = (append ? FileMode.Append : FileMode.Create);
			using (m_writer = new StreamWriter(LongPathFile.Open(outputFile, mode, FileAccess.Write), Encoding.Unicode))
			{
				ConvertToStream(!append, null);
			}
		}

		public void ConvertToReg(ref StringBuilder outputStr)
		{
			ConvertToReg(ref outputStr, null);
		}

		public void ConvertToReg(ref StringBuilder outputStr, HashSet<string> exclusions)
		{
			ConvertToReg(ref outputStr, null, true, exclusions);
		}

		public void ConvertToReg(ref StringBuilder outputStr, string subKey, bool outputHeader)
		{
			ConvertToReg(ref outputStr, null, true, null);
		}

		public void ConvertToReg(ref StringBuilder outputStr, string subKey, bool outputHeader, HashSet<string> exclusions)
		{
			if (outputStr == null)
			{
				throw new ArgumentNullException("outputStr");
			}
			if (exclusions != null)
			{
				m_exclusions.UnionWith(exclusions);
			}
			using (m_writer = new StringWriter(outputStr))
			{
				ConvertToStream(outputHeader, subKey);
			}
		}

		private void ConvertToStream(bool outputHeader, string subKey)
		{
			if (outputHeader)
			{
				m_writer.WriteLine("Windows Registry Editor Version 5.00");
			}
			using (ORRegistryKey oRRegistryKey = ORRegistryKey.OpenHive(m_hiveFile, m_keyPrefix))
			{
				ORRegistryKey oRRegistryKey2 = oRRegistryKey;
				if (!string.IsNullOrEmpty(subKey))
				{
					oRRegistryKey2 = oRRegistryKey.OpenSubKey(subKey);
				}
				WriteKeyContents(oRRegistryKey2);
				WalkHive(oRRegistryKey2);
			}
		}

		private void WalkHive(ORRegistryKey root)
		{
			foreach (string item in root.SubKeys.OrderBy((string x) => x, StringComparer.OrdinalIgnoreCase))
			{
				using (ORRegistryKey oRRegistryKey = root.OpenSubKey(item))
				{
					try
					{
						bool num = m_exclusions.Contains(oRRegistryKey.FullName + "\\*");
						bool flag = m_exclusions.Contains(oRRegistryKey.FullName);
						if (!num)
						{
							if (!flag)
							{
								WriteKeyContents(oRRegistryKey);
							}
							WalkHive(oRRegistryKey);
						}
					}
					catch (Exception innerException)
					{
						throw new IUException("Failed to iterate through hive", innerException);
					}
				}
			}
		}

		private void WriteKeyName(string keyname)
		{
			m_writer.WriteLine();
			m_writer.WriteLine("[{0}]", keyname);
		}

		private string FormatValueName(string valueName)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (valueName.Equals(""))
			{
				stringBuilder.Append("@=");
			}
			else
			{
				StringBuilder stringBuilder2 = new StringBuilder(valueName);
				stringBuilder2.Replace("\\", "\\\\").Replace("\"", "\\\"");
				stringBuilder.AppendFormat("\"{0}\"=", stringBuilder2.ToString());
			}
			return stringBuilder.ToString();
		}

		private string FormatValue(ORRegistryKey key, string valueName)
		{
			RegistryValueType valueKind = key.GetValueKind(valueName);
			StringBuilder stringBuilder = new StringBuilder();
			switch (valueKind)
			{
			case RegistryValueType.DWord:
			{
				uint dwordValue = key.GetDwordValue(valueName);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "dword:{0:X8}", new object[1] { dwordValue });
				break;
			}
			case RegistryValueType.MultiString:
			{
				byte[] byteValue2 = key.GetByteValue(valueName);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "hex(7):{0}", new object[1] { OfflineRegUtils.ConvertByteArrayToRegStrings(byteValue2) });
				string[] multiStringValue = key.GetMultiStringValue(valueName);
				stringBuilder.AppendLine();
				stringBuilder.AppendLine(GetMultiStringValuesAsComments(multiStringValue));
				break;
			}
			case RegistryValueType.String:
			{
				StringBuilder stringBuilder2 = new StringBuilder();
				stringBuilder2.Append(key.GetStringValue(valueName));
				stringBuilder2.Replace("\\", "\\\\").Replace("\"", "\\\"");
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "\"{0}\"", new object[1] { stringBuilder2.ToString() });
				break;
			}
			default:
			{
				byte[] byteValue = key.GetByteValue(valueName);
				stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "hex({0,1:X}):{1}", new object[2]
				{
					valueKind,
					OfflineRegUtils.ConvertByteArrayToRegStrings(byteValue)
				});
				if (valueKind == RegistryValueType.ExpandString)
				{
					stringBuilder.AppendLine();
					stringBuilder.AppendLine(GetExpandStringValueAsComments(key.GetStringValue(valueName)));
				}
				break;
			}
			}
			return stringBuilder.ToString();
		}

		private string GetMultiStringValuesAsComments(string[] values)
		{
			StringBuilder stringBuilder = new StringBuilder(500);
			int num = 80;
			if (values != null && values.Length != 0)
			{
				stringBuilder.Append(";Values=");
				int num2 = stringBuilder.Length;
				foreach (string text in values)
				{
					stringBuilder.AppendFormat("{0},", text);
					num2 += text.Length + 1;
					if (num2 > num)
					{
						stringBuilder.AppendLine();
						stringBuilder.Append(";");
						num2 = 1;
					}
				}
				stringBuilder.Replace(",", string.Empty, stringBuilder.Length - 1, 1);
			}
			return stringBuilder.ToString();
		}

		private string GetExpandStringValueAsComments(string value)
		{
			return $";Value={value}";
		}

		private void WriteKeyContents(ORRegistryKey key)
		{
			WriteKeyName(key.FullName);
			string @class = key.Class;
			if (!string.IsNullOrEmpty(@class))
			{
				m_writer.WriteLine($";Class=\"{@class}\"");
			}
			foreach (string item in key.ValueNames.OrderBy((string x) => x, StringComparer.OrdinalIgnoreCase))
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(FormatValueName(item));
				stringBuilder.Append(FormatValue(key, item));
				m_writer.WriteLine(stringBuilder.ToString());
			}
		}
	}
}
