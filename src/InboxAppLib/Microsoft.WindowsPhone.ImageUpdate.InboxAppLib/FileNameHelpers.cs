using System;
using System.IO;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public static class FileNameHelpers
	{
		public static string RemoveMpapPrefix(this string str)
		{
			int length = "MPAP_".Length;
			if (str.StartsWith("MPAP_"))
			{
				return str.Substring(length);
			}
			return str;
		}

		public static string RemoveSrcExtension(this string str)
		{
			if (".src".Equals(Path.GetExtension(str), StringComparison.OrdinalIgnoreCase))
			{
				return Path.GetFileNameWithoutExtension(str);
			}
			return str;
		}

		public static string CleanFileNameForUpdate(this string str, bool isEarly)
		{
			str = (isEarly ? "early" : "") + "_" + str.CleanFileName();
			return str;
		}

		public static string CleanFileName(this string str)
		{
			str = str.RemoveMpapPrefix().RemoveSrcExtension().Replace(".provxml", "_Infused.provxml");
			return str;
		}
	}
}
