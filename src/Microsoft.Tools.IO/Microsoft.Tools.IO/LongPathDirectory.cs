using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Tools.IO.Interop;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Tools.IO
{
	public static class LongPathDirectory
	{
		private const int FILE_FLAG_BACKUP_SEMANTICS = 33554432;

		public static void Create(string path)
		{
			InternalCreateDirectory(LongPathCommon.NormalizeLongPath(path), path);
		}

		public static void Delete(string path)
		{
			if (!NativeMethods.RemoveDirectory(LongPathCommon.NormalizeLongPath(path)))
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
		}

		public static bool Exists(string path)
		{
			bool isDirectory;
			if (LongPathCommon.Exists(path, out isDirectory))
			{
				return isDirectory;
			}
			return false;
		}

		public static IEnumerable<string> EnumerateDirectories(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return InternalEnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			return InternalEnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			if (searchOption != 0 && searchOption != SearchOption.AllDirectories)
			{
				throw new ArgumentOutOfRangeException("searchOption", "Enum value was out of legal range");
			}
			return InternalEnumerateDirectories(path, searchPattern, searchOption);
		}

		private static IEnumerable<string> InternalEnumerateDirectories(string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateFileSystemNames(path, searchPattern, searchOption, false, true);
		}

		public static IEnumerable<string> EnumerateFiles(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return InternalEnumerateFiles(path, "*", SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			return InternalEnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			if (searchOption != 0 && searchOption != SearchOption.AllDirectories)
			{
				throw new ArgumentOutOfRangeException("searchOption", "Enum value was out of legal range");
			}
			return InternalEnumerateFiles(path, searchPattern, searchOption);
		}

		private static IEnumerable<string> InternalEnumerateFiles(string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateFileSystemNames(path, searchPattern, searchOption, true, false);
		}

		public static IEnumerable<string> EnumerateFileSystemEntries(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			return InternalEnumerateFileSystemEntries(path, "*", SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			return InternalEnumerateFileSystemEntries(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
		{
			if (path == null)
			{
				throw new ArgumentNullException("path");
			}
			if (searchPattern == null)
			{
				throw new ArgumentNullException("searchPattern");
			}
			if (searchOption != 0 && searchOption != SearchOption.AllDirectories)
			{
				throw new ArgumentOutOfRangeException("searchOption", "Enum value was out of legal range");
			}
			return InternalEnumerateFileSystemEntries(path, searchPattern, searchOption);
		}

		private static IEnumerable<string> InternalEnumerateFileSystemEntries(string path, string searchPattern, SearchOption searchOption)
		{
			return EnumerateFileSystemNames(path, searchPattern, searchOption, true, true);
		}

		private static IEnumerable<string> EnumerateFileSystemNames(string path, string searchPattern, SearchOption searchOption, bool includeFiles, bool includeDirs)
		{
			return EnumerateFileSystemEntries(path, searchPattern, includeFiles, includeDirs, searchOption);
		}

		public static DateTime GetCreationTime(string path)
		{
			return LongPathFile.GetCreationTime(path);
		}

		public static void SetCreationTime(string path, DateTime creationTime)
		{
			SetCreationTimeUtc(path, creationTime.ToUniversalTime());
		}

		public static DateTime GetCreationTimeUtc(string path)
		{
			return LongPathFile.GetCreationTimeUtc(path);
		}

		public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		{
			using (SafeFileHandle hFile = OpenHandle(path))
			{
				LongPathCommon.SetFileTimes(hFile, creationTimeUtc.ToFileTimeUtc(), 0L, 0L);
			}
		}

		public static DateTime GetLastAccessTime(string path)
		{
			return LongPathFile.GetLastAccessTime(path);
		}

		public static void SetLastAccessTime(string path, DateTime lastAccessTime)
		{
			SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());
		}

		public static DateTime GetLastAccessTimeUtc(string path)
		{
			return LongPathFile.GetLastAccessTimeUtc(path);
		}

		public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		{
			using (SafeFileHandle hFile = OpenHandle(path))
			{
				LongPathCommon.SetFileTimes(hFile, 0L, lastAccessTimeUtc.ToFileTimeUtc(), 0L);
			}
		}

		public static DateTime GetLastWriteTime(string path)
		{
			return LongPathFile.GetLastWriteTime(path);
		}

		public static void SetLastWriteTime(string path, DateTime lastWriteTime)
		{
			SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());
		}

		public static DateTime GetLastWriteTimeUtc(string path)
		{
			return LongPathFile.GetLastWriteTimeUtc(path);
		}

		public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			using (SafeFileHandle hFile = OpenHandle(path))
			{
				LongPathCommon.SetFileTimes(hFile, 0L, 0L, lastWriteTimeUtc.ToFileTimeUtc());
			}
		}

		public static FileAttributes GetAttributes(string path)
		{
			return LongPathCommon.GetAttributes(path);
		}

		public static void SetAttributes(string path, FileAttributes directoryAttributes)
		{
			LongPathCommon.SetAttributes(path, directoryAttributes);
		}

		private static IEnumerable<string> EnumerateFileSystemEntries(string path, string searchPattern, bool includeFiles, bool includeDirectories, SearchOption searchOption)
		{
			string normalizedSearchPattern = NormalizeSearchPatternForIterator(searchPattern);
			string normalizedPath = LongPathCommon.NormalizeLongPath(path);
			FileAttributes attributes;
			int num = LongPathCommon.TryGetDirectoryAttributes(normalizedPath, out attributes);
			if (num != 0)
			{
				throw LongPathCommon.GetExceptionFromWin32Error(num);
			}
			return EnumerateFileSystemIterator(normalizedPath, normalizedSearchPattern, includeFiles, includeDirectories, searchOption);
		}

		private static string NormalizeSearchPatternForIterator(string searchPattern)
		{
			string text = searchPattern.TrimEnd(LongPathPath.TrimEndChars);
			if (text.Equals("."))
			{
				text = "*";
			}
			CheckSearchPattern(text);
			return text;
		}

		[SecuritySafeCritical]
		internal static void CheckSearchPattern(string searchPattern)
		{
			int num;
			while ((num = searchPattern.IndexOf("..", StringComparison.Ordinal)) != -1)
			{
				if (num + 2 == searchPattern.Length)
				{
					throw new ArgumentException("Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.");
				}
				if (searchPattern[num + 2] == LongPathPath.DirectorySeparatorChar || searchPattern[num + 2] == LongPathPath.AltDirectorySeparatorChar)
				{
					throw new ArgumentException("Search pattern cannot contain '..' to move up directories and can be contained only internally in file/directory names, as in 'a..b'.");
				}
				searchPattern = searchPattern.Substring(num + 2);
			}
		}

		private static IEnumerable<string> EnumerateFileSystemIterator(string normalizedPath, string normalizedSearchPattern, bool includeFiles, bool includeDirectories, SearchOption searchOption)
		{
			if (normalizedSearchPattern.Length == 0)
			{
				yield break;
			}
			Queue<string> directoryQueue = new Queue<string>();
			directoryQueue.Enqueue(normalizedPath);
			while (directoryQueue.Count > 0)
			{
				normalizedPath = directoryQueue.Dequeue();
				string path = LongPathCommon.RemoveLongPathPrefix(normalizedPath);
				if (searchOption == SearchOption.AllDirectories)
				{
					foreach (string item2 in EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
					{
						directoryQueue.Enqueue(LongPathCommon.NormalizeLongPath(item2));
					}
				}
				NativeMethods.WIN32_FIND_DATA findData;
				using (SafeFindHandle handle = BeginFind(Path.Combine(normalizedPath, normalizedSearchPattern), out findData))
				{
					if (handle == null)
					{
						if (searchOption != 0)
						{
							continue;
						}
						break;
					}
					do
					{
						string currentFileName = findData.cFileName;
						if (IsDirectory(findData.dwFileAttributes))
						{
							if (IsCurrentOrParentDirectory(currentFileName))
							{
								continue;
							}
							if (searchOption == SearchOption.AllDirectories)
							{
								string item = Path.Combine(normalizedPath, currentFileName);
								if (!directoryQueue.Contains(item))
								{
									directoryQueue.Enqueue(item);
								}
							}
							if (includeDirectories)
							{
								yield return Path.Combine(path, currentFileName);
							}
						}
						else if (includeFiles)
						{
							yield return Path.Combine(path, currentFileName);
						}
					}
					while (NativeMethods.FindNextFile(handle, out findData));
					int lastWin32Error = Marshal.GetLastWin32Error();
					if (lastWin32Error != 18)
					{
						throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
					}
				}
			}
		}

		private static SafeFindHandle BeginFind(string normalizedPathWithSearchPattern, out NativeMethods.WIN32_FIND_DATA findData)
		{
			SafeFindHandle safeFindHandle = NativeMethods.FindFirstFile(normalizedPathWithSearchPattern, out findData);
			if (safeFindHandle.IsInvalid)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 2)
				{
					throw LongPathCommon.GetExceptionFromWin32Error(lastWin32Error);
				}
				return null;
			}
			return safeFindHandle;
		}

		internal static bool IsDirectory(FileAttributes attributes)
		{
			return (attributes & FileAttributes.Directory) == FileAttributes.Directory;
		}

		internal static void InternalCreateDirectory(string normalizedPath, string path)
		{
			int num = normalizedPath.Length;
			if (num >= 2 && LongPathPath.IsDirectorySeparator(normalizedPath[num - 1]))
			{
				num--;
			}
			int rootLength = LongPathPath.GetRootLength(normalizedPath);
			if (num == 2 && LongPathPath.IsDirectorySeparator(normalizedPath[1]))
			{
				throw new IOException($"The specified directory '{path}' cannot be created.");
			}
			List<string> list = new List<string>();
			bool flag = false;
			if (num > rootLength)
			{
				int num2 = num - 1;
				while (num2 >= rootLength && !flag)
				{
					string text = normalizedPath.Substring(0, num2 + 1);
					if (!InternalExists(text))
					{
						list.Add(text);
					}
					else
					{
						flag = true;
					}
					while (num2 > rootLength && normalizedPath[num2] != Path.DirectorySeparatorChar && normalizedPath[num2] != Path.AltDirectorySeparatorChar)
					{
						num2--;
					}
					num2--;
				}
			}
			int count = list.Count;
			if (list.Count != 0)
			{
				string[] array = new string[list.Count];
				list.CopyTo(array, 0);
				for (int i = 0; i < array.Length; i++)
				{
					array[i] += "\\.";
				}
			}
			bool flag2 = true;
			int num3 = 0;
			while (list.Count > 0)
			{
				string text2 = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
				if (text2.Length >= 32000)
				{
					throw new PathTooLongException("The specified file name or path is too long, or a component of the specified path is too long.");
				}
				flag2 = NativeMethods.CreateDirectory(text2, IntPtr.Zero);
				if (!flag2 && num3 == 0)
				{
					int lastError = Marshal.GetLastWin32Error();
					if (lastError != 183)
					{
						num3 = lastError;
					}
					else if (LongPathFile.InternalExists(text2) || (!InternalExists(text2, out lastError) && lastError == 5))
					{
						num3 = lastError;
					}
				}
			}
			if (count == 0 && !flag)
			{
				if (!InternalExists(InternalGetDirectoryRoot(normalizedPath)))
				{
					throw new DirectoryNotFoundException($"Could not find a part of the path '{InternalGetDirectoryRoot(path)}'");
				}
			}
			else if (!flag2 && num3 != 0)
			{
				throw LongPathCommon.GetExceptionFromWin32Error(num3);
			}
		}

		internal static bool InternalExists(string normalizedPath)
		{
			int lastError;
			return InternalExists(normalizedPath, out lastError);
		}

		internal static bool InternalExists(string normalizedPath, out int lastError)
		{
			NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data = default(NativeMethods.WIN32_FILE_ATTRIBUTE_DATA);
			lastError = LongPathCommon.FillAttributeInfo(normalizedPath, ref data, false, false);
			if (lastError == 0 && data.fileAttributes != -1)
			{
				return (data.fileAttributes & 0x10) != 0;
			}
			return false;
		}

		internal static string InternalGetDirectoryRoot(string path)
		{
			return path?.Substring(0, LongPathPath.GetRootLength(path));
		}

		private static SafeFileHandle OpenHandle(string path)
		{
			string text = LongPathCommon.NormalizeLongPath(path);
			string pathRoot = LongPathPath.GetPathRoot(text);
			if (pathRoot == text && pathRoot[1] == Path.VolumeSeparatorChar)
			{
				throw new ArgumentException("Path must not be a drive.", "path");
			}
			SafeFileHandle safeFileHandle = NativeMethods.SafeCreateFile(text, NativeMethods.EFileAccess.GenericWrite, 6u, IntPtr.Zero, 3u, 33554432u, IntPtr.Zero);
			if (safeFileHandle.IsInvalid)
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
			return safeFileHandle;
		}

		private static bool IsCurrentOrParentDirectory(string directoryName)
		{
			if (!directoryName.Equals(".", StringComparison.OrdinalIgnoreCase))
			{
				return directoryName.Equals("..", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
	}
}
