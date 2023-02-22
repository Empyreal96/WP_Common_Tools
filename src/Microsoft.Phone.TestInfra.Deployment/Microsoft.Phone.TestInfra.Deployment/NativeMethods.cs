using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Microsoft.Phone.TestInfra.Deployment
{
	internal static class NativeMethods
	{
		public enum SymbolicLinkFlag
		{
			File,
			Directory
		}

		public const uint FileWriteAttributes = 256u;

		public const uint ShareModeRead = 1u;

		public const uint ShareModeWrite = 2u;

		public const uint OpenExisting = 3u;

		public const uint FileFlagOpenReparsePoint = 2097152u;

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.I1)]
		public static extern bool CreateSymbolicLink(string linkFileName, string targetFileName, SymbolicLinkFlag flags);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetFileTime(SafeFileHandle hFile, ref long lpCreationTime, ref long lpLastAccessTime, ref long lpLastWriteTime);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, [MarshalAs(UnmanagedType.U4)] uint dwDesiredAccess, [MarshalAs(UnmanagedType.U4)] uint dwShareMode, IntPtr lpSecurityAttributes, [MarshalAs(UnmanagedType.U4)] uint dwCreationDisposition, [MarshalAs(UnmanagedType.U4)] uint dwFlagsAndAttributes, IntPtr hTemplateFile);
	}
}
