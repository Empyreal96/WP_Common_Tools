using System;
using System.Runtime.InteropServices;

namespace FFUComponents
{
	[StructLayout(LayoutKind.Sequential)]
	public class SP_DEVICE_INTERFACE_DATA
	{
		public uint cbSize;

		public Guid InterfaceClassGuid;

		public uint Flags;

		private IntPtr Reserved;

		public SP_DEVICE_INTERFACE_DATA()
		{
			cbSize = (uint)Marshal.SizeOf(this);
		}
	}
}
