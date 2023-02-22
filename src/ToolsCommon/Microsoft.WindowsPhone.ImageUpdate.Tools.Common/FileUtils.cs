using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
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
			return Path.Combine(dir, Path.GetRandomFileName());
		}

		public static void DeleteTree(string dirPath)
		{
			if (string.IsNullOrEmpty(dirPath))
			{
				throw new ArgumentException("Empty directory path");
			}
			if (LongPathFile.Exists(dirPath))
			{
				throw new IOException($"Cannot delete directory {dirPath}, it's a file");
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
				throw new IOException($"Cannot create directory {dirPath}, a file with same name exists");
			}
			NativeMethods.IU_CleanDirectory(dirPath, false);
		}

		public static string GetTempDirectory()
		{
			string text;
			do
			{
				text = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			}
			while (LongPathDirectory.Exists(text));
			LongPathDirectory.CreateDirectory(text);
			return text;
		}

		public static bool IsTargetUpToDate(string inputFile, string targetFile)
		{
			if (!LongPathFile.Exists(targetFile))
			{
				return false;
			}
			DateTime lastWriteTimeUtc = new FileInfo(targetFile).LastWriteTimeUtc;
			if (new FileInfo(inputFile).LastWriteTimeUtc > lastWriteTimeUtc)
			{
				return false;
			}
			return true;
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
			if (GetShortPathName(dirPath, stringBuilder, 260u) == 0)
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
				LongPathFile.Copy(text, Path.Combine(destination, Path.GetFileName(text)));
			}
			files = LongPathDirectory.GetDirectories(source);
			foreach (string text2 in files)
			{
				CopyDirectory(text2, Path.Combine(destination, Path.GetFileName(text2)));
			}
		}
	}
}
