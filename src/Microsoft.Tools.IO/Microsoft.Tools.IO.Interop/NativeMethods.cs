using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Tools.IO.Interop
{
	internal static class NativeMethods
	{
		[Flags]
		internal enum EFileAccess : uint
		{
			GenericRead = 0x80000000u,
			GenericWrite = 0x40000000u,
			GenericExecute = 0x20000000u,
			GenericAll = 0x10000000u
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct WIN32_FIND_DATA
		{
			internal FileAttributes dwFileAttributes;

			internal System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;

			internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;

			internal System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;

			internal int nFileSizeHigh;

			internal int nFileSizeLow;

			internal int dwReserved0;

			internal int dwReserved1;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			internal string cFileName;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			internal string cAlternate;
		}

		[Serializable]
		internal struct WIN32_FILE_ATTRIBUTE_DATA
		{
			internal int fileAttributes;

			internal uint ftCreationTimeLow;

			internal uint ftCreationTimeHigh;

			internal uint ftLastAccessTimeLow;

			internal uint ftLastAccessTimeHigh;

			internal uint ftLastWriteTimeLow;

			internal uint ftLastWriteTimeHigh;

			internal int fileSizeHigh;

			internal int fileSizeLow;

			[SecurityCritical]
			internal void PopulateFrom(WIN32_FIND_DATA findData)
			{
				fileAttributes = (int)findData.dwFileAttributes;
				ftCreationTimeLow = (uint)findData.ftCreationTime.dwLowDateTime;
				ftCreationTimeHigh = (uint)findData.ftCreationTime.dwHighDateTime;
				ftLastAccessTimeLow = (uint)findData.ftLastAccessTime.dwLowDateTime;
				ftLastAccessTimeHigh = (uint)findData.ftLastAccessTime.dwHighDateTime;
				ftLastWriteTimeLow = (uint)findData.ftLastWriteTime.dwLowDateTime;
				ftLastWriteTimeHigh = (uint)findData.ftLastWriteTime.dwHighDateTime;
				fileSizeHigh = findData.nFileSizeHigh;
				fileSizeLow = findData.nFileSizeLow;
			}
		}

		internal const int ERROR_FILE_NOT_FOUND = 2;

		internal const int ERROR_PATH_NOT_FOUND = 3;

		internal const int ERROR_ACCESS_DENIED = 5;

		internal const int ERROR_INVALID_DRIVE = 15;

		internal const int ERROR_NO_MORE_FILES = 18;

		internal const int ERROR_NOT_READY = 21;

		internal const int ERROR_INVALID_PARAMETER = 87;

		internal const int ERROR_INVALID_NAME = 123;

		internal const int ERROR_ALREADY_EXISTS = 183;

		internal const int ERROR_FILENAME_EXCED_RANGE = 206;

		internal const int ERROR_DIRECTORY = 267;

		internal const int ERROR_OPERATION_ABORTED = 995;

		internal const int INVALID_FILE_ATTRIBUTES = -1;

		internal const int SEM_FAILCRITICALERRORS = 1;

		internal const int FILE_TYPE_DISK = 1;

		internal const int FILE_TYPE_CHAR = 2;

		internal const int FILE_TYPE_PIPE = 3;

		internal const int MAX_PATH = 260;

		internal const int MAX_LONG_PATH = 32000;

		internal const int MAX_ALTERNATE = 14;

		internal const string LongPathPrefix = "\\\\?\\";

		internal const string LongUncPathPrefix = "\\\\?\\UNC\\";

		internal const int FORMAT_MESSAGE_IGNORE_INSERTS = 512;

		internal const int FORMAT_MESSAGE_FROM_SYSTEM = 4096;

		internal const int FORMAT_MESSAGE_ARGUMENT_ARRAY = 8192;

		internal static int MakeHRFromErrorCode(int errorCode)
		{
			return -2147024896 | errorCode;
		}

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CopyFile(string src, string dst, [MarshalAs(UnmanagedType.Bool)] bool failIfExists);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern SafeFindHandle FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FindNextFile(SafeFindHandle hFindFile, out WIN32_FIND_DATA lpFindFileData);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool FindClose(IntPtr hFindFile);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern uint GetFullPathName(string lpFileName, uint nBufferLength, StringBuilder lpBuffer, IntPtr mustBeNull);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DeleteFile(string lpFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool RemoveDirectory(string lpPathName);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool CreateDirectory(string lpPathName, IntPtr lpSecurityAttributes);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool MoveFile(string lpPathNameFrom, string lpPathNameTo);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern SafeFileHandle CreateFile(string lpFileName, EFileAccess dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern FileAttributes GetFileAttributes(string lpFileName);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int FormatMessage(int dwFlags, IntPtr lpSource, int dwMessageId, int dwLanguageId, StringBuilder lpBuffer, int nSize, IntPtr va_list_arguments);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
		internal static extern int SetErrorMode(int newMode);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetFileAttributes(string name, int attr);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern int GetFileType(SafeFileHandle handle);

		internal static SafeFileHandle SafeCreateFile(string lpFileName, EFileAccess dwDesiredAccess, uint dwShareMode, IntPtr securityAttrs, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile)
		{
			SafeFileHandle safeFileHandle = CreateFile(lpFileName, dwDesiredAccess, dwShareMode, securityAttrs, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
			if (!safeFileHandle.IsInvalid && GetFileType(safeFileHandle) != 1)
			{
				safeFileHandle.Dispose();
				throw new NotSupportedException("FileStream was asked to open a device that was not a file. For support for devices like 'com1:' or 'lpt1:', call CreateFile, then use the FileStream constructors that take an OS handle as an IntPtr.");
			}
			return safeFileHandle;
		}
	}
}
