using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	internal static class SddlNormalizer
	{
		private static readonly HashSet<string> s_knownSids = new HashSet<string>
		{
			"AN", "AO", "AU", "BA", "BG", "BO", "BU", "CA", "CD", "CG",
			"CO", "CY", "DA", "DC", "DD", "DG", "DU", "EA", "ED", "ER",
			"IS", "IU", "LA", "LG", "LS", "LU", "MU", "NO", "NS", "NU",
			"OW", "PA", "PO", "PS", "PU", "RC", "RD", "RE", "RO", "RS",
			"RU", "SA", "SO", "SU", "SY", "WD", "WR"
		};

		private static Dictionary<string, string> s_map = new Dictionary<string, string>();

		private static string ToFullSddl(string sid)
		{
			if (string.IsNullOrEmpty(sid) || sid.StartsWith("S-", StringComparison.OrdinalIgnoreCase) || s_knownSids.Contains(sid))
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

		public static string FixAceSddl(string sddl)
		{
			if (string.IsNullOrEmpty(sddl))
			{
				return sddl;
			}
			return Regex.Replace(sddl, ";(?<sid>[^;]*?)\\)", (Match x) => string.Format(CultureInfo.InvariantCulture, ";{0})", new object[1] { ToFullSddl(x.Groups["sid"].Value) }));
		}

		public static string FixOwnerSddl(string sddl)
		{
			if (string.IsNullOrEmpty(sddl))
			{
				return sddl;
			}
			return Regex.Replace(sddl, "O:(?<oid>.*?)G:(?<gid>.*?)", (Match x) => string.Format(CultureInfo.InvariantCulture, "O:{0}G:{1}", new object[2]
			{
				ToFullSddl(x.Groups["oid"].Value),
				ToFullSddl(x.Groups["gid"].Value)
			}));
		}
	}
}
