using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Windows.Flashing.Platform;

namespace FFUComponents
{
	public static class FFUManager
	{
		public static Microsoft.Windows.Flashing.Platform.FlashingPlatform flashingPlatform;

		public static NotificationCallback deviceNotification;

		private static object ffuManagerLock;

		private static IList<IFFUDeviceInternal> activeFFUDevices;

		private static DisconnectTimer disconnectTimer;

		private static bool isStarted;

		public static readonly Guid SimpleIOGuid;

		public static readonly Guid WinUSBClassGuid;

		public static readonly Guid WinUSBFlashingIfGuid;

		private static readonly string[] ThorDevicePids;

		internal static FlashingHostLogger HostLogger { get; private set; }

		internal static FlashingDeviceLogger DeviceLogger { get; private set; }

		public static ICollection<IFFUDevice> FlashableDevices
		{
			get
			{
				ICollection<IFFUDevice> devices = new List<IFFUDevice>();
				GetFlashableDevices(ref devices);
				return devices;
			}
		}

		public static event EventHandler<ConnectEventArgs> DeviceConnectEvent;

		public static event EventHandler<DisconnectEventArgs> DeviceDisconnectEvent;

		internal static void DisconnectDevice(Guid id)
		{
			List<IFFUDeviceInternal> list = new List<IFFUDeviceInternal>(activeFFUDevices.Count);
			lock (activeFFUDevices)
			{
				for (int i = 0; i < activeFFUDevices.Count; i++)
				{
					if (activeFFUDevices[i].DeviceUniqueID == id)
					{
						list.Add(activeFFUDevices[i]);
						activeFFUDevices.RemoveAt(i);
					}
				}
			}
			foreach (IFFUDeviceInternal item in list)
			{
				disconnectTimer.StopTimer(item);
				OnDisconnect(item);
			}
		}

		internal static void DisconnectDevice(SimpleIODevice deviceToRemove)
		{
			IFFUDeviceInternal iFFUDeviceInternal = null;
			lock (activeFFUDevices)
			{
				if (activeFFUDevices.Remove(deviceToRemove))
				{
					iFFUDeviceInternal = deviceToRemove;
				}
			}
			if (iFFUDeviceInternal != null)
			{
				disconnectTimer.StopTimer(iFFUDeviceInternal);
				OnDisconnect(iFFUDeviceInternal);
			}
		}

		internal static void DisconnectDevice(ThorDevice deviceToRemove)
		{
			ThorDevice thorDevice = null;
			lock (activeFFUDevices)
			{
				if (activeFFUDevices.Remove(deviceToRemove))
				{
					thorDevice = deviceToRemove;
				}
			}
			if (thorDevice != null)
			{
				OnDisconnect(thorDevice);
			}
		}

		private static bool DevicePresent(Guid id)
		{
			bool result = false;
			lock (activeFFUDevices)
			{
				for (int i = 0; i < activeFFUDevices.Count; i++)
				{
					if (activeFFUDevices[i].DeviceUniqueID == id)
					{
						return true;
					}
				}
				return result;
			}
		}

		private static void StartTimerIfNecessary(IFFUDeviceInternal device)
		{
			if (device.NeedsTimer())
			{
				disconnectTimer?.StartTimer(device);
			}
		}

		private static void OnConnect(IFFUDeviceInternal device)
		{
			if (device != null)
			{
				if (FFUManager.DeviceConnectEvent != null)
				{
					FFUManager.DeviceConnectEvent(null, new ConnectEventArgs(device));
				}
				HostLogger.EventWriteDevice_Attach(device.DeviceUniqueID, device.DeviceFriendlyName);
			}
		}

		private static void OnDisconnect(IFFUDeviceInternal device)
		{
			if (device != null && !DevicePresent(device.DeviceUniqueID))
			{
				if (FFUManager.DeviceDisconnectEvent != null)
				{
					FFUManager.DeviceDisconnectEvent(null, new DisconnectEventArgs(device.DeviceUniqueID));
				}
				HostLogger.EventWriteDevice_Remove(device.DeviceUniqueID, device.DeviceFriendlyName);
			}
		}

