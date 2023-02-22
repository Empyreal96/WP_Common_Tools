using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class LongPath
	{
		private const string LONGPATH_PREFIX = "\\\\?\\";

		public static string GetFullPath(string path)
		{
			return LongPathCommon.NormalizeLongPath(path).Substring("\\\\?\\".Length);
		}

		public static string GetPathRoot(string path)
		{
			if (path == null)
			{
				return null;
			}
			if (path == string.Empty || path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
			{
				throw new ArgumentException("Path parameter was empty or otherwise invalid.");
			}
			if (!Path.IsPathRooted(path))
			{
				return string.Empty;
			}
			if (path.StartsWith("\\\\", StringComparison.OrdinalIgnoreCase))
			{
				int num = path.IndexOf(Path.DirectorySeparatorChar, "\\\\".Length);
				if (num == -1)
				{
					return path;
				}
				int num2 = path.IndexOf(Path.DirectorySeparatorChar, num + 1);
				if (num2 == -1)
				{
					return path;
				}
				return path.Substring(0, num2);
			}
			if (path.IndexOf(Path.VolumeSeparatorChar) != 1)
			{
				return string.Empty;
			}
			if (path.Length <= 2 || path[2] != Path.DirectorySeparatorChar)
			{
				return path.Substring(0, 2);
			}
			return path.Substring(0, 3);
		}

		public static string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		public static string GetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}

		public static string RemoveExtension(string path)
		{
			return Regex.Replace(path, "\\.[^\\.]+$", "");
		}

		public static string GetExtension(string path)
		{
			return Regex.Match(path.ToLowerInvariant(), "\\.[^\\.]+$").Value.TrimStart('.');
		}

		public static string Combine(string path, string file)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", new object[2]
			{
				path.TrimEnd('\\'),
				file
			});
		}
	}
}
