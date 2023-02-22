using System.Runtime.InteropServices;

namespace FFUComponents
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	internal struct WinUsbSetupPacket
	{
		public byte RequestType;

		public byte Request;

		public ushort Value;

		public ushort Index;

		public ushort Length;
	}
}
