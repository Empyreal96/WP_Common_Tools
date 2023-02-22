using System;
using System.IO;
using Microsoft.Tools.IO.Interop;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Tools.IO
{
	public static class LongPathFile
	{
		public static bool Exists(string path)
		{
			bool isDirectory;
			if (LongPathCommon.Exists(path, out isDirectory))
			{
				return !isDirectory;
			}
			return false;
		}

		public static void Delete(string path)
		{
			if (!NativeMethods.DeleteFile(LongPathCommon.NormalizeLongPath(path)))
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
		}

		public static void Move(string sourcePath, string destinationPath)
		{
			string lpPathNameFrom = LongPathCommon.NormalizeLongPath(sourcePath, "sourcePath");
			string lpPathNameTo = LongPathCommon.NormalizeLongPath(destinationPath, "destinationPath");
			if (!NativeMethods.MoveFile(lpPathNameFrom, lpPathNameTo))
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
		}

		public static void Copy(string sourcePath, string destinationPath, bool overwrite)
		{
			string src = LongPathCommon.NormalizeLongPath(sourcePath, "sourcePath");
			string dst = LongPathCommon.NormalizeLongPath(destinationPath, "destinationPath");
			if (!NativeMethods.CopyFile(src, dst, !overwrite))
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access)
		{
			return Open(path, mode, access, FileShare.None);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
		{
			return Open(path, mode, access, share, 0, FileOptions.None);
		}

		public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options)
		{
			if (bufferSize == 0)
			{
				bufferSize = 1024;
			}
			return new FileStream(GetFileHandle(LongPathCommon.NormalizeLongPath(path), mode, access, share, options), access, bufferSize, (options & FileOptions.Asynchronous) == FileOptions.Asynchronous);
		}

		public static DateTime GetCreationTime(string path)
		{
			return GetCreationTimeUtc(path).ToLocalTime();
		}

		public static void SetCreationTime(string path, DateTime creationTime)
		{
			SetCreationTimeUtc(path, creationTime.ToUniversalTime());
		}

		public static DateTime GetCreationTimeUtc(string path)
		{
			NativeMethods.WIN32_FILE_ATTRIBUTE_DATA win32FileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
			return DateTime.FromFileTimeUtc((long)(((ulong)win32FileAttributeData.ftCreationTimeHigh << 32) | win32FileAttributeData.ftCreationTimeLow));
		}

		public static void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
		{
			using (SafeFileHandle hFile = OpenHandle(LongPathCommon.NormalizeLongPath(path)))
			{
				LongPathCommon.SetFileTimes(hFile, creationTimeUtc.ToFileTimeUtc(), 0L, 0L);
			}
		}

		public static DateTime GetLastAccessTime(string path)
		{
			return GetLastAccessTimeUtc(path).ToLocalTime();
		}

		public static void SetLastAccessTime(string path, DateTime lastAccessTime)
		{
			SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());
		}

		public static DateTime GetLastAccessTimeUtc(string path)
		{
			NativeMethods.WIN32_FILE_ATTRIBUTE_DATA win32FileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
			return DateTime.FromFileTimeUtc((long)(((ulong)win32FileAttributeData.ftLastAccessTimeHigh << 32) | win32FileAttributeData.ftLastAccessTimeLow));
		}

		public static void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
		{
			using (SafeFileHandle hFile = OpenHandle(LongPathCommon.NormalizeLongPath(path)))
			{
				LongPathCommon.SetFileTimes(hFile, 0L, lastAccessTimeUtc.ToFileTimeUtc(), 0L);
			}
		}

		public static DateTime GetLastWriteTime(string path)
		{
			return GetLastWriteTimeUtc(path).ToLocalTime();
		}

		public static void SetLastWriteTime(string path, DateTime lastWriteTime)
		{
			SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());
		}

		public static DateTime GetLastWriteTimeUtc(string path)
		{
			NativeMethods.WIN32_FILE_ATTRIBUTE_DATA win32FileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
			return DateTime.FromFileTimeUtc((long)(((ulong)win32FileAttributeData.ftLastWriteTimeHigh << 32) | win32FileAttributeData.ftLastWriteTimeLow));
		}

		public static void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
		{
			using (SafeFileHandle hFile = OpenHandle(LongPathCommon.NormalizeLongPath(path)))
			{
				LongPathCommon.SetFileTimes(hFile, 0L, 0L, lastWriteTimeUtc.ToFileTimeUtc());
			}
		}

		public static FileAttributes GetAttributes(string path)
		{
			return LongPathCommon.GetAttributes(path);
		}

		public static void SetAttributes(string path, FileAttributes fileAttributes)
		{
			LongPathCommon.SetAttributes(path, fileAttributes);
		}

		public static long GetFileLengthBytes(string path)
		{
			NativeMethods.WIN32_FILE_ATTRIBUTE_DATA win32FileAttributeData = LongPathCommon.GetWin32FileAttributeData(LongPathCommon.NormalizeLongPath(path));
			if (((uint)win32FileAttributeData.fileAttributes & 0x10u) != 0)
			{
				throw new FileNotFoundException($"Could not find file '{path}'", path);
			}
			return ((long)win32FileAttributeData.fileSizeHigh << 32) | (win32FileAttributeData.fileSizeLow & 0xFFFFFFFFu);
		}

		internal static bool InternalExists(string normalizedPath)
		{
			NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data = default(NativeMethods.WIN32_FILE_ATTRIBUTE_DATA);
			if (LongPathCommon.FillAttributeInfo(normalizedPath, ref data, false, false) == 0 && data.fileAttributes != -1)
			{
				return (data.fileAttributes & 0x10) == 0;
			}
			return false;
		}

		private static SafeFileHandle GetFileHandle(string normalizedPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)
		{
			NativeMethods.EFileAccess underlyingAccess = LongPathCommon.GetUnderlyingAccess(access);
			SafeFileHandle safeFileHandle = NativeMethods.CreateFile(normalizedPath, underlyingAccess, (uint)share, IntPtr.Zero, (uint)mode, (uint)options, IntPtr.Zero);
			if (safeFileHandle.IsInvalid)
			{
				throw LongPathCommon.GetExceptionFromLastWin32Error();
			}
			return safeFileHandle;
		}

		private static SafeFileHandle OpenHandle(string normalizedPath)
		{
			return GetFileHandle(normalizedPath, FileMode.Open, FileAccess.Write, FileShare.None, FileOptions.None);
		}
	}
}
