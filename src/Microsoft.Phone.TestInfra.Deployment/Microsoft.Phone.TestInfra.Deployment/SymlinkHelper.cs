using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Tools.IO;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class SymlinkHelper
	{
		public static void CreateSymlink(string source, string target, bool overwrite)
		{
			if (string.IsNullOrEmpty(source))
			{
				throw new ArgumentNullException("source");
			}
			if (string.IsNullOrEmpty(target))
			{
				throw new ArgumentNullException("target");
			}
			if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Source file is equal to target: {0}", new object[1] { source }));
			}
			if (!LongPathFile.Exists(source))
			{
				throw new FileNotFoundException("Source file does not exist", source);
			}
			bool flag = LongPathFile.Exists(target);
			if (flag && overwrite)
			{
				LongPathFile.Delete(target);
				flag = false;
			}
			if (!flag)
			{
				LongPathDirectory.Create(LongPathPath.GetDirectoryName(target));
				if (!NativeMethods.CreateSymbolicLink(target, source, NativeMethods.SymbolicLinkFlag.File))
				{
					Exception exceptionForHR = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
					throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unable to create symbolic link: {0} -> {1}", new object[2] { target, source }), exceptionForHR);
				}
				SetSymlinkTimestamps(target, source);
			}
		}

		public static void CreateSymlinks(string sourceDirectory, string targetDirectory, bool overwrite)
		{
			CreateSymlinks(sourceDirectory, targetDirectory, overwrite, null);
		}

		public static void CreateSymlinks(string sourceDirectory, string targetDirectory, bool overwrite, IEnumerable<string> filesToSkip)
		{
			if (string.IsNullOrEmpty(sourceDirectory))
			{
				throw new ArgumentNullException("sourceDirectory");
			}
			if (string.IsNullOrEmpty(targetDirectory))
			{
				throw new ArgumentNullException("targetDirectory");
			}
			foreach (string item in LongPathDirectory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
			{
				if (filesToSkip == null || !filesToSkip.Contains(LongPathPath.GetFileName(item), StringComparer.OrdinalIgnoreCase))
				{
					string target = PathHelper.ChangeParent(item, sourceDirectory, targetDirectory);
					CreateSymlink(item, target, overwrite);
				}
			}
		}

		private static void SetSymlinkTimestamps(string link, string source)
		{
			using (SafeFileHandle hFile = NativeMethods.CreateFile(link, 256u, 3u, IntPtr.Zero, 3u, 2097152u, IntPtr.Zero))
			{
				long lpCreationTime = LongPathFile.GetCreationTimeUtc(source).ToFileTimeUtc();
				long lpLastAccessTime = LongPathFile.GetLastAccessTimeUtc(source).ToFileTimeUtc();
				long lpLastWriteTime = LongPathFile.GetLastWriteTimeUtc(source).ToFileTimeUtc();
				if (!NativeMethods.SetFileTime(hFile, ref lpCreationTime, ref lpLastAccessTime, ref lpLastWriteTime))
				{
					Exception exceptionForHR = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
					throw new InvalidOperationException("Unable to update symbolic link timestamps", exceptionForHR);
				}
			}
		}
	}
}
