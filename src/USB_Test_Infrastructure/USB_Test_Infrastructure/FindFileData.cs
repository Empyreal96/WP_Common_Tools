using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace USB_Test_Infrastructure
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct FindFileData
	{
		public uint dwFileAttributes;

		public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;

		public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;

		public uint FileSizeHigh;

		public uint FileSizeLow;

		public uint Reserved0;

		public uint Reserved1;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string FileName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
		public string Alternate;
	}
}
