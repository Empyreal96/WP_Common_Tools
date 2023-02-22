using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using Microsoft.WindowsPhone.ImageUpdate.Tools.Common;

namespace Microsoft.WindowsPhone.ImageUpdate.PkgSignTool
{
	internal class NativeMethods
	{
		private const uint FILE_ATTRIBUTE_NORMAL = 128u;

		private const uint GENERIC_READ = 2147483648u;

		private const uint GENERIC_WRITE = 1073741824u;

		private const uint CREATE_NEW = 1u;

		private const uint CREATE_ALWAYS = 2u;

		private const uint OPEN_EXISTING = 3u;

		private const uint FILE_SHARE_READ = 1u;

		private const uint FILE_SHARE_WRITE = 2u;

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetFileTime(SafeFileHandle hFile, IntPtr lpCreationTime, IntPtr lpLastAccessTime, ref long lpLastWriteTime);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, IntPtr lpLastAccessTime, IntPtr lpLastWriteTime);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetFileTime(SafeFileHandle hFile, out long lpCreationTime, IntPtr lpLastAccessTime, IntPtr lpLastWriteTime);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFile);

		public static void SetLastWriteTimeLong(string file, DateTime time)
		{
			using (SafeFileHandle safeFileHandle = CreateFile(LongPath.GetFullPathUNC(file), 1073741824u, 2u, IntPtr.Zero, 3u, 128u, IntPtr.Zero))
			{
				if (safeFileHandle.IsInvalid)
				{
					throw new Exception("CreateFile() failed while calling SetLastWriteTimeLong().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
				}
				long lpLastWriteTime = time.ToFileTime();
				if (!SetFileTime(safeFileHandle, IntPtr.Zero, IntPtr.Zero, ref lpLastWriteTime))
				{
					throw new Exception("SetFileTime() failed while calling SetLastWriteTimeLong().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		public static void SetCreationTimeLong(string file, DateTime time)
		{
			using (SafeFileHandle safeFileHandle = CreateFile(LongPath.GetFullPathUNC(file), 1073741824u, 2u, IntPtr.Zero, 3u, 128u, IntPtr.Zero))
			{
				if (safeFileHandle.IsInvalid)
				{
					throw new Exception("CreateFile() failed while calling SetCreationTimeLong().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
				}
				long lpCreationTime = time.ToFileTime();
				if (!SetFileTime(safeFileHandle, ref lpCreationTime, IntPtr.Zero, IntPtr.Zero))
				{
					throw new Exception("SetFileTime() failed while calling SetCreationTimeLong().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
				}
			}
		}

		public static DateTime GetCreationTimeLong(string file)
		{
			using (SafeFileHandle safeFileHandle = CreateFile(LongPath.GetFullPathUNC(file), 2147483648u, 1u, IntPtr.Zero, 3u, 128u, IntPtr.Zero))
			{
				if (safeFileHandle.IsInvalid)
				{
					throw new Exception("CreateFile() failed while calling GetCreationTimeLong().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
				}
				long lpCreationTime;
				if (!GetFileTime(safeFileHandle, out lpCreationTime, IntPtr.Zero, IntPtr.Zero))
				{
					throw new Exception("GetFileTime() failed while calling GetCreationTimeLong().  GetLastError() = " + Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture));
				}
				return DateTime.FromFileTime(lpCreationTime);
			}
		}
	}
}
