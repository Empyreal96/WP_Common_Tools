using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;

namespace FFUComponents
{
	internal static class NativeMethods
	{
		internal static DEVPROPKEY DEVPKEY_Device_Address = new DEVPROPKEY(new Guid(2757502286u, 57116, 20221, 128, 32, 103, 209, 70, 168, 80, 224), 30u);

		internal const uint CR_SUCCESS = 0u;

		internal static IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

		internal static Guid GUID_DEVINTERFACE_USB_HUB = new Guid(4052356744u, 49932, 4560, 136, 21, 0, 160, 201, 6, 190, 216);

		internal const uint GENERIC_WRITE = 1073741824u;

		internal const uint FILE_SHARE_READ = 1u;

		internal const uint FILE_SHARE_WRITE = 2u;

		internal const uint FILE_SHARE_DELETE = 4u;

		internal const uint USB_GET_NODE_CONNECTION_INFORMATION_EX_V2 = 279u;

		internal const uint FILE_DEVICE_USB = 34u;

		internal const uint FILE_DEVICE_UNKNOWN = 34u;

		internal const uint METHOD_BUFFERED = 0u;

		internal const uint FILE_ANY_ACCESS = 0u;

		internal static uint IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX_V2 = CTL_CODE(34u, 279u, 0u, 0u);

		internal static uint CTL_CODE(uint DeviceType, uint Function, uint Method, uint Access)
		{
			return (DeviceType << 16) | (Access << 14) | (Function << 2) | Method;
		}

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

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern SafeFileHandle CreateFile(string fileName, uint desiredAccess, uint shareMode, IntPtr securityAttributes, uint creationDisposition, uint flagsAndAttributes, IntPtr templateFileHandle);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CancelIo(SafeFileHandle handle);

		[DllImport("iphlpapi.dll", ExactSpelling = true)]
		public static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, uint Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, SP_DEVINFO_DATA DeviceInfoData);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetupDiGetDeviceProperty(IntPtr DeviceInfoSet, SP_DEVINFO_DATA DeviceInfoData, DEVPROPKEY PropertyKey, out uint PropertyType, out uint PropertyBuffer, uint PropertyBufferSize, IntPtr RequiredSize, uint Flags);

		[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool SetupDiEnumDeviceInterfaces(IntPtr DeviceInfoSet, IntPtr DeviceInfoData, ref Guid InterfaceClassGuid, uint MemberIndex, SP_DEVICE_INTERFACE_DATA DeviceInterfaceData);

		[DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
		internal static extern uint CM_Get_Parent(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

		[DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
		internal static extern uint CM_Get_Device_ID_Size(out uint pulLen, uint dnDevInst, uint ulFlags);

		[DllImport("cfgmgr32.dll", CharSet = CharSet.Auto)]
		internal static extern uint CM_Get_Device_ID(uint dnDevInst, StringBuilder Buffer, uint BufferLen, uint ulFlags);

		[DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		internal static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr DeviceInfoSet, SP_DEVICE_INTERFACE_DATA DeviceInterfaceData, [In][Out] SP_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, uint DeviceInterfaceDetailDataSize, IntPtr RequiredSize, IntPtr DeviceInfoData);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern bool DeviceIoControl(SafeFileHandle hDevice, uint dwIoControlCode, USB_NODE_CONNECTION_INFORMATION_EX_V2 lpInBuffer, uint nInBufferSize, USB_NODE_CONNECTION_INFORMATION_EX_V2 lpOutBuffer, uint nOutBufferSize, out uint lpBytesReturned, IntPtr lpOverlapped);
	}
}