		internal static void OnDeviceConnect(string usbDevicePath)
		{
			lock (activeFFUDevices)
			{
				IFFUDeviceInternal iFFUDeviceInternal = null;
				IFFUDeviceInternal device2 = null;
				IFFUDeviceInternal iFFUDeviceInternal2 = null;
				string[] thorDevicePids = ThorDevicePids;
				foreach (string value in thorDevicePids)
				{
					if (usbDevicePath.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0 && flashingPlatform.CreateConnectedDevice(usbDevicePath).CanFlash())
					{
						FlashingDevice val = null;
						try
						{
							val = flashingPlatform.CreateFlashingDevice(usbDevicePath);
						}
						catch (Exception)
						{
						}
						if (val != null)
						{
							iFFUDeviceInternal2 = new ThorDevice(val, usbDevicePath);
							activeFFUDevices.Add(iFFUDeviceInternal2);
							OnConnect(iFFUDeviceInternal2);
						}
					}
				}
				if (iFFUDeviceInternal2 != null)
				{
					return;
				}
				SimpleIODevice device = new SimpleIODevice(usbDevicePath);
				if (device.OnConnect(device))
				{
					iFFUDeviceInternal = activeFFUDevices.SingleOrDefault((IFFUDeviceInternal deviceInstance) => deviceInstance.DeviceUniqueID == device.DeviceUniqueID);
					IFFUDeviceInternal iFFUDeviceInternal3 = disconnectTimer.StopTimer(device);
					if (iFFUDeviceInternal == null && iFFUDeviceInternal3 != null)
					{
						activeFFUDevices.Add(iFFUDeviceInternal3);
						iFFUDeviceInternal = iFFUDeviceInternal3;
						iFFUDeviceInternal2 = iFFUDeviceInternal3;
					}
					if (iFFUDeviceInternal != null && !((SimpleIODevice)iFFUDeviceInternal).OnConnect(device))
					{
						activeFFUDevices.Remove(iFFUDeviceInternal);
						device2 = iFFUDeviceInternal;
						iFFUDeviceInternal = null;
					}
					if (iFFUDeviceInternal == null)
					{
						iFFUDeviceInternal2 = device;
						activeFFUDevices.Add(device);
					}
					OnDisconnect(device2);
					OnConnect(iFFUDeviceInternal2);
				}
			}
		}

		internal static void OnDeviceDisconnect(string usbDevicePath)
		{
			List<IFFUDeviceInternal> list = new List<IFFUDeviceInternal>();
			lock (activeFFUDevices)
			{
				IList<IFFUDeviceInternal> source = activeFFUDevices;
				Func<IFFUDeviceInternal, bool> func = default(Func<IFFUDeviceInternal, bool>);
				Func<IFFUDeviceInternal, bool> func2 = func;
				if (func2 == null)
				{
					func2 = (func = (IFFUDeviceInternal d) => d.UsbDevicePath.Equals(usbDevicePath, StringComparison.OrdinalIgnoreCase));
				}
				foreach (IFFUDeviceInternal item in source.Where(func2))
				{
					if (item != null)
					{
						list.Add(item);
					}
				}
				foreach (IFFUDeviceInternal item2 in list)
				{
					activeFFUDevices.Remove(item2);
					StartTimerIfNecessary(item2);
				}
			}
			foreach (IFFUDeviceInternal item3 in list)
			{
				OnDisconnect(item3);
			}
		}

		static FFUManager()
		{
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Expected O, but got Unknown
			isStarted = false;
			SimpleIOGuid = new Guid("{67EA0A90-FF06-417D-AB66-6676DCE879CD}");
			WinUSBClassGuid = new Guid("{88BAE032-5A81-49F0-BC3D-A4FF138216D6}");
			WinUSBFlashingIfGuid = new Guid("{82809DD0-51F5-11E1-B86C-0800200C9A66}");
			ThorDevicePids = new string[4] { "pid_0658", "pid_066e", "pid_0714", "pid_0a02" };
			ffuManagerLock = new object();
			activeFFUDevices = new List<IFFUDeviceInternal>();
			HostLogger = new FlashingHostLogger();
			DeviceLogger = new FlashingDeviceLogger();
			string text = null;
			if (Environment.GetEnvironmentVariable("FFUComponents_NoLog") == null)
			{
				text = GetUFPLogPath();
			}
			flashingPlatform = new Microsoft.Windows.Flashing.Platform.FlashingPlatform(text);
			deviceNotification = null;
		}

