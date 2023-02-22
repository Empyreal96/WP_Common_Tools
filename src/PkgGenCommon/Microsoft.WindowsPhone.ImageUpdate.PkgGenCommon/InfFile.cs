using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgGenCommon
{
	public class InfFile
	{
		private string m_infFilePath;

		private List<string> m_infLines;

		private static readonly Regex regexSectionEnd = new Regex("^\\[.*\\](.*)$", RegexOptions.Compiled);

		private static readonly Regex regexInfSddl = new Regex("HKR,,Security,,\"(?<sddl>.*)\"", RegexOptions.Compiled);

		private const string STR_SDDL_FORMAT = "HKR,,Security,,\"{0}\"";

		private const string STR_INF_COMMENT_START = ";";

		private const string STR_SECTION_START_PATTERN = "^\\[[ \\t]*{0}[ \\t]*\\](.*)$";

		private const string STR_SDDL_COMMENT = ";------ SDDL auto-updated from pkg.xml policy";

		public ISecurityPolicyCompiler SecurityCompiler { get; set; }

		public InfFile(string infFilePath)
		{
			m_infFilePath = infFilePath;
			m_infLines = new List<string>(LongPathFile.ReadAllLines(m_infFilePath));
		}

		public string GetSectionSddl(string infSectionName)
		{
			int sectionStart = GetSectionStart(infSectionName);
			if (sectionStart == -1)
			{
				throw new IUException("INF section {0} referenced in pkg xml not found in {1}, if encodings other than ANSI are used in the input inf file, please make sure right BOM is included.", infSectionName, m_infFilePath);
			}
			int sectionEnd = GetSectionEnd(sectionStart);
			string text = null;
			for (int i = sectionStart; i <= sectionEnd; i++)
			{
				string input = m_infLines[i];
				Match match = regexInfSddl.Match(input);
				if (match.Success)
				{
					if (text != null)
					{
						throw new IUException("More than one security SDDL strings are specified around line {0}, file {1}", i, m_infFilePath);
					}
					text = match.Groups["sddl"].Value;
				}
			}
			return text;
		}

		public void SetSectionSddl(string infSectionName, string infSddl)
		{
			int sectionStart = GetSectionStart(infSectionName);
			if (sectionStart == -1)
			{
				throw new IUException("INF section {0} referenced in pkg xml not found in {1}, if encodings other than ANSI are used in the input inf file, please make sure right BOM is included.", infSectionName, m_infFilePath);
			}
			int sectionEnd = GetSectionEnd(sectionStart);
			List<string> list = new List<string>(from x in m_infLines.Skip(sectionStart).Take(sectionEnd - sectionStart + 1)
				where !regexInfSddl.Match(x).Success
				select x);
			if (infSddl != null)
			{
				string item = $"HKR,,Security,,\"{infSddl}\"";
				list.Add(";------ SDDL auto-updated from pkg.xml policy");
				list.Add(item);
				list.Add(string.Empty);
			}
			m_infLines.RemoveRange(sectionStart, sectionEnd - sectionStart + 1);
			m_infLines.InsertRange(sectionStart, list);
		}

		public void UpdateSecurityPolicy(string infSectionName)
		{
			if (string.IsNullOrEmpty(infSectionName))
			{
				throw new ArgumentNullException("infSectionName");
			}
			if (SecurityCompiler == null)
			{
				throw new PkgGenException("SecurityCompiler property not initialized");
			}
			string sectionSddl = GetSectionSddl(infSectionName);
			sectionSddl = SecurityCompiler.GetDriverSddlString(infSectionName, sectionSddl);
			SetSectionSddl(infSectionName, sectionSddl);
		}

		public void SaveInf(string outputPath)
		{
			if (string.IsNullOrEmpty(outputPath))
			{
				throw new ArgumentNullException("outputPath");
			}
			try
			{
				LongPathFile.WriteAllLines(outputPath, m_infLines.ToArray());
			}
			catch (IOException innerException)
			{
				throw new PkgGenException(innerException, "Failed to write updated INF {0} to disk", outputPath);
			}
		}

		private int GetSectionStart(string sectionName)
		{
			bool flag = false;
			int i;
			for (i = 0; i < m_infLines.Count; i++)
			{
				string text = m_infLines[i].Trim();
				if (!text.TrimStart().StartsWith(";", StringComparison.InvariantCulture))
				{
					string pattern = $"^\\[[ \\t]*{sectionName}[ \\t]*\\](.*)$";
					if (Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				return -1;
			}
			return i;
		}

		private int GetSectionEnd(int sectionStart)
		{
			bool flag = false;
			int i;
			for (i = sectionStart + 1; i < m_infLines.Count; i++)
			{
				string text = m_infLines[i].Trim();
				if (!text.TrimStart().StartsWith(";", StringComparison.InvariantCulture) && regexSectionEnd.IsMatch(text))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return m_infLines.Count - 1;
			}
			return i - 1;
		}
	}
}
