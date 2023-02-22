using System;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal class SP_DEVICE_INTERFACE_DETAIL_DATA
	{
		public uint cbSize;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		public string DevicePath;

		public SP_DEVICE_INTERFACE_DETAIL_DATA()
		{
			if (IntPtr.Size == 4)
			{
				cbSize = 6u;
			}
			else
			{
				cbSize = 8u;
			}
		}
	}
}
