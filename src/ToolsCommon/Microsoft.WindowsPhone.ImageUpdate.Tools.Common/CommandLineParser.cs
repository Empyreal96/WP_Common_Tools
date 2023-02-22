using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class CommandLineParser
	{
		internal abstract class CArgument
		{
			protected object m_val = "";

			protected bool m_fIsAssigned;

			private string m_id = "";

			private string m_description = "";

			private bool m_fIsOptional = true;

			public string Id => m_id;

			public string description
			{
				get
				{
					if (m_description.Length == 0)
					{
						return m_id;
					}
					return m_description;
				}
			}

			public bool isOptional => m_fIsOptional;

			public bool isAssigned => m_fIsAssigned;

			protected CArgument(string id, string desc, bool fIsOptional)
			{
				m_id = id;
				m_description = desc;
				m_fIsOptional = fIsOptional;
			}

			public object GetValue()
			{
				return m_val;
			}

			public abstract bool SetValue(string val);

			public abstract string possibleValues();
		}

		internal class CNumericArgument : CArgument
		{
			private double m_minRange = double.MinValue;

			private double m_maxRange = double.MaxValue;

			public CNumericArgument(string id, string desc, bool fIsOptional, double defVal, double minRange, double maxRange)
				: base(id, desc, fIsOptional)
			{
				m_val = defVal;
				m_minRange = minRange;
				m_maxRange = maxRange;
			}

			public override bool SetValue(string val)
			{
				bool isAssigned2 = base.isAssigned;
				m_fIsAssigned = true;
				try
				{
					if (val.ToLowerInvariant().StartsWith("0x", StringComparison.CurrentCulture))
					{
						m_val = (double)int.Parse(val.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
					}
					else
					{
						m_val = double.Parse(val, NumberStyles.Any, CultureInfo.InvariantCulture);
					}
				}
				catch (ArgumentNullException)
				{
					return false;
				}
				catch (FormatException)
				{
					return false;
				}
				catch (OverflowException)
				{
					return false;
				}
				if ((double)m_val >= m_minRange)
				{
					return (double)m_val <= m_maxRange;
				}
				return false;
			}

			public override string possibleValues()
			{
				return "between " + m_minRange + " and " + m_maxRange;
			}
		}

		internal class CStringArgument : CArgument
		{
			private string[] m_possibleVals = new string[1] { "" };

			private bool m_fIsPossibleValsCaseSensitive = true;

			public CStringArgument(string id, string desc, bool fIsOptional, string defVal, bool isPossibleValuesCaseSensitive, params string[] possibleValues)
				: base(id, desc, fIsOptional)
			{
				m_possibleVals = possibleValues;
				m_val = defVal;
				m_fIsPossibleValsCaseSensitive = isPossibleValuesCaseSensitive;
			}

			public override bool SetValue(string val)
			{
				bool isAssigned2 = base.isAssigned;
				m_fIsAssigned = true;
				m_val = val;
				if (m_possibleVals.Length == 0)
				{
					return true;
				}
				string[] possibleVals = m_possibleVals;
				foreach (string text in possibleVals)
				{
					if ((string)m_val == text || (!m_fIsPossibleValsCaseSensitive && string.Compare((string)m_val, text, StringComparison.OrdinalIgnoreCase) == 0))
					{
						return true;
					}
				}
				return false;
			}

			public override string possibleValues()
			{
				if (m_possibleVals.Length == 0)
				{
					return "free text";
				}
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("{");
				stringBuilder.Append(m_fIsPossibleValsCaseSensitive ? m_possibleVals[0] : m_possibleVals[0].ToLowerInvariant());
				for (int i = 1; i < m_possibleVals.Length; i++)
				{
					stringBuilder.Append("|");
					stringBuilder.Append(m_fIsPossibleValsCaseSensitive ? m_possibleVals[i] : m_possibleVals[i].ToLowerInvariant());
				}
				stringBuilder.Append("}");
				return stringBuilder.ToString();
			}
		}

		internal class CBooleanArgument : CArgument
		{
			public CBooleanArgument(string id, string desc, bool fIsOptional, bool defVal)
				: base(id, desc, fIsOptional)
			{
				m_val = defVal;
			}

			public override bool SetValue(string token)
			{
				bool isAssigned2 = base.isAssigned;
				m_fIsAssigned = true;
				m_val = token != "-";
				return true;
			}

			public override string possibleValues()
			{
				return "precede by [+] or [-]";
			}
		}

		internal class CArgGroups
		{
			public uint m_minAppear;

			public uint m_maxAppear;

			private string[] m_args;

			public string[] Args => m_args;

			public CArgGroups(uint min, uint max, params string[] args)
			{
				m_minAppear = min;
				m_maxAppear = max;
				m_args = args;
			}

			public bool InRange(uint num)
			{
				if (num >= m_minAppear)
				{
					return num <= m_maxAppear;
				}
				return false;
			}

			public string ArgList()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("{");
				string[] args = Args;
				foreach (string value in args)
				{
					stringBuilder.Append(",");
					stringBuilder.Append(value);
				}
				return stringBuilder.ToString().Replace("{,", "{") + "}";
			}

			public string RangeDescription()
			{
				if (m_minAppear == 1 && m_maxAppear == 1)
				{
					return "one of the switches " + ArgList() + " must be used exclusively";
				}
				if (m_minAppear == 1 && m_maxAppear == Args.Length)
				{
					return "one or more of the switches " + ArgList() + " must be used";
				}
				if (m_minAppear == 1 && m_maxAppear > 1)
				{
					return "one (but not more than " + m_maxAppear + ") of the switches " + ArgList() + " must be used";
				}
				if (m_minAppear == 0 && m_maxAppear == 1)
				{
					return "only one of the switches " + ArgList() + " can be used";
				}
				if (m_minAppear == 0 && m_maxAppear > 1)
				{
					return "only " + m_maxAppear + " of the switches " + ArgList() + " can be used";
				}
				return "between " + m_minAppear + " and " + m_maxAppear + " of the switches " + ArgList() + " must be used";
			}
		}

		private const string c_applicationNameString = "RESERVED_ID_APPLICATION_NAME";

		private char m_switchChar = '/';

		private char m_delimChar = ':';

		private string m_Syntax = "";

		private List<CArgument> m_declaredSwitches = new List<CArgument>();

		private List<CArgument> m_declaredParams = new List<CArgument>();

		private uint m_iRequiredParams;

		private List<CArgGroups> m_argGroups = new List<CArgGroups>();

		private SortedList<string, string> m_aliases = new SortedList<string, string>();

		private bool m_caseSensitive;

		private string m_lastError = "";

		private string m_usageCmdLine = "";

		private string m_usageArgs = "";

		private string m_usageGroups = "";

		private bool m_parseSuccess;

		private const char DEFAULT_SWITCH = '/';

		private const char DEFAULT_DELIM = ':';

		private const string SWITCH_TOKEN = "switchToken";

		private const string ID_TOKEN = "idToken";

		private const string DELIM_TOKEN = "delimToken";

		private const string VALUE_TOKEN = "valueToken";

		private const int USAGE_COL1 = 25;

		private const int USAGE_WIDTH = 79;

		public bool CaseSensitive
		{
			get
			{
				return m_caseSensitive;
			}
			set
			{
				m_caseSensitive = value;
				CheckNotAmbiguous();
			}
		}

		public string LastError
		{
			get
			{
				if (m_lastError.Length == 0)
				{
					return "There was no error";
				}
				return m_lastError;
			}
		}

		public CommandLineParser()
		{
			BuildRegularExpression();
		}

		public CommandLineParser(char yourOwnSwitch, char yourOwnDelimiter)
			: this()
		{
			m_switchChar = yourOwnSwitch;
			m_delimChar = yourOwnDelimiter;
		}

		public void SetOptionalSwitchNumeric(string id, string description, double defaultValue, double minRange, double maxRange)
		{
			DeclareNumericSwitch(id, description, true, defaultValue, minRange, maxRange);
		}

		public void SetOptionalSwitchNumeric(string id, string description, double defaultValue)
		{
			DeclareNumericSwitch(id, description, true, defaultValue, -2147483648.0, 2147483647.0);
		}

		public void SetRequiredSwitchNumeric(string id, string description, double minRange, double maxRange)
		{
			DeclareNumericSwitch(id, description, false, 0.0, minRange, maxRange);
		}

		public void SetRequiredSwitchNumeric(string id, string description)
		{
			DeclareNumericSwitch(id, description, false, 0.0, -2147483648.0, 2147483647.0);
		}

		public void SetOptionalSwitchString(string id, string description, string defaultValue, params string[] possibleValues)
		{
			DeclareStringSwitch(id, description, true, defaultValue, true, possibleValues);
		}

		public void SetOptionalSwitchString(string id, string description, string defaultValue, bool isPossibleValuesCaseSensitive, params string[] possibleValues)
		{
			DeclareStringSwitch(id, description, true, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
		}

		public void SetOptionalSwitchString(string id, string description)
		{
			DeclareStringSwitch(id, description, true, "", true);
		}

		public void SetRequiredSwitchString(string id, string description, params string[] possibleValues)
		{
			DeclareStringSwitch(id, description, false, "", true, possibleValues);
		}

		public void SetRequiredSwitchString(string id, string description, bool isPossibleValuesCaseSensitive, params string[] possibleValues)
		{
			DeclareStringSwitch(id, description, false, "", isPossibleValuesCaseSensitive, possibleValues);
		}

		public void SetRequiredSwitchString(string id, string description)
		{
			DeclareStringSwitch(id, description, false, "", true);
		}

		public void SetOptionalSwitchBoolean(string id, string description, bool defaultValue)
		{
			DeclareBooleanSwitch(id, description, true, defaultValue);
		}

		public void SetOptionalParameterNumeric(string id, string description, double defaultValue, double minRange, double maxRange)
		{
			DeclareParam_Numeric(id, description, true, defaultValue, minRange, maxRange);
		}

		public void SetOptionalParameterNumeric(string id, string description, double defaultValue)
		{
			DeclareParam_Numeric(id, description, true, defaultValue, -2147483648.0, 2147483647.0);
		}

		public void SetRequiredParameterNumeric(string id, string description, double minRange, double maxRange)
		{
			DeclareParam_Numeric(id, description, false, 0.0, minRange, maxRange);
		}

		public void SetRequiredParameterNumeric(string id, string description)
		{
			DeclareParam_Numeric(id, description, false, 0.0, -2147483648.0, 2147483647.0);
		}

		public void SetOptionalParameterString(string id, string description, string defaultValue, params string[] possibleValues)
		{
			DeclareStringParam(id, description, true, defaultValue, true, possibleValues);
		}

		public void SetOptionalParameterString(string id, string description, string defaultValue, bool isPossibleValuesCaseSensitive, params string[] possibleValues)
		{
			DeclareStringParam(id, description, true, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
		}

		public void SetOptionalParameterString(string id, string description)
		{
			DeclareStringParam(id, description, true, "", true);
		}

		public void SetRequiredParameterString(string id, string description, params string[] possibleValues)
		{
			DeclareStringParam(id, description, false, "", true, possibleValues);
		}

		public void SetRequiredParameterString(string id, string description, bool isPossibleValuesCaseSensitive, params string[] possibleValues)
		{
			DeclareStringParam(id, description, false, "", isPossibleValuesCaseSensitive, possibleValues);
		}

		public void SetRequiredParameterString(string id, string description)
		{
			DeclareStringParam(id, description, false, "", true);
		}

		public bool ParseCommandLine()
		{
			SetFirstArgumentAsAppName();
			return ParseString(Environment.CommandLine);
		}

		public bool ParseString(string argumentsLine, bool isFirstArgTheAppName)
		{
			if (isFirstArgTheAppName)
			{
				SetFirstArgumentAsAppName();
			}
			return ParseString(argumentsLine);
		}

		public bool ParseString(string argumentsLine)
		{
			if (argumentsLine == null)
			{
				throw new ArgumentNullException("argumentsLine");
			}
			if (m_parseSuccess)
			{
				throw new ParseFailedException("Cannot parse twice!");
			}
			SetOptionalSwitchBoolean("?", "Displays this usage string", false);
			int num = 0;
			argumentsLine = argumentsLine.TrimStart() + " ";
			Match match = new Regex(m_Syntax).Match(argumentsLine);
			while (match.Success)
			{
				string token = match.Result("${switchToken}");
				string text = match.Result("${idToken}");
				string delim = match.Result("${delimToken}");
				string text2 = match.Result("${valueToken}");
				text2 = text2.TrimEnd();
				if (text2.StartsWith("\"", StringComparison.CurrentCulture) && text2.EndsWith("\"", StringComparison.CurrentCulture))
				{
					text2 = text2.Substring(1, text2.Length - 2);
				}
				if (text.Length == 0)
				{
					if (!InputParam(text2, num++))
					{
						return false;
					}
				}
				else
				{
					if (text == "?")
					{
						m_lastError = "Usage Info requested";
						m_parseSuccess = false;
						return false;
					}
					if (!InputSwitch(token, text, delim, text2))
					{
						return false;
					}
				}
				match = match.NextMatch();
			}
			foreach (CArgument declaredSwitch in m_declaredSwitches)
			{
				if (!declaredSwitch.isOptional && !declaredSwitch.isAssigned)
				{
					m_lastError = "Required switch '" + declaredSwitch.Id + "' was not assigned a value";
					return false;
				}
			}
			foreach (CArgument declaredParam in m_declaredParams)
			{
				if (!declaredParam.isOptional && !declaredParam.isAssigned)
				{
					m_lastError = "Required parameter '" + declaredParam.Id + "' was not assigned a value";
					return false;
				}
			}
			m_parseSuccess = IsGroupRulesKept();
			return m_parseSuccess;
		}

		public object GetSwitch(string id)
		{
			if (!m_parseSuccess)
			{
				throw new ParseFailedException(LastError);
			}
			if (id == "RESERVED_ID_APPLICATION_NAME")
			{
				throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used");
			}
			CArgument cArgument = FindExactArg(id, m_declaredSwitches);
			if (cArgument == null)
			{
				throw new NoSuchArgumentException("switch", id);
			}
			return cArgument.GetValue();
		}

		public double GetSwitchAsNumeric(string id)
		{
			return (double)GetSwitch(id);
		}

		public string GetSwitchAsString(string id)
		{
			return (string)GetSwitch(id);
		}

		public bool GetSwitchAsBoolean(string id)
		{
			return (bool)GetSwitch(id);
		}

		public bool IsAssignedSwitch(string id)
		{
			if (!m_parseSuccess)
			{
				throw new ParseFailedException(LastError);
			}
			if (id == "RESERVED_ID_APPLICATION_NAME")
			{
				throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used");
			}
			CArgument cArgument = FindExactArg(id, m_declaredSwitches);
			if (cArgument == null)
			{
				throw new NoSuchArgumentException("switch", id);
			}
			return cArgument.isAssigned;
		}

		public object GetParameter(string id)
		{
			if (!m_parseSuccess)
			{
				throw new ParseFailedException(LastError);
			}
			if (id == "RESERVED_ID_APPLICATION_NAME")
			{
				throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used");
			}
			CArgument cArgument = FindExactArg(id, m_declaredParams);
			if (cArgument == null)
			{
				throw new NoSuchArgumentException("parameter", id);
			}
			return cArgument.GetValue();
		}

		public double GetParameterAsNumeric(string id)
		{
			return (double)GetParameter(id);
		}

		public string GetParameterAsString(string id)
		{
			return (string)GetParameter(id);
		}

		public bool IsAssignedParameter(string id)
		{
			if (!m_parseSuccess)
			{
				throw new ParseFailedException(LastError);
			}
			if (id == "RESERVED_ID_APPLICATION_NAME")
			{
				throw new ParseException("RESERVED_ID_APPLICATION_NAME is a reserved internal id and must not be used");
			}
			CArgument cArgument = FindExactArg(id, m_declaredParams);
			if (cArgument == null)
			{
				throw new NoSuchArgumentException("parameter", id);
			}
			return cArgument.isAssigned;
		}

		public object[] GetParameterList()
		{
			int num = (IsFirstArgumentAppName() ? 1 : 0);
			if (m_declaredParams.Count == num)
			{
				return null;
			}
			object[] array = new object[m_declaredParams.Count - num];
			for (int i = num; i < m_declaredParams.Count; i++)
			{
				array[i - num] = m_declaredParams[i].GetValue();
			}
			return array;
		}

		public Array SwitchesList()
		{
			Array array = Array.CreateInstance(typeof(object), m_declaredSwitches.Count, 2);
			for (int i = 0; i < m_declaredSwitches.Count; i++)
			{
				array.SetValue(m_declaredSwitches[i].Id, i, 1);
				array.SetValue(m_declaredSwitches[i].GetValue(), i, 0);
			}
			return array;
		}

		public void SetAlias(string alias, string treatedAs)
		{
			if (alias != treatedAs)
			{
				m_aliases[alias] = treatedAs;
			}
		}

		public void DefineSwitchGroup(uint minAppear, uint maxAppear, params string[] ids)
		{
			if (ids == null)
			{
				throw new ArgumentNullException("ids");
			}
			if (ids.Length < 2 || maxAppear < minAppear || maxAppear == 0)
			{
				throw new BadGroupException("A group must have at least two members");
			}
			if (minAppear == 0 && maxAppear == ids.Length)
			{
				return;
			}
			if (minAppear > ids.Length)
			{
				throw new BadGroupException(string.Format(CultureInfo.InvariantCulture, "You cannot have {0} appearance(s) in a group of {1} switch(es)!", new object[2] { minAppear, ids.Length }));
			}
			foreach (string text in ids)
			{
				if (FindExactArg(text, m_declaredSwitches) == null)
				{
					throw new NoSuchArgumentException("switch", text);
				}
			}
			CArgGroups cArgGroups = new CArgGroups(minAppear, maxAppear, ids);
			m_argGroups.Add(cArgGroups);
			if (m_usageGroups.Length == 0)
			{
				m_usageGroups = "NOTES:" + Environment.NewLine;
			}
			m_usageGroups = m_usageGroups + " - " + cArgGroups.RangeDescription() + Environment.NewLine;
		}

		public string UsageString()
		{
			return UsageString(new FileInfo(Environment.GetCommandLineArgs()[0]).Name);
		}

		public string UsageString(string appName)
		{
			string text = "";
			if (m_lastError.Length != 0)
			{
				text = ">> " + m_lastError + Environment.NewLine + Environment.NewLine;
			}
			return text + "Usage: " + appName + m_usageCmdLine + Environment.NewLine + Environment.NewLine + m_usageArgs + Environment.NewLine + m_usageGroups + Environment.NewLine;
		}

		private void SetFirstArgumentAsAppName()
		{
			if (m_declaredParams.Count <= 0 || !(m_declaredParams[0].Id == "RESERVED_ID_APPLICATION_NAME"))
			{
				CheckNotAmbiguous("RESERVED_ID_APPLICATION_NAME");
				CArgument item = new CStringArgument("RESERVED_ID_APPLICATION_NAME", "the application's name", false, "", true);
				m_declaredParams.Insert(0, item);
				m_iRequiredParams++;
			}
		}

		private void BuildRegularExpression()
		{
			m_Syntax = "\\G((?<switchToken>[\\+\\-" + m_switchChar + "]{1})(?<idToken>[\\w|?]+)(?<delimToken>[" + m_delimChar + "]?))?(?<valueToken>(\"[^\"]*\"|\\S*)\\s+){1}";
		}

		private void DeclareNumericSwitch(string id, string description, bool fIsOptional, double defaultValue, double minRange, double maxRange)
		{
			if (id.Length == 0)
			{
				throw new EmptyArgumentDeclaredException();
			}
			CheckNotAmbiguous(id);
			CArgument cArgument = new CNumericArgument(id, description, fIsOptional, defaultValue, minRange, maxRange);
			m_declaredSwitches.Add(cArgument);
			AddUsageInfo(cArgument, true, defaultValue);
		}

		private void DeclareStringSwitch(string id, string description, bool fIsOptional, string defaultValue, bool isPossibleValuesCaseSensitive, params string[] possibleValues)
		{
			if (id.Length == 0)
			{
				throw new EmptyArgumentDeclaredException();
			}
			CheckNotAmbiguous(id);
			CArgument cArgument = new CStringArgument(id, description, fIsOptional, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
			m_declaredSwitches.Add(cArgument);
			AddUsageInfo(cArgument, true, defaultValue);
		}

		private void DeclareBooleanSwitch(string id, string description, bool fIsOptional, bool defaultValue)
		{
			if (id.Length == 0)
			{
				throw new EmptyArgumentDeclaredException();
			}
			CheckNotAmbiguous(id);
			CArgument cArgument = new CBooleanArgument(id, description, fIsOptional, defaultValue);
			m_declaredSwitches.Add(cArgument);
			AddUsageInfo(cArgument, true, defaultValue);
		}

		private void DeclareParam_Numeric(string id, string description, bool fIsOptional, double defaultValue, double minRange, double maxRange)
		{
			if (id.Length == 0)
			{
				throw new EmptyArgumentDeclaredException();
			}
			if (!fIsOptional && m_declaredParams.Count > m_iRequiredParams)
			{
				throw new RequiredParameterAfterOptionalParameterException();
			}
			CheckNotAmbiguous(id);
			CArgument cArgument = new CNumericArgument(id, description, fIsOptional, defaultValue, minRange, maxRange);
			if (!fIsOptional)
			{
				m_iRequiredParams++;
			}
			m_declaredParams.Add(cArgument);
			AddUsageInfo(cArgument, false, defaultValue);
		}

		private void DeclareStringParam(string id, string description, bool fIsOptional, string defaultValue, bool isPossibleValuesCaseSensitive, params string[] possibleValues)
		{
			if (id.Length == 0)
			{
				throw new EmptyArgumentDeclaredException();
			}
			if (!fIsOptional && m_declaredParams.Count > m_iRequiredParams)
			{
				throw new RequiredParameterAfterOptionalParameterException();
			}
			CheckNotAmbiguous(id);
			CArgument cArgument = new CStringArgument(id, description, fIsOptional, defaultValue, isPossibleValuesCaseSensitive, possibleValues);
			if (!fIsOptional)
			{
				m_iRequiredParams++;
			}
			m_declaredParams.Add(cArgument);
			AddUsageInfo(cArgument, false, defaultValue);
		}

		private void AddUsageInfo(CArgument arg, bool isSwitch, object defVal)
		{
			m_usageCmdLine += (arg.isOptional ? " [" : " ");
			if (isSwitch)
			{
				if (arg.GetType() != typeof(CBooleanArgument))
				{
					m_usageCmdLine = m_usageCmdLine + m_switchChar + arg.Id + m_delimChar + "x";
				}
				else if (arg.Id == "?")
				{
					m_usageCmdLine = m_usageCmdLine + m_switchChar + "?";
				}
				else
				{
					m_usageCmdLine = m_usageCmdLine + "[+|-]" + arg.Id;
				}
			}
			else
			{
				m_usageCmdLine += arg.Id;
			}
			m_usageCmdLine += (arg.isOptional ? "]" : "");
			string text = ((arg.Id == "?" || (isSwitch && arg.GetType() != typeof(CBooleanArgument))) ? m_switchChar.ToString() : "") + arg.Id;
			if (arg.isOptional)
			{
				text = "[" + text + "]";
			}
			text = "  " + text.PadRight(22, '·') + " ";
			text += arg.description;
			if (arg.Id != "?")
			{
				text = text + ". Values: " + arg.possibleValues();
				if (arg.isOptional)
				{
					text = text + "; default= " + defVal.ToString();
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			while (text.Length > 0)
			{
				if (text.Length <= 79)
				{
					m_usageArgs = m_usageArgs + text + Environment.NewLine;
					break;
				}
				int num = 79;
				while (num > 69 && text[num] != ' ')
				{
					num--;
				}
				if (num <= 69)
				{
					num = 79;
				}
				m_usageArgs = m_usageArgs + text.Substring(0, num) + Environment.NewLine;
				text = text.Substring(num).TrimStart();
				if (text.Length > 0)
				{
					stringBuilder.Append("".PadLeft(25, ' '));
					stringBuilder.Append(text);
					text = stringBuilder.ToString();
					stringBuilder.Remove(0, stringBuilder.Length);
				}
			}
		}

		private bool InputSwitch(string token, string ID, string delim, string val)
		{
			if (m_aliases.ContainsKey(ID))
			{
				ID = m_aliases[ID];
			}
			CArgument cArgument = FindSimilarArg(ID, m_declaredSwitches);
			if (cArgument == null)
			{
				return false;
			}
			if (cArgument.GetType() == typeof(CBooleanArgument))
			{
				cArgument.SetValue(token);
				if (delim.Length != 0 || val.Length != 0)
				{
					m_lastError = "A boolean switch cannot be followed by a delimiter. Use \"-booleanFlag\", not \"-booleanFlag" + m_delimChar + "\"";
					return false;
				}
				return true;
			}
			if (delim.Length == 0)
			{
				m_lastError = "you must use the delimiter '" + m_delimChar + "', e.g. \"" + m_switchChar + "arg" + m_delimChar + "x\"";
				return false;
			}
			if (cArgument.SetValue(val))
			{
				return true;
			}
			m_lastError = "Switch '" + ID + "' cannot accept '" + val + "' as a value";
			return false;
		}

		private bool InputParam(string val, int paramIndex)
		{
			if (int.MaxValue == paramIndex)
			{
				m_lastError = "paramIndex must be less than Int32.MaxValue";
				return false;
			}
			if (m_declaredParams.Count < paramIndex + 1)
			{
				m_lastError = "Command-line has too many parameters";
				return false;
			}
			CArgument cArgument = m_declaredParams[paramIndex];
			if (cArgument.SetValue(val))
			{
				return true;
			}
			m_lastError = "Parameter '" + cArgument.Id + "' did not have a legal value";
			return false;
		}

		private CArgument FindExactArg(string argID, List<CArgument> list)
		{
			foreach (CArgument item in list)
			{
				if (string.Compare(item.Id, argID, (!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
				{
					return item;
				}
			}
			return null;
		}

		private CArgument FindSimilarArg(string argSubstringID, List<CArgument> list)
		{
			argSubstringID = (CaseSensitive ? argSubstringID : argSubstringID.ToUpper(CultureInfo.InvariantCulture));
			CArgument cArgument = null;
			foreach (CArgument item in list)
			{
				string text = (CaseSensitive ? item.Id : item.Id.ToUpper(CultureInfo.InvariantCulture));
				if (text.StartsWith(argSubstringID, StringComparison.CurrentCulture))
				{
					if (cArgument != null)
					{
						string text2 = (CaseSensitive ? cArgument.Id : cArgument.Id.ToUpper(CultureInfo.InvariantCulture));
						m_lastError = "Ambiguous ID: '" + argSubstringID + "' matches both '" + text2 + "' and '" + text + "'";
						return null;
					}
					cArgument = item;
				}
			}
			if (cArgument == null)
			{
				m_lastError = "No such argument '" + argSubstringID + "'";
			}
			return cArgument;
		}

		private void CheckNotAmbiguous()
		{
			CheckNotAmbiguous("");
		}

		private void CheckNotAmbiguous(string newId)
		{
			CheckNotAmbiguous(newId, m_declaredSwitches);
			CheckNotAmbiguous(newId, m_declaredParams);
		}

		private void CheckNotAmbiguous(string newID, List<CArgument> argList)
		{
			foreach (CArgument arg in argList)
			{
				if (string.Compare(arg.Id, newID, (!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
				{
					throw new ArgumentAlreadyDeclaredException(arg.Id);
				}
				if (newID.Length != 0 && (arg.Id.StartsWith(newID, (!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) || newID.StartsWith(arg.Id, (!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)))
				{
					throw new AmbiguousArgumentException(arg.Id, newID);
				}
				foreach (CArgument arg2 in argList)
				{
					if (!arg.Equals(arg2))
					{
						if (string.Compare(arg.Id, arg2.Id, (!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
						{
							throw new ArgumentAlreadyDeclaredException(arg.Id);
						}
						if (arg.Id.StartsWith(arg2.Id, (!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) || arg2.Id.StartsWith(arg.Id, (!CaseSensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
						{
							throw new AmbiguousArgumentException(arg.Id, arg2.Id);
						}
					}
				}
			}
		}

		private bool IsGroupRulesKept()
		{
			foreach (CArgGroups argGroup in m_argGroups)
			{
				uint num = 0u;
				string[] args = argGroup.Args;
				foreach (string argID in args)
				{
					CArgument cArgument = FindExactArg(argID, m_declaredSwitches);
					if (cArgument != null && cArgument.isAssigned)
					{
						num++;
					}
				}
				if (!argGroup.InRange(num))
				{
					m_lastError = argGroup.RangeDescription();
					return false;
				}
			}
			return true;
		}

		private bool IsFirstArgumentAppName()
		{
			if (m_declaredParams.Count > 0)
			{
				return m_declaredParams[0].Id == "RESERVED_ID_APPLICATION_NAME";
			}
			return false;
		}
	}
}
