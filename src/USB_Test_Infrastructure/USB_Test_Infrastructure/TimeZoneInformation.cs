using System.Runtime.InteropServices;

namespace USB_Test_Infrastructure
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct TimeZoneInformation
	{
		public int Bias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string StandardName;

		public SystemTime StandardDate;

		public int StandardBias;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
		public string DaylightName;

		public SystemTime DaylightDate;

		public int DaylightBias;
	}
}
