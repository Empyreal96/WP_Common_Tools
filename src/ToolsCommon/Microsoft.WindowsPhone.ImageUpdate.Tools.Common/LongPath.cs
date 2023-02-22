using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public static class LongPath
	{
		private const string UNC_PREFIX = "\\\\";

		private const string LONGPATH_PREFIX = "\\\\?\\";

		private const string LONGPATH_UNC_PREFIX = "\\\\?\\UNC\\";

		public static string GetDirectoryName(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path", "Path cannot be null.");
			}
			if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
			{
				throw new ArgumentException("Path cannot contain invalid characters.", "path");
			}
			int num = path.LastIndexOfAny(new char[2]
			{
				Path.DirectorySeparatorChar,
				Path.VolumeSeparatorChar
			});
			if (num == -1)
			{
				return null;
			}
			return path.Substring(0, num);
		}

		public static string GetFullPath(string path)
		{
			string text = LongPathCommon.NormalizeLongPath(path);
			if (text.StartsWith("\\\\?\\UNC\\", StringComparison.OrdinalIgnoreCase))
			{
				return "\\\\" + text.Substring("\\\\?\\UNC\\".Length);
			}
			if (text.StartsWith("\\\\?\\", StringComparison.OrdinalIgnoreCase))
			{
				return text.Substring("\\\\?\\".Length);
			}
			return text;
		}

		public static string GetFullPathUNC(string path)
		{
			return LongPathCommon.NormalizeLongPath(path);
		}

		public static string GetPathRoot(string path)
		{
			if (path == null)
			{
				return null;
			}
			if (path == string.Empty || path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
			{
				throw new ArgumentException("Path cannot be empty or contain invalid characters.", "path");
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

		public static string Combine(string path, string file)
		{
			return $"{path.TrimEnd('\\')}\\{file.Trim('\\')}";
		}

		public static string GetFileName(string path)
		{
			return Regex.Match(path, "\\\\[^\\\\]+$").Value.TrimStart('\\');
		}

		public static string GetExtension(string path)
		{
			if (path == null)
			{
				return null;
			}
			string text = Regex.Match(path.ToLowerInvariant(), "\\.[^\\.]+$").Value.TrimStart('.');
			if (string.IsNullOrEmpty(text))
			{
				return string.Empty;
			}
			return "." + text;
		}
	}
}
