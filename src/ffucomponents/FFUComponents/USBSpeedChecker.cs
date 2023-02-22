using System;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FFUComponents
{
	public class USBSpeedChecker
	{
		private string UsbDevicePath;

		public USBSpeedChecker(string UsbDevicePath)
		{
			if (string.IsNullOrEmpty(UsbDevicePath))
			{
				throw new FFUException(Resources.ERROR_NULL_OR_EMPTY_STRING);
			}
			this.UsbDevicePath = UsbDevicePath;
		}

		public ConnectionType GetConnectionSpeed()
		{
			char[] separator = new char[1] { '#' };
			string[] array = UsbDevicePath.Split(separator);
			string arg = array[1];
			string arg2 = array[2];
			string devinstId = $"usb\\{arg}\\{arg2}";
			try
			{
				DeviceSet deviceSet = new DeviceSet(devinstId);
				uint address = deviceSet.GetAddress();
				string hubDevicePath = new DeviceSet(deviceSet.GetParentId()).GetHubDevicePath();
				USB_NODE_CONNECTION_INFORMATION_EX_V2 uSB_NODE_CONNECTION_INFORMATION_EX_V = new USB_NODE_CONNECTION_INFORMATION_EX_V2(address);
				SafeFileHandle safeFileHandle;
				bool flag;
				using (safeFileHandle = NativeMethods.CreateFile(hubDevicePath, 0u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero))
				{
					if (safeFileHandle.IsInvalid)
					{
						int lastWin32Error = Marshal.GetLastWin32Error();
						throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_INVALID_HANDLE, new object[2] { hubDevicePath, lastWin32Error }));
					}
					uint lpBytesReturned;
					flag = NativeMethods.DeviceIoControl(safeFileHandle, NativeMethods.IOCTL_USB_GET_NODE_CONNECTION_INFORMATION_EX_V2, uSB_NODE_CONNECTION_INFORMATION_EX_V, (uint)Marshal.SizeOf(uSB_NODE_CONNECTION_INFORMATION_EX_V), uSB_NODE_CONNECTION_INFORMATION_EX_V, (uint)Marshal.SizeOf(uSB_NODE_CONNECTION_INFORMATION_EX_V), out lpBytesReturned, IntPtr.Zero);
				}
				if (!flag)
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_DEVICE_IO_CONTROL, new object[1] { lastWin32Error }));
				}
				if ((uSB_NODE_CONNECTION_INFORMATION_EX_V.Flags & 1) == 1)
				{
					return ConnectionType.SuperSpeed3;
				}
				return ConnectionType.HighSpeed;
			}
			catch (FFUException)
			{
				throw;
			}
		}
	}
}
