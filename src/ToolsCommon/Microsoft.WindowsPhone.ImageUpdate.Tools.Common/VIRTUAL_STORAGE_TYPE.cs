using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[CLSCompliant(false)]
	public struct VIRTUAL_STORAGE_TYPE
	{
		public VHD_STORAGE_TYPE_DEVICE DeviceId;

		public Guid VendorId;
	}
}
