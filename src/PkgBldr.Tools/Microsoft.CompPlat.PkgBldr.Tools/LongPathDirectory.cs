using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class LongPathDirectory
	{
		public const string ALL_FILE_PATTERN = "*.*";

		public static void CreateDirectory(string path)
		{
			try
			{
				NativeMethods.IU_EnsureDirectoryExists(path);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public static void Delete(string path)
		{
			string text = LongPathCommon.NormalizeLongPath(path);
			if (!Exists(text) || NativeMethods.RemoveDirectory(text))
			{
				return;
			}
			throw LongPathCommon.GetExceptionFromLastWin32Error();
		}

		public static void Delete(string path, bool recursive)
		{
			if (recursive)
			{
				NativeMethods.IU_CleanDirectory(path, true);
			}
			else
			{
				Delete(path);
			}
		}

		public static bool Exists(string path)
		{
			return NativeMethods.IU_DirectoryExists(path);
		}

		public static FileAttributes GetAttributes(string path)
		{
			FileAttributes attributes = LongPathCommon.GetAttributes(path);
			if (!attributes.HasFlag(FileAttributes.Directory))
			{
				throw LongPathCommon.GetExceptionFromWin32Error(267);
			}
			return attributes;
		}

		public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern, SearchOption searchOptions)
		{
			return GetDirectories(path, searchPattern, searchOptions);
		}

		public static IEnumerable<string> EnumerateDirectories(string path, string searchPattern)
		{
			return EnumerateDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateDirectories(string path)
		{
			return EnumerateDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static string[] GetDirectories(string path, string searchPattern, SearchOption searchOptions)
		{
			if (searchOptions != SearchOption.AllDirectories && searchOptions != 0)
			{
				throw new NotImplementedException("Unknown search option: " + searchOptions);
			}
			bool fRecursive = searchOptions == SearchOption.AllDirectories;
			IntPtr rgDirectories = IntPtr.Zero;
			int cDirectories = 0;
			string strFolder = LongPath.Combine(path, LongPath.GetDirectoryName(searchPattern));
			string fileName = Path.GetFileName(searchPattern);
			int num = NativeMethods.IU_GetAllDirectories(strFolder, fileName, fRecursive, out rgDirectories, out cDirectories);
			if (num != 0)
			{
				throw LongPathCommon.GetExceptionFromWin32Error(num);
			}
			try
			{
				return LongPathCommon.ConvertPtrArrayToStringArray(rgDirectories, cDirectories);
			}
			finally
			{
				NativeMethods.IU_FreeStringList(rgDirectories, cDirectories);
			}
		}

		public static string[] GetDirectories(string path, string searchPattern)
		{
			return GetDirectories(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static string[] GetDirectories(string path)
		{
			return GetDirectories(path, "*.*", SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern, SearchOption searchOptions)
		{
			return GetFiles(path, searchPattern, searchOptions);
		}

		public static IEnumerable<string> EnumerateFiles(string path, string searchPattern)
		{
			return EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static IEnumerable<string> EnumerateFiles(string path)
		{
			return EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly);
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static string[] GetFiles(string path, string searchPattern, SearchOption searchOptions)
		{
			if (searchOptions != SearchOption.AllDirectories && searchOptions != 0)
			{
				throw new NotImplementedException("Unknown search option: " + searchOptions);
			}
			bool fRecursive = searchOptions == SearchOption.AllDirectories;
			IntPtr rgFiles = IntPtr.Zero;
			int cFiles = 0;
			string strFolder = LongPath.Combine(path, Path.GetDirectoryName(searchPattern));
			string fileName = Path.GetFileName(searchPattern);
			int num = NativeMethods.IU_GetAllFiles(strFolder, fileName, fRecursive, out rgFiles, out cFiles);
			if (num != 0)
			{
				throw LongPathCommon.GetExceptionFromWin32Error(num);
			}
			try
			{
				return LongPathCommon.ConvertPtrArrayToStringArray(rgFiles, cFiles);
			}
			finally
			{
				NativeMethods.IU_FreeStringList(rgFiles, cFiles);
			}
		}

		public static string[] GetFiles(string path, string searchPattern)
		{
			return GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
		}

		public static string[] GetFiles(string path)
		{
			return GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
		}
	}
}
