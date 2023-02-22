using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[Flags]
	public enum OPEN_VIRTUAL_DISK_FLAG
	{
		OPEN_VIRTUAL_DISK_FLAG_NONE = 0,
		OPEN_VIRTUAL_DISK_FLAG_NO_PARENTS = 1,
		OPEN_VIRTUAL_DISK_FLAG_BLANK_FILE = 2,
		OPEN_VIRTUAL_DISK_FLAG_BOOT_DRIVE = 4
	}
}
