using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[CLSCompliant(false)]
	public struct CREATE_VIRTUAL_DISK_PARAMETERS
	{
		public CREATE_VIRTUAL_DISK_VERSION Version;

		public CREATE_VIRTUAL_DISK_PARAMETERS_V1 Version1Data;
	}
}
