using System.Runtime.InteropServices;

namespace USB_Test_Infrastructure
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
	internal struct DeviceInterfaceDetailData
	{
		public int Size;

		public char DevicePath;
	}
}
