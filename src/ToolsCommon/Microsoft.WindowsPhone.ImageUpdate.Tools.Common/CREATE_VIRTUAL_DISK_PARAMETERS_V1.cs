using System;
using System.Runtime.InteropServices;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[CLSCompliant(false)]
	public struct CREATE_VIRTUAL_DISK_PARAMETERS_V1
	{
		public Guid UniqueId;

		public ulong MaximumSize;

		public uint BlockSizeInBytes;

		public uint SectorSizeInBytes;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string ParentPath;

		[MarshalAs(UnmanagedType.LPWStr)]
		public string SourcePath;
	}
}
