using System.Runtime.InteropServices;

namespace FFUComponents
{
	[StructLayout(LayoutKind.Sequential)]
	internal class USB_NODE_CONNECTION_INFORMATION_EX_V2
	{
		public uint ConnectionIndex;

		public uint Length;

		public uint SupportedUsbProtocols;

		public uint Flags;

		public USB_NODE_CONNECTION_INFORMATION_EX_V2(uint portNumber)
		{
			ConnectionIndex = portNumber;
			Length = (uint)Marshal.SizeOf(this);
			SupportedUsbProtocols = 7u;
			Flags = 0u;
		}
	}
}
