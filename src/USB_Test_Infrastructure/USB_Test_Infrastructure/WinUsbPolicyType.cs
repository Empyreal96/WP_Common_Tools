namespace USB_Test_Infrastructure
{
	internal enum WinUsbPolicyType : uint
	{
		ShortPacketTerminate = 1u,
		AutoClearStall,
		PipeTransferTimeout,
		IgnoreShortPackets,
		AllowPartialReads,
		AutoFlush,
		RawIO,
		MaximumTransferSize,
		ResetPipeOnResume
	}
}
