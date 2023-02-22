using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class FileUtils
	{
		public const int MAX_PATH = 260;

		public static string RerootPath(string path, string oldRoot, string newRoot)
		{
			if (oldRoot.Last() != '\\')
			{
				oldRoot += "\\";
			}
			if (newRoot.Last() != '\\')
			{
				newRoot += "\\";
			}
			return path.Replace(oldRoot, newRoot);
		}

		public static string GetTempFile()
		{
			return GetTempFile(Path.GetTempPath());
		}

		public static string GetTempFile(string dir)
		{
			return LongPath.Combine(dir, Path.GetRandomFileName());
		}

		public static void DeleteTree(string dirPath)
		{
			if (string.IsNullOrEmpty(dirPath))
			{
				throw new ArgumentException("Empty directory path");
			}
			if (LongPathFile.Exists(dirPath))
			{
				throw new IOException(string.Format(CultureInfo.InvariantCulture, "Cannot delete directory {0}, it's a file", new object[1] { dirPath }));
			}
			if (LongPathDirectory.Exists(dirPath))
			{
				LongPathDirectory.Delete(dirPath, true);
			}
		}

		public static void DeleteFile(string filePath)
		{
			if (LongPathFile.Exists(filePath))
			{
				LongPathFile.SetAttributes(filePath, FileAttributes.Normal);
				LongPathFile.Delete(filePath);
			}
		}

		public static void CleanDirectory(string dirPath)
		{
			if (string.IsNullOrEmpty(dirPath))
			{
				throw new ArgumentException("Empty directory path");
			}
			if (LongPathFile.Exists(dirPath))
			{
				throw new IOException(string.Format(CultureInfo.InvariantCulture, "Cannot create directory {0}, a file with same name exists", new object[1] { dirPath }));
			}
			NativeMethods.IU_CleanDirectory(dirPath, false);
		}

		public static string GetTempDirectory()
		{
			string text;
			do
			{
				text = LongPath.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			}
			while (LongPathDirectory.Exists(text));
			LongPathDirectory.CreateDirectory(text);
			return text;
		}

		public static string GetFileVersion(string filepath)
		{
			string result = string.Empty;
			if (LongPathFile.Exists(filepath))
			{
				result = FileVersionInfo.GetVersionInfo(filepath).FileVersion;
			}
			return result;
		}

		public static string GetCurrentAssemblyFileVersion()
		{
			return GetFileVersion(Process.GetCurrentProcess().MainModule.FileName);
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern uint GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string lpszLongPath, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszShortPath, uint cchBuffer);

		public static string GetShortPathName(string dirPath)
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			if (GetShortPathName(dirPath, stringBuilder, 260u) == 0 || stringBuilder.Length == 0)
			{
				return dirPath;
			}
			return stringBuilder.ToString();
		}

		public static void CopyDirectory(string source, string destination)
		{
			LongPathDirectory.CreateDirectory(destination);
			string[] files = LongPathDirectory.GetFiles(source);
			foreach (string text in files)
			{
				LongPathFile.Copy(text, LongPath.Combine(destination, LongPath.GetFileName(text)));
			}
			files = LongPathDirectory.GetDirectories(source);
			foreach (string text2 in files)
			{
				CopyDirectory(text2, LongPath.Combine(destination, LongPath.GetFileName(text2)));
			}
		}
	}
}
