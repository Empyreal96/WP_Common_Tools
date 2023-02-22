using System;

namespace USB_Test_Infrastructure
{
	internal struct DeviceInformationData
	{
		public int Size;

		public Guid ClassGuid;

		public int DevInst;

		public IntPtr Reserved;
	}
}