		public static void Start()
		{
			lock (ffuManagerLock)
			{
				if (!isStarted)
				{
					disconnectTimer = new DisconnectTimer();
					DeviceNotificationCallback val = null;
					NotificationCallback notificationCallback = new NotificationCallback();
					List<Guid> list = new List<Guid>();
					list.Add(SimpleIOGuid);
					list.Add(WinUSBClassGuid);
					list.Add(WinUSBFlashingIfGuid);
					list.Add(Microsoft.Windows.Flashing.Platform.FlashingPlatform.GuidDevinterfaceUfp);
					flashingPlatform.RegisterDeviceNotificationCallback(list.ToArray(), (string)null, (DeviceNotificationCallback)(object)notificationCallback, ref val);
					deviceNotification = notificationCallback;
					isStarted = true;
				}
			}
		}

		public static void Stop()
		{
			lock (ffuManagerLock)
			{
				if (isStarted)
				{
					DeviceNotificationCallback val = null;
					flashingPlatform.RegisterDeviceNotificationCallback((Guid[])null, (string)null, (DeviceNotificationCallback)null, ref val);
					deviceNotification = null;
					lock (activeFFUDevices)
					{
						activeFFUDevices.Clear();
					}
					Interlocked.Exchange(ref disconnectTimer, null).StopAllTimers();
					isStarted = false;
				}
			}
		}

		public static void GetFlashableDevices(ref ICollection<IFFUDevice> devices)
		{
			lock (ffuManagerLock)
			{
				if (!isStarted)
				{
					throw new FFUManagerException(Resources.ERROR_FFUMANAGER_NOT_STARTED);
				}
				devices.Clear();
				lock (activeFFUDevices)
				{
					foreach (IFFUDeviceInternal activeFFUDevice in activeFFUDevices)
					{
						devices.Add(activeFFUDevice);
					}
				}
			}
		}

		public static IFFUDevice GetFlashableDevice(string instancePath, bool enableFallback)
		{
			SimpleIODevice simpleIODevice = new SimpleIODevice(instancePath);
			SimpleIODevice simpleIODevice2 = simpleIODevice;
			if (simpleIODevice2.OnConnect(simpleIODevice2))
			{
				return simpleIODevice;
			}
			if (enableFallback)
			{
				string fallbackInstancePath = GetFallbackInstancePath(instancePath);
				if (!string.IsNullOrEmpty(fallbackInstancePath))
				{
					simpleIODevice = new SimpleIODevice(fallbackInstancePath);
					SimpleIODevice simpleIODevice3 = simpleIODevice;
					if (simpleIODevice3.OnConnect(simpleIODevice3))
					{
						return simpleIODevice;
					}
				}
			}
			return null;
		}

		public static string GetUFPLogPath()
		{
			string text = Process.GetCurrentProcess().ProcessName + Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
			return Path.Combine(Path.GetTempPath(), text + ".log");
		}

		private static string ReplaceUsbSerial(Match match)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = match.Groups["serial"].Value;
			if (Regex.IsMatch(value, "[a-f0-9]{32}", RegexOptions.IgnoreCase))
			{
				int num;
				for (num = 0; num < 8; num++)
				{
					stringBuilder.AppendFormat("{0}{1}", value.ElementAt(num + 1), value.ElementAt(num));
					num++;
				}
				stringBuilder.Append("-");
				for (int i = 0; i < 3; i++)
				{
					int num2;
					for (num2 = 0; num2 < 4; num2++)
					{
						stringBuilder.AppendFormat("{0}{1}", value.ElementAt(8 + 4 * i + num2 + 1), value.ElementAt(8 + 4 * i + num2));
						num2++;
					}
					stringBuilder.Append("-");
				}
				int num3;
				for (num3 = 0; num3 < 12; num3++)
				{
					stringBuilder.AppendFormat("{0}{1}", value.ElementAt(20 + num3 + 1), value.ElementAt(20 + num3));
					num3++;
				}
				return match.ToString().Replace(value, stringBuilder.ToString());
			}
			if (Regex.IsMatch(value, "[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}", RegexOptions.IgnoreCase))
			{
				for (int j = 0; j < value.Length - 1; j++)
				{
					if (value.ElementAt(j) != '-')
					{
						stringBuilder.AppendFormat("{0}{1}", value.ElementAt(j + 1), value.ElementAt(j));
						j++;
					}
				}
				return match.ToString().Replace(value, stringBuilder.ToString());
			}
			return null;
		}

		private static string GetFallbackInstancePath(string instancePath)
		{
			MatchEvaluator evaluator = ReplaceUsbSerial;
			return Regex.Replace(instancePath, "\\\\\\?\\\\usb#vid_[a-zA-Z0-9]{4}&pid_[a-zA-Z0-9]{4}#(?<serial>.+)#{[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}}\\z", evaluator, RegexOptions.IgnoreCase);
		}
	}
}
