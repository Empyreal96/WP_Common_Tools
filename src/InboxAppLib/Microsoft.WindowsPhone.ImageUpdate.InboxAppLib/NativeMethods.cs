using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.InboxAppLib
{
	public static class NativeMethods
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct PACKAGE_ID
		{
			public uint reserved;

			public uint processorArchitecture;

			public short Revision;

			public short Build;

			public short Minor;

			public short Major;

			public string name;

			public string publisher;

			public string resourceId;

			public string publisherId;
		}

		private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

		[DllImport("Kernel32.dll")]
		internal static extern int PackageFullNameFromId(ref PACKAGE_ID packageId, ref uint packageFullNameLength, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder packageFullName);

		[DllImport("AppxPackaging.dll", EntryPoint = "#4")]
		internal static extern int Unbundle([MarshalAs(UnmanagedType.LPWStr)] string inputBundlePath, [MarshalAs(UnmanagedType.LPWStr)] string outputDirectoryPath, [MarshalAs(UnmanagedType.Bool)] bool createMonikerSubdirectory, IntPtr messageHandler, [MarshalAs(UnmanagedType.LPWStr)] ref string destinationDirectory);

		[DllImport("AppxPackaging.dll", EntryPoint = "#3")]
		internal static extern int Unpack([MarshalAs(UnmanagedType.LPWStr)] string inputPackagePath, [MarshalAs(UnmanagedType.LPWStr)] string outputDirectoryPath, [MarshalAs(UnmanagedType.Bool)] bool createMonikerSubdirectory, IntPtr messageHandler, [MarshalAs(UnmanagedType.LPWStr)] ref string destinationDirectory);

		[DllImport("BCP47Langs.dll")]
		internal static extern int Bcp47GetNlsForm([MarshalAs(UnmanagedType.HString)] string languageTag, [MarshalAs(UnmanagedType.HString)] ref string nlsForm);
	}
}
