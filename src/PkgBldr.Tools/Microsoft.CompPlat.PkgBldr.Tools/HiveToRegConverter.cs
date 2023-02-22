using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public class HiveToRegConverter
	{
		private HashSet<string> m_exclusions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		private string m_keyPrefix;

		private string m_hiveFile;

		private TextWriter m_writer;

		[SuppressMessage("Microsoft.Design", "CA1026")]
		public HiveToRegConverter(string hiveFile, string keyPrefix = null)
		{
			if (string.IsNullOrEmpty(hiveFile))
			{
				throw new ArgumentNullException("hiveFile");
			}
			if (!LongPathFile.Exists(hiveFile))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "Hive file {0} does not exist or cannot be read", new object[1] { hiveFile }));
			}
			m_hiveFile = hiveFile;
			m_keyPrefix = keyPrefix;
		}

		[SuppressMessage("Microsoft.Design", "CA1026")]
		public void ConvertToReg(string outputFile, HashSet<string> exclusions = null, bool append = false)
		{
			if (string.IsNullOrEmpty(outputFile))
			{
				throw new ArgumentNullException("outputFile");
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

		[SuppressMessage("Microsoft.Design", "CA1026")]
		[SuppressMessage("Microsoft.Design", "CA1045")]
		public void ConvertToReg(ref StringBuilder outputStr, HashSet<string> exclusions = null)
		{
			ConvertToReg(ref outputStr, null, true, exclusions);
		}

		[SuppressMessage("Microsoft.Design", "CA1026")]
		[SuppressMessage("Microsoft.Design", "CA1045")]
		public void ConvertToReg(ref StringBuilder outputStr, string subKey, bool outputHeader, HashSet<string> exclusions = null)
		{
			if (outputStr == null)
			{
				throw new ArgumentNullException("outputStr");
			}
			if (exclusions != null)
			{
				m_exclusions.UnionWith(exclusions);
			}
			using (m_writer = new StringWriter(outputStr, CultureInfo.InvariantCulture))
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
			return string.Format(CultureInfo.InvariantCulture, ";Value={0}", new object[1] { value });
		}

		private void WriteKeyContents(ORRegistryKey key)
		{
			WriteKeyName(key.FullName);
			string @class = key.Class;
			if (!string.IsNullOrEmpty(@class))
			{
				m_writer.WriteLine(string.Format(CultureInfo.InvariantCulture, ";Class=\"{0}\"", new object[1] { @class }));
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
