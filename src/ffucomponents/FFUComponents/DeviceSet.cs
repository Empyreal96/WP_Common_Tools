using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace FFUComponents
{
	public class DeviceSet
	{
		private IntPtr deviceSetHandle;

		private SP_DEVINFO_DATA deviceInfoData;

		public DeviceSet(string DevinstId)
		{
			Initialize(DevinstId);
		}

		public uint GetAddress()
		{
			uint PropertyBuffer = 0u;
			uint PropertyType;
			if (!NativeMethods.SetupDiGetDeviceProperty(deviceSetHandle, deviceInfoData, NativeMethods.DEVPKEY_Device_Address, out PropertyType, out PropertyBuffer, (uint)Marshal.SizeOf(PropertyBuffer), IntPtr.Zero, 0u))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_SETUP_DI_GET_DEVICE_PROPERTY, new object[1] { lastWin32Error }));
			}
			return PropertyBuffer;
		}

		public string GetParentId()
		{
			uint pdnDevInst;
			uint num = NativeMethods.CM_Get_Parent(out pdnDevInst, deviceInfoData.DevInst, 0u);
			if (num != 0)
			{
				throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_CM_GET_PARENT, new object[1] { num }));
			}
			return DeviceIdFromCmDevinst(pdnDevInst);
		}

		public string GetHubDevicePath()
		{
			SP_DEVICE_INTERFACE_DATA deviceInterfaceData = new SP_DEVICE_INTERFACE_DATA();
			if (!NativeMethods.SetupDiEnumDeviceInterfaces(deviceSetHandle, IntPtr.Zero, ref NativeMethods.GUID_DEVINTERFACE_USB_HUB, 0u, deviceInterfaceData))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_SETUP_DI_ENUM_DEVICE_INTERFACES, new object[1] { lastWin32Error }));
			}
			SP_DEVICE_INTERFACE_DETAIL_DATA sP_DEVICE_INTERFACE_DETAIL_DATA = new SP_DEVICE_INTERFACE_DETAIL_DATA();
			if (!NativeMethods.SetupDiGetDeviceInterfaceDetailW(deviceSetHandle, deviceInterfaceData, sP_DEVICE_INTERFACE_DETAIL_DATA, (uint)Marshal.SizeOf(sP_DEVICE_INTERFACE_DETAIL_DATA), IntPtr.Zero, IntPtr.Zero))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_SETUP_DI_GET_DEVICE_INTERFACE_DETAIL_W, new object[1] { lastWin32Error }));
			}
			return sP_DEVICE_INTERFACE_DETAIL_DATA.DevicePath.ToString();
		}

		private static string DeviceIdFromCmDevinst(uint devinst)
		{
			uint pulLen;
			uint num = NativeMethods.CM_Get_Device_ID_Size(out pulLen, devinst, 0u);
			if (num != 0)
			{
				throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_CM_GET_DEVICE_ID_SIZE, new object[1] { num }));
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Capacity = (int)(pulLen + 1);
			num = NativeMethods.CM_Get_Device_ID(devinst, stringBuilder, (uint)stringBuilder.Capacity, 0u);
			if (num != 0)
			{
				throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_CM_GET_DEVICE_ID, new object[1] { num }));
			}
			return stringBuilder.ToString();
		}

		private void GetDeviceInfoData()
		{
			SP_DEVINFO_DATA sP_DEVINFO_DATA = new SP_DEVINFO_DATA();
			if (NativeMethods.SetupDiEnumDeviceInfo(deviceSetHandle, 0u, sP_DEVINFO_DATA))
			{
				deviceInfoData = sP_DEVINFO_DATA;
				return;
			}
			int lastWin32Error = Marshal.GetLastWin32Error();
			throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_SETUP_DI_ENUM_DEVICE_INFO, new object[1] { lastWin32Error }));
		}

		private void Initialize(string DevinstId)
		{
			if (string.IsNullOrEmpty(DevinstId))
			{
				throw new FFUException(Resources.ERROR_NULL_OR_EMPTY_STRING);
			}
			deviceInfoData = null;
			deviceSetHandle = NativeMethods.SetupDiGetClassDevs(IntPtr.Zero, DevinstId, IntPtr.Zero, 22u);
			if (deviceSetHandle == NativeMethods.INVALID_HANDLE_VALUE)
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				throw new FFUException(string.Format(CultureInfo.CurrentCulture, Resources.ERROR_INVALID_HANDLE, new object[2] { DevinstId, lastWin32Error }));
			}
			GetDeviceInfoData();
		}

		~DeviceSet()
		{
			if (NativeMethods.INVALID_HANDLE_VALUE != deviceSetHandle)
			{
				NativeMethods.SetupDiDestroyDeviceInfoList(deviceSetHandle);
				deviceSetHandle = NativeMethods.INVALID_HANDLE_VALUE;
			}
		}
	}
}
