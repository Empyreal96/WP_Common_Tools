using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Tools.IO.Interop;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Tools.IO
{
	internal static class LongPathCommon
	{
		internal static string NormalizeSearchPattern(string searchPattern)
		{
			if (string.IsNullOrEmpty(searchPattern) || searchPattern == ".")
			{
				return "*";
			}
			return searchPattern;
		}

		internal static string NormalizeLongPath(string path)
		{
			return NormalizeLongPath(path, "path");
		}

		internal static string NormalizeLongPath(string path, string parameterName)
		{
			if (path == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			if (path.Length == 0)
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'{0}' cannot be an empty string.", new object[1] { parameterName }), parameterName);
			}
			StringBuilder stringBuilder = new StringBuilder(path.Length + 1);
			uint fullPathName = NativeMethods.GetFullPathName(path, (uint)stringBuilder.Capacity, stringBuilder, IntPtr.Zero);
			if (fullPathName > stringBuilder.Capacity)
			{
				stringBuilder.Capacity = (int)fullPathName;
				fullPathName = NativeMethods.GetFullPathName(path, fullPathName, stringBuilder, IntPtr.Zero);
			}
			if (fullPathName == 0)
			{
				throw GetExceptionFromLastWin32Error(parameterName);
			}
			if (fullPathName > 32000)
			{
				throw GetExceptionFromWin32Error(206, parameterName);
			}
			return AddLongPathPrefix(stringBuilder.ToString());
		}

		private static bool TryNormalizeLongPath(string path, out string result)
		{
			try
			{
				result = NormalizeLongPath(path);
				return true;
			}
			catch (ArgumentException)
			{
			}
			catch (PathTooLongException)
			{
			}
			result = null;
			return false;
		}

		private static string AddLongPathPrefix(string path)
		{
			if (!path.StartsWith("\\\\"))
			{
				return "\\\\?\\" + path;
			}
			return "\\\\?\\UNC\\" + path.Substring(2);
		}

		internal static string RemoveLongPathPrefix(string normalizedPath)
		{
			if (!normalizedPath.StartsWith("\\\\?\\UNC\\"))
			{
				return normalizedPath.Substring("\\\\?\\".Length);
			}
			return "\\\\" + normalizedPath.Substring("\\\\?\\UNC\\".Length);
		}

		internal static bool Exists(string path, out bool isDirectory)
		{
			string result;
			FileAttributes attributes;
			if (TryNormalizeLongPath(path, out result) && TryGetFileAttributes(result, out attributes) == 0)
			{
				isDirectory = LongPathDirectory.IsDirectory(attributes);
				return true;
			}
			isDirectory = false;
			return false;
		}

		internal static int TryGetDirectoryAttributes(string normalizedPath, out FileAttributes attributes)
		{
			int result = TryGetFileAttributes(normalizedPath, out attributes);
			if (!LongPathDirectory.IsDirectory(attributes))
			{
				result = 267;
			}
			return result;
		}

		internal static int TryGetFileAttributes(string normalizedPath, out FileAttributes attributes)
		{
			attributes = NativeMethods.GetFileAttributes(normalizedPath);
			if (attributes == (FileAttributes)(-1))
			{
				return Marshal.GetLastWin32Error();
			}
			return 0;
		}

		internal static FileAttributes GetAttributes(string path)
		{
			return (FileAttributes)GetWin32FileAttributeData(NormalizeLongPath(path)).fileAttributes;
		}

		internal static void SetAttributes(string path, FileAttributes attributes)
		{
			if (!NativeMethods.SetFileAttributes(NormalizeLongPath(path), (int)attributes))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				switch (lastWin32Error)
				{
				case 87:
					throw new ArgumentException("Invalid File or Directory attributes value.");
				case 5:
					throw new ArgumentException("Access to the path is denied.");
				default:
					throw GetExceptionFromWin32Error(lastWin32Error);
				}
			}
		}

		internal static void SetFileTimes(SafeFileHandle hFile, long creationTime, long accessTime, long writeTime)
		{
			if (!NativeMethods.SetFileTime(hFile, ref creationTime, ref accessTime, ref writeTime))
			{
				throw GetExceptionFromWin32Error(Marshal.GetLastWin32Error());
			}
		}

		internal static NativeMethods.WIN32_FILE_ATTRIBUTE_DATA GetWin32FileAttributeData(string normalizedPath)
		{
			NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data = default(NativeMethods.WIN32_FILE_ATTRIBUTE_DATA);
			int num = FillAttributeInfo(normalizedPath, ref data, false, false);
			if (num != 0)
			{
				throw GetExceptionFromWin32Error(num);
			}
			return data;
		}

		internal static int FillAttributeInfo(string normalizedLongPath, ref NativeMethods.WIN32_FILE_ATTRIBUTE_DATA data, bool tryagain, bool returnErrorOnNotFound)
		{
			int num = 0;
			if (tryagain)
			{
				string lpFileName = normalizedLongPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				int errorMode = NativeMethods.SetErrorMode(1);
				NativeMethods.WIN32_FIND_DATA lpFindFileData;
				try
				{
					bool flag = false;
					SafeFindHandle safeFindHandle = NativeMethods.FindFirstFile(lpFileName, out lpFindFileData);
					try
					{
						if (safeFindHandle.IsInvalid)
						{
							flag = true;
							num = Marshal.GetLastWin32Error();
							if ((num == 2 || num == 3 || num == 21) && !returnErrorOnNotFound)
							{
								num = 0;
								data.fileAttributes = -1;
							}
							return num;
						}
					}
					finally
					{
						try
						{
							safeFindHandle.Close();
						}
						catch
						{
							if (!flag)
							{
								throw GetExceptionFromLastWin32Error("handle");
							}
						}
					}
				}
				finally
				{
					NativeMethods.SetErrorMode(errorMode);
				}
				data.PopulateFrom(lpFindFileData);
			}
			else
			{
				int errorMode2 = NativeMethods.SetErrorMode(1);
				bool fileAttributesEx;
				try
				{
					fileAttributesEx = NativeMethods.GetFileAttributesEx(normalizedLongPath, 0, ref data);
				}
				finally
				{
					NativeMethods.SetErrorMode(errorMode2);
				}
				if (!fileAttributesEx)
				{
					num = Marshal.GetLastWin32Error();
					if (num != 2 && num != 3 && num != 21)
					{
						return FillAttributeInfo(normalizedLongPath, ref data, true, returnErrorOnNotFound);
					}
					if (!returnErrorOnNotFound)
					{
						num = 0;
						data.fileAttributes = -1;
					}
				}
			}
			return num;
		}

		internal static NativeMethods.EFileAccess GetUnderlyingAccess(FileAccess access)
		{
			switch (access)
			{
			case FileAccess.Read:
				return NativeMethods.EFileAccess.GenericRead;
			case FileAccess.Write:
				return NativeMethods.EFileAccess.GenericWrite;
			case FileAccess.ReadWrite:
				return NativeMethods.EFileAccess.GenericRead | NativeMethods.EFileAccess.GenericWrite;
			default:
				throw new ArgumentOutOfRangeException("access");
			}
		}

		internal static Exception GetExceptionFromLastWin32Error(string parameterName = "path")
		{
			return GetExceptionFromWin32Error(Marshal.GetLastWin32Error(), parameterName);
		}

		internal static Exception GetExceptionFromWin32Error(int errorCode, string parameterName = "path")
		{
			string messageFromErrorCode = GetMessageFromErrorCode(errorCode);
			switch (errorCode)
			{
			case 2:
				return new FileNotFoundException(messageFromErrorCode);
			case 3:
				return new DirectoryNotFoundException(messageFromErrorCode);
			case 5:
				return new UnauthorizedAccessException(messageFromErrorCode);
			case 206:
				return new PathTooLongException(messageFromErrorCode);
			case 15:
				return new DriveNotFoundException(messageFromErrorCode);
			case 995:
				return new OperationCanceledException(messageFromErrorCode);
			case 123:
				return new ArgumentException(messageFromErrorCode, parameterName);
			default:
				return new IOException(messageFromErrorCode, NativeMethods.MakeHRFromErrorCode(errorCode));
			}
		}

		private static string GetMessageFromErrorCode(int errorCode)
		{
			StringBuilder stringBuilder = new StringBuilder(512);
			NativeMethods.FormatMessage(12800, IntPtr.Zero, errorCode, 0, stringBuilder, stringBuilder.Capacity, IntPtr.Zero);
			return stringBuilder.ToString();
		}
	}
}
