using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.Multivariant.Offline
{
	public class MVCondition : IEquatable<MVCondition>
	{
		public Dictionary<string, WPConstraintValue> KeyValues { get; private set; }

		public static IEnumerable<string> ValidKeys => BuiltInKeys;

		public static IEnumerable<string> BuiltInKeys => new List<string> { "mcc", "mnc", "spn", "uiname", "uiorder", "gid1" };

		public static IEnumerable<string> WildCardKeys => new List<string> { "mcc", "mnc", "carrierid" };

		private static Dictionary<string, Func<WPConstraintValue, bool>> keyTypes
		{
			get
			{
				uint num;
				ulong longnum;
				return new Dictionary<string, Func<WPConstraintValue, bool>>(StringComparer.OrdinalIgnoreCase)
				{
					{
						"mcc",
						(WPConstraintValue value) => (!value.IsWildCard && value.KeyValue != null && uint.TryParse(value.KeyValue, out num) && value.KeyValue.Length == 3) || IsWildCardCondition(value)
					},
					{
						"mnc",
						(WPConstraintValue value) => (!value.IsWildCard && value.KeyValue != null && uint.TryParse(value.KeyValue, out num) && 2 <= value.KeyValue.Length && value.KeyValue.Length <= 3) || IsWildCardCondition(value)
					},
					{
						"spn",
						(WPConstraintValue value) => value.KeyValue != null && value.KeyValue.Length <= 16
					},
					{
						"uiname",
						(WPConstraintValue value) => value.KeyValue != null
					},
					{
						"uiorder",
						(WPConstraintValue value) => value.KeyValue != null
					},
					{
						"gid1",
						(WPConstraintValue value) => value.KeyValue != null && ulong.TryParse(value.KeyValue, NumberStyles.HexNumber, null, out longnum)
					}
				};
			}
		}

		private static Dictionary<string, string> errors => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "mcc", "MCC values must be a three digit numerical value or WildCard with null value" },
			{ "mnc", "MNC values must be a two or three digit numerical value or WildCard with null value" },
			{ "spn", "SPN values must be under 16 characters" },
			{ "uiname", "UIName values must be defined" },
			{ "uiorder", "UIOrder values must be defined" },
			{ "gid1", "GID1 values must be a string of hexadecimal digits" }
		};

		public static IEnumerable<string> InvalidKeys => new List<string> { "name", "type", "data", "target", "productid", "settingsgroup" };

		private static bool IsWildCardCondition(WPConstraintValue value)
		{
			if (value.IsWildCard)
			{
				if (value.KeyValue != null)
				{
					return value.KeyValue == "";
				}
				return true;
			}
			return false;
		}

		public static bool IsValidValue(string keyName, WPConstraintValue value, out string errorMessage)
		{
			if (!ValidKeys.Contains(keyName, StringComparer.OrdinalIgnoreCase))
			{
				errorMessage = string.Empty;
				return true;
			}
			if (value.IsWildCard)
			{
				errorMessage = string.Empty;
				return true;
			}
			Match match = Regex.Match(value.KeyValue, "^(?<not>!?)(?<comparison>(pattern:|range:)?)(?<value>.*)$");
			if (!match.Success)
			{
				errorMessage = "Target values should be in the form \"[!][pattern:|range:]<match value>";
				return false;
			}
			Group group = match.Groups["comparison"];
			string text = (match.Groups["value"].Success ? match.Groups["value"].Value : "");
			if (group.Success)
			{
				string value2 = group.Value;
				if (value2 == "pattern:")
				{
					errorMessage = $"The provided pattern string \"{text}\" is not a valid Regular Expression.";
					try
					{
						Regex.Match("", text);
					}
					catch (ArgumentException)
					{
						return false;
					}
					return true;
				}
				if (value2 == "range:")
				{
					errorMessage = $"The provided range string \"{text}\" is not in the form \"<minimum>, <maximum>\"";
					return Regex.IsMatch(text, "^(-?\\d+),\\s?(-?\\d+)$");
				}
			}
			WPConstraintValue arg = new WPConstraintValue(text, value.IsWildCard);
			errorMessage = errors[keyName];
			return keyTypes[keyName](arg);
		}

		public static bool IsWildCardKey(string keyname)
		{
			return WildCardKeys.Contains(keyname, StringComparer.OrdinalIgnoreCase);
		}

		public static bool IsValidKey(string keyName)
		{
			return !InvalidKeys.Contains(keyName, StringComparer.OrdinalIgnoreCase);
		}

		public MVCondition()
		{
			KeyValues = new Dictionary<string, WPConstraintValue>();
		}

		public bool IsValidCondition()
		{
			if (KeyValues.Any((KeyValuePair<string, WPConstraintValue> x) => !IsValidKey(x.Key)))
			{
				return false;
			}
			return true;
		}

		public bool Equals(MVCondition other)
		{
			if (other == null)
			{
				return false;
			}
			if (KeyValues.Count != other.KeyValues.Count)
			{
				return false;
			}
			foreach (KeyValuePair<string, WPConstraintValue> keyValue in KeyValues)
			{
				WPConstraintValue value;
				if (!other.KeyValues.TryGetValue(keyValue.Key, out value))
				{
					return false;
				}
				if (value.IsWildCard != keyValue.Value.IsWildCard)
				{
					return false;
				}
				if (!string.Equals(value.KeyValue, keyValue.Value.KeyValue, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			return Equals(obj as MVCondition);
		}

		public override int GetHashCode()
		{
			int num = 0;
			foreach (KeyValuePair<string, WPConstraintValue> keyValue in KeyValues)
			{
				num ^= keyValue.Key.GetHashCode();
				if (keyValue.Value != null)
				{
					num ^= keyValue.Value.GetHashCode();
				}
			}
			return num;
		}
	}
}
