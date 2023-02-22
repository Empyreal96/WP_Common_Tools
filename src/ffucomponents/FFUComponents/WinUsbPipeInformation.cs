namespace FFUComponents
{
	internal struct WinUsbPipeInformation
	{
		public WinUsbPipeType PipeType;

		public byte PipeId;

		public ushort MaximumPacketSize;

		public byte Interval;
	}
}
