using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	internal static class SddlNormalizer
	{
		private static readonly HashSet<string> s_knownSids = new HashSet<string>
		{
			"AN", "AO", "AU", "BA", "BG", "BO", "BU", "CA", "CD", "CG",
			"CO", "CY", "EA", "ED", "ER", "IS", "IU", "LA", "LG", "LS",
			"LU", "MU", "NO", "NS", "NU", "OW", "PA", "PO", "PS", "PU",
			"RC", "RD", "RE", "RO", "RS", "RU", "SA", "SO", "SU", "SY",
			"WD", "WR"
		};

		private static Dictionary<string, string> s_map = new Dictionary<string, string>();

		private static string ToFullSddl(string sid)
		{
			if (string.IsNullOrEmpty(sid) || sid.StartsWith("S-", StringComparison.Ordinal) || s_knownSids.Contains(sid))
			{
				return sid;
			}
			string value = null;
			if (!s_map.TryGetValue(sid, out value))
			{
				value = new SecurityIdentifier(sid).ToString();
				s_map.Add(sid, value);
			}
			return value;
		}

		private static string FormatFullAccountSid(string matchGroupIndex, Match match)
		{
			string value = match.Value;
			string value2 = match.Groups[matchGroupIndex].Value;
			char c = value[value.Length - 1];
			return value.Remove(value.Length - (value2.Length + 1)) + ToFullSddl(value2) + c;
		}

		public static string FixAceSddl(string sddl)
		{
			if (string.IsNullOrEmpty(sddl))
			{
				return sddl;
			}
			return Regex.Replace(sddl, "((;[^;]*){4};)(?<sid>[^;\\)]+)([;\\)])", (Match x) => FormatFullAccountSid("sid", x));
		}

		public static string FixOwnerSddl(string sddl)
		{
			if (string.IsNullOrEmpty(sddl))
			{
				return sddl;
			}
			return Regex.Replace(sddl, "O:(?<oid>.*?)G:(?<gid>.*)", (Match x) => string.Format("O:{0}G:{1}", ToFullSddl(x.Groups["oid"].Value), ToFullSddl(x.Groups["gid"].Value)));
		}
	}
}
