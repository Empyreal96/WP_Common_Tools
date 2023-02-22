using System;
using System.Runtime.InteropServices;

namespace USB_Test_Infrastructure
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct DeviceInterfaceData
	{
		public int Size;

		public Guid InterfaceClassGuid;

		public int Flags;

		public IntPtr Reserved;
	}
}
