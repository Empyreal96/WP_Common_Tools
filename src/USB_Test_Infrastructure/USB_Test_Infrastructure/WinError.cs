namespace USB_Test_Infrastructure
{
	internal enum WinError : uint
	{
		Success = 0u,
		FileNotFound = 2u,
		NoMoreFiles = 18u,
		NotReady = 21u,
		GeneralFailure = 31u,
		InvalidParameter = 87u,
		InsufficientBuffer = 122u,
		IoPending = 997u,
		DeviceNotConnected = 1167u,
		TimeZoneIdInvalid = uint.MaxValue,
		InvalidHandleValue = uint.MaxValue,
		PathNotFound = 3u,
		AlreadyExists = 183u
	}
}
