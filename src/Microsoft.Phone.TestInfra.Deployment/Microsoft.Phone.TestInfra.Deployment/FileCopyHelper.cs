using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary;
using Microsoft.Tools.IO;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class FileCopyHelper
	{
		public static bool FilesMatch(string file1, string file2)
		{
			if (string.IsNullOrEmpty(file1))
			{
				throw new ArgumentNullException("file1");
			}
			if (!LongPathFile.Exists(file1))
			{
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "File file does not exist: {0}", new object[1] { file1 }));
			}
			if (string.IsNullOrEmpty(file2))
			{
				throw new ArgumentNullException("file2");
			}
			return LongPathFile.Exists(file2) && ((LongPathFile.GetFileLengthBytes(file1) == LongPathFile.GetFileLengthBytes(file2) && LongPathFile.GetLastWriteTimeUtc(file1) == LongPathFile.GetLastWriteTimeUtc(file2)) || LongPathFile.GetAttributes(file2).HasFlag(FileAttributes.ReparsePoint));
		}

		public static bool CopyFile(string sourceFile, string targetFile, int retryCount, TimeSpan retryDelay)
		{
			if (string.IsNullOrEmpty(sourceFile))
			{
				throw new ArgumentNullException("sourceFile");
			}
			if (string.IsNullOrEmpty(targetFile))
			{
				throw new ArgumentNullException("targetFile");
			}
			if (retryCount < 0)
			{
				throw new ArgumentOutOfRangeException("retryCount", retryCount, "Retry count is negative");
			}
			if (retryDelay < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("retryDelay", retryDelay, "Retry delay is negative");
			}
			return InternalLongCopyFile(sourceFile, targetFile, retryCount, retryDelay);
		}

		public static IEnumerable<string> CopyFiles(string source, string destination, string pattern, bool recursive, int retryCount, TimeSpan retryDelay)
		{
			IEnumerable<string> skippedFiles;
			return CopyFiles(source, destination, pattern, recursive, retryCount, retryDelay, out skippedFiles);
		}

		public static IEnumerable<string> CopyFiles(string source, string destination, string pattern, bool recursive, int retryCount, TimeSpan retryDelay, out IEnumerable<string> skippedFiles)
		{
			if (string.IsNullOrEmpty(source))
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrEmpty(destination))
			{
				throw new ArgumentNullException("destination");
			}
			if (string.IsNullOrEmpty(pattern))
			{
				throw new ArgumentNullException("pattern");
			}
			if (retryCount < 0)
			{
				throw new ArgumentOutOfRangeException("retryCount", retryCount, "Retry count is negative");
			}
			if (retryDelay < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("retryDelay", retryDelay, "Retry delay is negative");
			}
			LongPathDirectory.Create(destination);
			object syncRoot = new object();
			List<string> affected = new List<string>();
			List<string> skipped = new List<string>();
			IEnumerable<string> source2 = RetryHelper.Retry(() => LongPathDirectory.EnumerateFiles(source, pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly), retryCount, retryDelay);
			Parallel.ForEach(source2, delegate(string sourceFile)
			{
				string text = PathHelper.ChangeParent(sourceFile, source, destination);
				bool flag = CopyFile(sourceFile, text, retryCount, retryDelay);
				lock (syncRoot)
				{
					if (!flag)
					{
						skipped.Add(text);
					}
					affected.Add(text);
				}
			});
			skippedFiles = skipped;
			return affected;
		}

		internal static bool InternalLongCopyFile(string sourceFile, string targetFile, int retryCount, TimeSpan retryDelay)
		{
			if (FilesMatch(sourceFile, targetFile))
			{
				return false;
			}
			LongPathDirectory.Create(LongPathPath.GetDirectoryName(targetFile));
			RetryHelper.Retry(delegate
			{
				LongPathFile.Copy(sourceFile, targetFile, true);
			}, retryCount, retryDelay, new Type[2]
			{
				typeof(UnauthorizedAccessException),
				typeof(IOException)
			});
			LongPathFile.SetAttributes(targetFile, FileAttributes.Normal);
			LongPathFile.SetCreationTimeUtc(targetFile, LongPathFile.GetCreationTimeUtc(sourceFile));
			LongPathFile.SetLastWriteTimeUtc(targetFile, LongPathFile.GetLastWriteTimeUtc(sourceFile));
			return true;
		}
	}
}
