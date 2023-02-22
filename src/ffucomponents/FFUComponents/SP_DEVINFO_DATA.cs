using System;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[StructLayout(LayoutKind.Sequential)]
	public class SP_DEVINFO_DATA
	{
		public uint cbSize;

		public Guid ClassGuid;

		public uint DevInst;

		public IntPtr Reserved;

		public SP_DEVINFO_DATA()
		{
			cbSize = (uint)Marshal.SizeOf(this);
		}
	}
}
