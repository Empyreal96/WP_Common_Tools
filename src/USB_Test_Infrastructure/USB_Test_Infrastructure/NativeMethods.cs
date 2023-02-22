using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace USB_Test_Infrastructure
{
	internal static class NativeMethods
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern uint GetTimeZoneInformation(out TimeZoneInformation timeZoneInformation);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern void GetSystemTime(out SystemTime systemTime);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_Initialize", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbInitialize(SafeFileHandle deviceHandle, ref IntPtr interfaceHandle);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_Free", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbFree(IntPtr interfaceHandle);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_ControlTransfer", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbControlTransfer(IntPtr interfaceHandle, WinUsbSetupPacket setupPacket, IntPtr buffer, uint bufferLength, ref uint lengthTransferred, IntPtr overlapped);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_ControlTransfer", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal unsafe static extern bool WinUsbControlTransfer(IntPtr interfaceHandle, WinUsbSetupPacket setupPacket, byte* buffer, uint bufferLength, ref uint lengthTransferred, IntPtr overlapped);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_QueryInterfaceSettings", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbQueryInterfaceSettings(IntPtr interfaceHandle, byte alternateInterfaceNumber, ref WinUsbInterfaceDescriptor usbAltInterfaceDescriptor);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_QueryPipe", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbQueryPipe(IntPtr interfaceHandle, byte alternateInterfaceNumber, byte pipeIndex, ref WinUsbPipeInformation pipeInformation);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_SetPipePolicy", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbSetPipePolicy(IntPtr interfaceHandle, byte pipeID, uint policyType, uint valueLength, ref bool value);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_SetPipePolicy", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbSetPipePolicy(IntPtr interfaceHandle, byte pipeID, uint policyType, uint valueLength, ref uint value);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_ResetPipe", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbResetPipe(IntPtr interfaceHandle, byte pipeID);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_AbortPipe", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbAbortPipe(IntPtr interfaceHandle, byte pipeID);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_FlushPipe", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool WinUsbFlushPipe(IntPtr interfaceHandle, byte pipeID);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_ReadPipe", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal unsafe static extern bool WinUsbReadPipe(IntPtr interfaceHandle, byte pipeID, byte* buffer, uint bufferLength, IntPtr lenghtTransferred, NativeOverlapped* overlapped);

		[DllImport("winusb.dll", EntryPoint = "WinUsb_WritePipe", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal unsafe static extern bool WinUsbWritePipe(IntPtr interfaceHandle, byte pipeID, byte* buffer, uint bufferLength, IntPtr lenghtTransferred, NativeOverlapped* overlapped);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern IntPtr SetupDiGetClassDevs(ref Guid classGuid, string enumerator, int parent, int flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr deviceInfoSet, int deviceInfoData, ref Guid interfaceClassGuid, int memberIndex, ref DeviceInterfaceData deviceInterfaceData);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref DeviceInterfaceData deviceInterfaceData, IntPtr deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, IntPtr deviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal unsafe static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref DeviceInterfaceData deviceInterfaceData, DeviceInterfaceDetailData* deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, ref DeviceInformationData deviceInfoData);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool GetDiskFreeSpace(string pathName, out uint sectorsPerCluster, out uint bytesPerSector, out uint numberOfFreeClusters, out uint totalNumberOfClusters);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle FindFirstFile(string fileName, ref FindFileData findFileData);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool FindNextFile(SafeFileHandle findFileHandle, ref FindFileData findFileData);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool FindClose(SafeFileHandle findFileHandle);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFileHandle);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern void CloseHandle(SafeHandle handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public unsafe static extern bool ReadFile(SafeFileHandle handle, byte* buffer, int numBytesToRead, IntPtr numBytesRead, NativeOverlapped* overlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		public unsafe static extern bool WriteFile(SafeFileHandle handle, byte* buffer, int numBytesToWrite, IntPtr numBytesWritten, NativeOverlapped* overlapped);

		[DllImport("iphlpapi.dll", ExactSpelling = true)]
		public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);
	}
}
