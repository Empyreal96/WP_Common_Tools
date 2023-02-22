using System;
using System.Collections;
using System.ComponentModel;
using System.Management;
using System.Runtime.InteropServices;

namespace USB_Test_Infrastructure
{
	public class UsbConnectionManager
	{
		private static string UsbClassGuidString = "{88BAE032-5A81-49F0-BC3D-A4FF138216D6}";

		private static string UsbIfGuidString = "{82809DD0-51F5-11E1-B86C-0800200C9A66}";

		private static Guid UsbClassGuid = new Guid(UsbClassGuidString);

		private static Guid UsbIfGuid = new Guid(UsbIfGuidString);

		private OnDeviceConnect deviceConnectCallback;

		private OnDeviceDisconnect deviceDisconnectCallback;

		private ManagementEventWatcher connectWatcher;

		private ManagementEventWatcher disconnectWatcher;

		private Hashtable DevicesHash;

		public UsbConnectionManager(OnDeviceConnect deviceConnectCallback, OnDeviceDisconnect deviceDisconnectCallback)
		{
			if (deviceConnectCallback == null)
			{
				throw new ArgumentNullException("deviceConnectCallback");
			}
			if (deviceDisconnectCallback == null)
			{
				throw new ArgumentNullException("deviceDisconnectCallback");
			}
			this.deviceConnectCallback = deviceConnectCallback;
			this.deviceDisconnectCallback = deviceDisconnectCallback;
			DevicesHash = new Hashtable();
		}

		public void Start()
		{
			DiscoverUsbDevices();
			StartUsbDeviceNotificationWatcher();
		}

		public void Stop()
		{
			StopUsbDeviceNotificationWatcher();
		}

		private void DiscoverUsbDevices()
		{
			foreach (ManagementBaseObject item in new ManagementObjectSearcher(string.Format("SELECT PnPDeviceId FROM Win32_PnPEntity where ClassGuid ='{0}'", UsbClassGuid.ToString("B"))).Get())
			{
				string text = item["PnPDeviceId"] as string;
				if (text != null)
				{
					OnUsbDeviceConnect(text);
				}
			}
		}

		private void StartUsbDeviceNotificationWatcher()
		{
			WqlEventQuery query = new WqlEventQuery(string.Format("SELECT * FROM __InstanceCreationEvent WITHIN 0.1 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.ClassGuid = '{0}'", UsbClassGuid.ToString("B")));
			connectWatcher = new ManagementEventWatcher(query);
			connectWatcher.EventArrived += delegate(object sender, EventArrivedEventArgs args)
			{
				ManagementBaseObject managementBaseObject2 = args.NewEvent["TargetInstance"] as ManagementBaseObject;
				if (managementBaseObject2 != null)
				{
					string pnpId2 = managementBaseObject2.GetPropertyValue("PnPDeviceID").ToString();
					try
					{
						OnUsbDeviceConnect(pnpId2);
					}
					catch (Exception)
					{
					}
				}
			};
			connectWatcher.Start();
			WqlEventQuery query2 = new WqlEventQuery(string.Format("SELECT * FROM __InstanceDeletionEvent WITHIN 0.1 WHERE TargetInstance ISA 'Win32_PnPEntity' AND TargetInstance.ClassGuid = '{0}'", UsbClassGuid.ToString("B")));
			disconnectWatcher = new ManagementEventWatcher(query2);
			disconnectWatcher.EventArrived += delegate(object sender, EventArrivedEventArgs args)
			{
				ManagementBaseObject managementBaseObject = args.NewEvent["TargetInstance"] as ManagementBaseObject;
				if (managementBaseObject != null)
				{
					string pnpId = managementBaseObject.GetPropertyValue("PnPDeviceID").ToString();
					try
					{
						OnUsbDeviceDisconnect(pnpId, true);
					}
					catch (Exception)
					{
					}
				}
			};
			disconnectWatcher.Start();
		}

		private void StopUsbDeviceNotificationWatcher()
		{
			if (connectWatcher != null)
			{
				connectWatcher.Stop();
				connectWatcher.Dispose();
				connectWatcher = null;
			}
			if (disconnectWatcher != null)
			{
				disconnectWatcher.Stop();
				disconnectWatcher.Dispose();
				disconnectWatcher = null;
			}
		}

		private void OnUsbDeviceConnect(string pnpId)
		{
			try
			{
				DevicesHash.Add(pnpId, GetUsbDevicePath(pnpId));
			}
			catch
			{
			}
			deviceConnectCallback(GetUsbDevicePath(pnpId));
		}

		private void OnUsbDeviceDisconnect(string pnpId, bool fCallCallback)
		{
			deviceDisconnectCallback((string)DevicesHash[pnpId]);
			DevicesHash.Remove(pnpId);
		}

		private unsafe string GetUsbDevicePath(string pnpId)
		{
			IntPtr intPtr = NativeMethods.SetupDiGetClassDevs(ref UsbIfGuid, pnpId, 0, 18);
			if (IntPtr.Zero == intPtr)
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			int memberIndex = 0;
			int requiredSize = 0;
			DeviceInterfaceData deviceInterfaceData = default(DeviceInterfaceData);
			deviceInterfaceData.Size = Marshal.SizeOf(typeof(DeviceInterfaceData));
			DeviceInterfaceData deviceInterfaceData2 = deviceInterfaceData;
			if (!NativeMethods.SetupDiEnumDeviceInterfaces(intPtr, 0, ref UsbIfGuid, memberIndex, ref deviceInterfaceData2))
			{
				Marshal.GetLastWin32Error();
			}
			if (!NativeMethods.SetupDiGetDeviceInterfaceDetail(intPtr, ref deviceInterfaceData2, IntPtr.Zero, 0, ref requiredSize, IntPtr.Zero))
			{
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error != 122)
				{
					throw new Win32Exception(lastWin32Error);
				}
			}
			DeviceInterfaceDetailData* ptr = (DeviceInterfaceDetailData*)(void*)Marshal.AllocHGlobal(requiredSize);
			if (IntPtr.Size == 4)
			{
				ptr->Size = 6;
			}
			else
			{
				ptr->Size = 8;
			}
			DeviceInformationData deviceInformationData = default(DeviceInformationData);
			deviceInformationData.Size = Marshal.SizeOf(typeof(DeviceInformationData));
			DeviceInformationData deviceInfoData = deviceInformationData;
			if (!NativeMethods.SetupDiGetDeviceInterfaceDetail(intPtr, ref deviceInterfaceData2, ptr, requiredSize, ref requiredSize, ref deviceInfoData))
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
			string result = Marshal.PtrToStringAuto(new IntPtr(&ptr->DevicePath));
			Marshal.FreeHGlobal((IntPtr)ptr);
			return result;
		}
	}
}
