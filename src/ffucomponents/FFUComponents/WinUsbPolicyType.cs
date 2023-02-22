namespace FFUComponents
{
	internal enum WinUsbPolicyType : uint
	{
		ShortPacketTerminate = 1u,
		AutoClearStall,
		PipeTransferTimeout,
		IgnoreShortPackets,
		AllowPartialReads,
		AutoFlush,
		RawIO
	}
}
