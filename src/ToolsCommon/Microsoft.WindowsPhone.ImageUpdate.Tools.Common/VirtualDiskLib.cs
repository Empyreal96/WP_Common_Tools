using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public static class VirtualDiskLib
	{
		[DllImport("Virtdisk.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		[CLSCompliant(false)]
		public static extern int OpenVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, [MarshalAs(UnmanagedType.LPWStr)] string Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, OPEN_VIRTUAL_DISK_FLAG Flags, ref OPEN_VIRTUAL_DISK_PARAMETERS Parameters, ref IntPtr Handle);

		[DllImport("Virtdisk.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		[CLSCompliant(false)]
		public static extern int OpenVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, [MarshalAs(UnmanagedType.LPWStr)] string Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, OPEN_VIRTUAL_DISK_FLAG Flags, IntPtr Parameters, ref IntPtr Handle);

		[DllImport("Virtdisk.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern int GetVirtualDiskPhysicalPath(IntPtr VirtualDiskHandle, ref int DiskPathSizeInBytes, StringBuilder DiskPath);

		[DllImport("Virtdisk.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		[CLSCompliant(false)]
		public static extern int AttachVirtualDisk(IntPtr VirtualDiskHandle, IntPtr SecurityDescriptor, ATTACH_VIRTUAL_DISK_FLAG Flags, uint ProviderSpecificFlags, ref ATTACH_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped);

		[DllImport("Virtdisk.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		[CLSCompliant(false)]
		public static extern int DetachVirtualDisk(IntPtr VirtualDiskHandle, DETACH_VIRTUAL_DISK_FLAG Flags, uint ProviderSpecificFlags);

		[DllImport("Virtdisk.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
		[CLSCompliant(false)]
		public static extern int CreateVirtualDisk(ref VIRTUAL_STORAGE_TYPE VirtualStorageType, [MarshalAs(UnmanagedType.LPWStr)] string Path, VIRTUAL_DISK_ACCESS_MASK VirtualDiskAccessMask, IntPtr SecurityDescriptor, CREATE_VIRTUAL_DISK_FLAG Flags, uint ProviderSpecificFlags, ref CREATE_VIRTUAL_DISK_PARAMETERS Parameters, IntPtr Overlapped, ref IntPtr Handle);

		[DllImport("kernel32.dll")]
		public static extern bool CloseHandle(IntPtr hObject);
	}
}
