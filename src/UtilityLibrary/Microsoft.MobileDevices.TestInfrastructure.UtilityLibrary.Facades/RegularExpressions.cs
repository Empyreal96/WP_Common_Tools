using System.Text.RegularExpressions;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.Facades
{
	public class RegularExpressions
	{
		public static bool MatchRegEx(string stringToTest, string expression)
		{
			Regex expression2 = new Regex(expression);
			return MatchRegEx(stringToTest, expression2);
		}

		public static bool MatchRegEx(string stringToTest, Regex expression)
		{
			Match match = expression.Match(stringToTest);
			return match.Success;
		}

		public static int FindFirstMatchGroupInString(string regularExpression, string stringToMatch, out string firstMatch)
		{
			Regex regex = new Regex(regularExpression, RegexOptions.IgnoreCase);
			return FindFirstMatchGroupInString(regex, stringToMatch, out firstMatch);
		}

		public static int FindFirstMatchGroupInMultiLineString(string regularExpression, string stringToMatch, out string firstMatch)
		{
			Regex regex = new Regex(regularExpression, RegexOptions.IgnoreCase | RegexOptions.Multiline);
			return FindFirstMatchGroupInString(regex, stringToMatch, out firstMatch);
		}

		public static int FindFirstMatchGroupInString(Regex regex, string stringToMatch, out string firstMatch)
		{
			int result = 0;
			Match match = regex.Match(stringToMatch);
			if (match.Success)
			{
				firstMatch = match.Groups[1].ToString();
				result = match.Groups.Count;
			}
			else
			{
				firstMatch = string.Empty;
			}
			return result;
		}
	}
}
