using System.Text.RegularExpressions;

namespace Microsoft.CompPlat.PkgBldr.Plugins.CsiToPkg
{
	internal static class SddlHelpers
	{
		public static string GetSddlOwner(string SDDL)
		{
			string result = null;
			Match match = Regex.Match(SDDL + ":", "[O]:([A-Za-z0-9\\-]+)[:]");
			if (match.Success)
			{
				result = match.Value;
				result = result.Substring(2, result.Length - 2 - 2);
			}
			return result;
		}

		public static string GetSddlGroup(string SDDL)
		{
			string result = null;
			Match match = Regex.Match(SDDL + ":", "[G]:([A-Za-z0-9\\-]+)[:]");
			if (match.Success)
			{
				result = match.Value;
				result = result.Substring(2, result.Length - 2 - 2);
			}
			return result;
		}

		public static string GetSddlDacl(string SDDL)
		{
			string result = null;
			Match match = Regex.Match(SDDL + ":", "[D]:([A-Za-z0-9\\(\\);\\-]+)[:]");
			if (match.Success)
			{
				result = match.Value;
				result = result.Substring(2, result.Length - 2 - 2);
			}
			return result;
		}

		public static string GetSddlSacl(string SDDL)
		{
			string result = null;
			Match match = Regex.Match(SDDL + ":", "[S]:([A-Za-z0-9\\(\\);\\-]+)[:]");
			if (match.Success)
			{
				result = match.Value;
				result = result.Substring(2, result.Length - 2 - 1);
			}
			return result;
		}
	}
}
