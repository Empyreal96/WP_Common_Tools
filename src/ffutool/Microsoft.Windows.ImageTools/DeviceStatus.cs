namespace Microsoft.Windows.ImageTools
{
	internal enum DeviceStatus
	{
		CONNECTED,
		FLASHING,
		TRANSFER_WIM,
		BOOTING_WIM,
		DONE,
		EXCEPTION,
		ERROR,
		MESSAGE
	}
}
