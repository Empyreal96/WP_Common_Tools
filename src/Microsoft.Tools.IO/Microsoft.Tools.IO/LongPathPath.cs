using System;
using System.IO;

namespace Microsoft.Tools.IO
{
	public static class LongPathPath
	{
		public static readonly char DirectorySeparatorChar = Path.DirectorySeparatorChar;

		public static readonly char AltDirectorySeparatorChar = Path.AltDirectorySeparatorChar;

		public static readonly char VolumeSeparatorChar = Path.VolumeSeparatorChar;

		[Obsolete("Please use GetInvalidPathChars or GetInvalidFileNameChars instead.")]
		public static readonly char[] InvalidPathChars = Path.InvalidPathChars;

		public static readonly char PathSeparator = Path.PathSeparator;

		internal static readonly char[] TrimEndChars = new char[8] { '\t', '\n', '\v', '\f', '\r', ' ', '\u0085', '\u00a0' };

		public static string GetFullPath(string path)
		{
			path = NormalizePath(path, true);
			path = LongPathCommon.NormalizeLongPath(path);
			return LongPathCommon.RemoveLongPathPrefix(path);
		}

		public static string GetPathRoot(string path)
		{
			if (path == null)
			{
				return null;
			}
			path = NormalizePath(path, false);
			return path?.Substring(0, GetRootLength(path));
		}

		public static string GetDirectoryName(string path)
		{
			if (path != null)
			{
				CheckInvalidPathChars(path);
				path = NormalizePath(path, false);
				int rootLength = GetRootLength(path);
				int length = path.Length;
				if (length > rootLength)
				{
					length = path.Length;
					if (length == rootLength)
					{
						return null;
					}
					while (length > rootLength && path[--length] != DirectorySeparatorChar && path[length] != AltDirectorySeparatorChar)
					{
					}
					string text = path.Substring(0, length);
					if (length > rootLength)
					{
						return text.TrimEnd(DirectorySeparatorChar, AltDirectorySeparatorChar);
					}
					return text;
				}
			}
			return null;
		}

		public static string Combine(string path1, string path2)
		{
			return Path.Combine(path1, path2);
		}

		public static string Combine(string path1, string path2, string path3)
		{
			return Path.Combine(path1, path2, path3);
		}

		public static string Combine(string path1, string path2, string path3, string path4)
		{
			return Path.Combine(path1, path2, path3, path4);
		}

		public static string Combine(params string[] paths)
		{
			return Path.Combine(paths);
		}

		public static string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		public static string GetFileNameWithoutExtension(string path)
		{
			return Path.GetFileNameWithoutExtension(path);
		}

		public static bool IsPathRooted(string path)
		{
			return Path.IsPathRooted(path);
		}

		public static string GetRandomFileName()
		{
			return Path.GetRandomFileName();
		}

		public static string ChangeExtension(string path, string extension)
		{
			return Path.ChangeExtension(path, extension);
		}

		public static string GetExtension(string path)
		{
			return Path.GetExtension(path);
		}

		public static char[] GetInvalidPathChars()
		{
			return Path.GetInvalidPathChars();
		}

		public static char[] GetInvalidFileNameChars()
		{
			return Path.GetInvalidFileNameChars();
		}

		public static string GetTempPath()
		{
			return Path.GetTempPath();
		}

		public static string GetTempFileName()
		{
			return Path.GetTempFileName();
		}

		public static bool HasExtension(string path)
		{
			return Path.HasExtension(path);
		}

		internal static void CheckInvalidPathChars(string path)
		{
			foreach (int num in path)
			{
				if (num == 34 || num == 60 || num == 62 || num == 124 || num < 32)
				{
					throw new ArgumentException("Illegal characters in path.");
				}
			}
		}

		internal static int GetRootLength(string path)
		{
			CheckInvalidPathChars(path);
			int i = 0;
			int length = path.Length;
			if (length >= 1 && IsDirectorySeparator(path[0]))
			{
				i = 1;
				if (length >= 2 && IsDirectorySeparator(path[1]))
				{
					i = 2;
					int num = 2;
					for (; i < length; i++)
					{
						if ((path[i] == DirectorySeparatorChar || path[i] == AltDirectorySeparatorChar) && --num <= 0)
						{
							break;
						}
					}
				}
			}
			else if (length >= 2 && path[1] == VolumeSeparatorChar)
			{
				i = 2;
				if (length >= 3 && IsDirectorySeparator(path[2]))
				{
					i++;
				}
			}
			return i;
		}

		internal static bool IsDirectorySeparator(char c)
		{
			if (c != DirectorySeparatorChar)
			{
				return c == AltDirectorySeparatorChar;
			}
			return true;
		}

		private static string NormalizePath(string path, bool fullCheck)
		{
			if (fullCheck)
			{
				path = path.TrimEnd(TrimEndChars);
				CheckInvalidPathChars(path);
			}
			string text = path.Substring(0, GetRootLength(path));
			path = path.Remove(0, text.Length);
			path = path.Replace(AltDirectorySeparatorChar, DirectorySeparatorChar);
			string text2 = new string(new char[2] { DirectorySeparatorChar, DirectorySeparatorChar });
			do
			{
				path = path.Replace(text2, DirectorySeparatorChar.ToString());
			}
			while (path.Contains(text2));
			path = path.Insert(0, text);
			return path;
		}
	}
}
