using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	internal static class LongPathCommon
	{
		private static int MAX_LONG_PATH = 32000;

		internal static Exception GetExceptionFromLastWin32Error()
		{
			return GetExceptionFromLastWin32Error("path");
		}

		internal static Exception GetExceptionFromLastWin32Error(string parameterName)
		{
			return GetExceptionFromWin32Error(Marshal.GetLastWin32Error(), parameterName);
		}

		internal static Exception GetExceptionFromWin32Error(int errorCode)
		{
			return GetExceptionFromWin32Error(errorCode, "path");
		}

		internal static Exception GetExceptionFromWin32Error(int errorCode, string parameterName)
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

		internal static string[] ConvertPtrArrayToStringArray(IntPtr strPtrArray, int cStrings)
		{
			IntPtr[] array = new IntPtr[cStrings];
			if (strPtrArray != IntPtr.Zero)
			{
				Marshal.Copy(strPtrArray, array, 0, cStrings);
			}
			List<string> list = new List<string>(cStrings);
			for (int i = 0; i < cStrings; i++)
			{
				list.Add(Marshal.PtrToStringUni(array[i]));
			}
			return list.ToArray();
		}

		public static string NormalizeLongPath(string path)
		{
			StringBuilder stringBuilder = new StringBuilder(MAX_LONG_PATH);
			int num = NativeMethods.IU_GetCanonicalUNCPath(path, stringBuilder, stringBuilder.Capacity);
			if (num != 0)
			{
				throw GetExceptionFromWin32Error(num);
			}
			return stringBuilder.ToString();
		}

		public static FileAttributes GetAttributes(string path)
		{
			FileAttributes fileAttributes = NativeMethods.GetFileAttributes(NormalizeLongPath(path));
			if (fileAttributes == (FileAttributes)(-1))
			{
				throw GetExceptionFromLastWin32Error();
			}
			return fileAttributes;
		}
	}
}
