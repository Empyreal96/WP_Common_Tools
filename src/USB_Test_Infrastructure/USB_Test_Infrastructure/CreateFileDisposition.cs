namespace USB_Test_Infrastructure
{
	internal enum CreateFileDisposition : uint
	{
		CreateNew = 1u,
		CreateAlways,
		CreateExisting,
		OpenAlways,
		TruncateExisting
	}
}
