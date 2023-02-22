using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.CompPlat.PkgBldr.Tools;

namespace Microsoft.CompPlat.PkgBldr.Base
{
	public class MacroResolver : MacroStack, IMacroResolver
	{
		private class MatchEvaluator
		{
			private MacroResolveOptions _option = MacroResolveOptions.ValidWhenEqual;

			private MacroStack _macroStack;

			public int MatchCount { get; private set; }

			public string Evaluate(Match match)
			{
				string value = _macroStack.GetValue(match.Groups["name"].Value);
				if (value == null)
				{
					if (_option != 0)
					{
						throw new PkgGenException("Undefined variable {0}", match.Groups["name"].Value);
					}
					value = match.Groups[0].Value;
				}
				else
				{
					MatchCount++;
				}
				return value;
			}

			public MatchEvaluator(MacroStack macroStack, MacroResolveOptions option)
			{
				_macroStack = macroStack;
				_option = option;
			}
		}

		private const int _maxResolveCount = 99;

		private Regex _varReferencePattern = new Regex("\\$\\((?<name>.*?)\\)");

		private Regex _varNamePattern = new Regex("^[A-Za-z.0-9_{-][A-Za-z.0-9_+{}-]*$");

		private MacroStack _macroStack = new MacroStack();

		public void BeginLocal()
		{
			_dictionaries.Push(new Dictionary<string, Macro>(StringComparer.OrdinalIgnoreCase));
		}

		public void Register(string name, string value)
		{
			Register(name, value, false);
		}

		public void Register(string name, object value, MacroDelegate del)
		{
			Register(name, value, del, false);
		}

		public void Register(IEnumerable<KeyValuePair<string, Macro>> values, bool allowOverride = false)
		{
			if (values == null)
			{
				return;
			}
			foreach (KeyValuePair<string, Macro> value in values)
			{
				Register(value.Key, value.Value.Value, value.Value.Delegate, allowOverride);
			}
		}

		public Dictionary<string, Macro> GetMacroTable()
		{
			Dictionary<string, Macro> dictionary = new Dictionary<string, Macro>();
			foreach (KeyValuePair<string, Macro> item in _dictionaries.Peek())
			{
				dictionary[item.Key] = item.Value;
			}
			return dictionary;
		}

		public void EndLocal()
		{
			if (_dictionaries.Count > 0)
			{
				_dictionaries.Pop();
			}
		}

		public string Resolve(string input)
		{
			return Resolve(input, MacroResolveOptions.ValidWhenEqual);
		}

		public bool PassThrough(string input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}
			return input.StartsWith("$", StringComparison.OrdinalIgnoreCase);
		}

		public string Resolve(string input, MacroResolveOptions option)
		{
			if (string.IsNullOrEmpty(input))
			{
				return input;
			}
			int i = 0;
			string text = input;
			for (; i < 99; i++)
			{
				MatchEvaluator matchEvaluator = new MatchEvaluator(this, option);
				string value = text;
				text = _varReferencePattern.Replace(text, matchEvaluator.Evaluate);
				if (option == MacroResolveOptions.ValidWhenEqual && text.Equals(value, StringComparison.OrdinalIgnoreCase))
				{
					return text;
				}
				if (matchEvaluator.MatchCount == 0)
				{
					return text;
				}
			}
			throw new PkgGenException("Too many recurrence maros");
		}

		public MacroResolver()
		{
			BeginLocal();
		}

		public MacroResolver(MacroResolver parent)
		{
			if (parent == null)
			{
				throw new ArgumentNullException("parent");
			}
			foreach (Dictionary<string, Macro> dictionary in parent._dictionaries)
			{
				_dictionaries.Push(dictionary);
			}
			BeginLocal();
		}

		public void Register(string name, string value, bool allowOverride)
		{
			Register(name, value, (object x) => x.ToString(), allowOverride);
		}

		public void Register(string name, object value, MacroDelegate del, bool allowOverride)
		{
			if (!_varNamePattern.IsMatch(name))
			{
				throw new PkgGenException("Incorrect macro id: '{0}', expecting a string matching regular expression pattern '{1}'", name, _varNamePattern);
			}
			if (!allowOverride)
			{
				string value2 = GetValue(name);
				if (value2 != null)
				{
					throw new PkgGenException("Redefining macro is not allowed, id:'{0}', current value:'{1}', new value:'{2}'", name, value2, value);
				}
			}
			_dictionaries.Peek()[name] = new Macro(name, value, del);
		}

		public bool Unregister(string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				return false;
			}
			return RemoveName(name);
		}

		public void Import(string File)
		{
			if (LongPathFile.Exists(File))
			{
				XmlReader macroDefinitionReader = XmlReader.Create(new FileStream(File, FileMode.Open, FileAccess.Read));
				Load(macroDefinitionReader);
			}
		}

		public void Load(XmlReader macroDefinitionReader)
		{
			try
			{
				MacroTable macroTable = (MacroTable)new XmlSerializer(typeof(MacroTable)).Deserialize(macroDefinitionReader);
				if (macroTable != null)
				{
					Register(macroTable.Values);
				}
			}
			catch (InvalidOperationException ex)
			{
				if (ex.InnerException != null)
				{
					throw ex.InnerException;
				}
				throw;
			}
		}
	}
}
