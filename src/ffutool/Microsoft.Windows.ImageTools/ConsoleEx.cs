using System;
using System.Collections.Generic;
using FFUComponents;

namespace Microsoft.Windows.ImageTools
{
	internal class ConsoleEx
	{
		private static ConsoleEx instance;

		private static object syncRoot = new object();

		private Dictionary<Guid, Tuple<int, ProgressReporter>> deviceRows;

		private int cursorTop;

		private int lastcursorTop;

		private bool error;

		private bool legacy;

		private int lastRow;

		private readonly int RESEVERED_LINES = 30;

		public static ConsoleEx Instance
		{
			get
			{
				if (instance == null)
				{
					lock (syncRoot)
					{
						if (instance == null)
						{
							instance = new ConsoleEx();
						}
					}
				}
				return instance;
			}
		}

		public void Initialize(ICollection<IFFUDevice> devices)
		{
			legacy = devices.Count == 1;
			if (!legacy)
			{
				int num = devices.Count * 7 + 100;
				if (Console.BufferHeight < num)
				{
					Console.SetBufferSize(Console.BufferWidth, num);
				}
			}
			deviceRows = new Dictionary<Guid, Tuple<int, ProgressReporter>>();
			int num2 = 0;
			foreach (IFFUDevice device in devices)
			{
				Console.WriteLine(Resources.DEVICE_NO, num2);
				Console.WriteLine(Resources.NAME, device.DeviceFriendlyName);
				Console.WriteLine(Resources.ID, device.DeviceUniqueID);
				Console.WriteLine(Resources.DEVICE_TYPE, device.DeviceType);
				deviceRows[device.DeviceUniqueID] = new Tuple<int, ProgressReporter>(num2, new ProgressReporter());
				if (!legacy)
				{
					Console.WriteLine();
					Console.WriteLine();
					Console.WriteLine();
				}
				num2++;
			}
			if (legacy)
			{
				lastcursorTop = 0;
				cursorTop = lastcursorTop;
				lastRow = GetDeviceCursorPosition(0, DeviceStatusPosition.DeviceStatus);
			}
			else
			{
				for (num2 = 0; num2 < RESEVERED_LINES; num2++)
				{
					Console.WriteLine();
				}
				lastcursorTop = Console.CursorTop - RESEVERED_LINES;
				cursorTop = lastcursorTop - devices.Count * 7;
			}
			foreach (IFFUDevice device2 in devices)
			{
				UpdateStatus(device2, DeviceStatus.CONNECTED, null);
			}
			error = false;
		}

		public void UpdateProgress(ProgressEventArgs progress)
		{
			Tuple<int, ProgressReporter> tuple = deviceRows[progress.Device.DeviceUniqueID];
			string text = tuple.Item2.CreateProgressDisplay(progress.Position, progress.Length);
			lock (syncRoot)
			{
				WriteLine(text, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceProgress));
			}
		}

		public void UpdateStatus(IFFUDevice device, DeviceStatus status, object data)
		{
			Tuple<int, ProgressReporter> tuple = deviceRows[device.DeviceUniqueID];
			lock (syncRoot)
			{
				switch (status)
				{
				case DeviceStatus.CONNECTED:
					WriteLine(Resources.STATUS_CONNECTED, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceStatus));
					break;
				case DeviceStatus.FLASHING:
					WriteLine(Resources.STATUS_FLASHING, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceStatus));
					break;
				case DeviceStatus.TRANSFER_WIM:
					WriteLine(Resources.STATUS_TRANSFER_WIM, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceStatus));
					break;
				case DeviceStatus.BOOTING_WIM:
					WriteLine(Resources.STATUS_BOOTING_TO_WIM, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceStatus));
					break;
				case DeviceStatus.DONE:
					WriteLine(Resources.STATUS_DONE, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceStatus));
					if (legacy)
					{
						Console.WriteLine();
					}
					break;
				case DeviceStatus.ERROR:
				case DeviceStatus.MESSAGE:
					WriteLine(data as string, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceStatus));
					break;
				case DeviceStatus.EXCEPTION:
				{
					Exception ex = (Exception)data;
					WriteLine(Resources.STATUS_ERROR, GetDeviceCursorPosition(tuple.Item1, DeviceStatusPosition.DeviceStatus));
					if (legacy)
					{
						Console.WriteLine(ex.Message);
						break;
					}
					if (!error)
					{
						Console.SetCursorPosition(0, lastcursorTop);
						Console.WriteLine(Resources.ERRORS);
						lastcursorTop = Console.CursorTop;
						error = true;
					}
					Console.SetCursorPosition(0, lastcursorTop);
					Console.WriteLine(Resources.DEVICE_NO, tuple.Item1);
					Console.WriteLine(ex.Message);
					Console.WriteLine();
					lastcursorTop = Console.CursorTop;
					break;
				}
				default:
					throw new Exception(Resources.ERROR_UNEXPECTED_DEVICESTATUS);
				}
				if (!legacy)
				{
					Console.SetCursorPosition(0, lastcursorTop);
				}
			}
		}

		private int GetDeviceCursorPosition(int index, DeviceStatusPosition position)
		{
			return (int)(cursorTop + 7 * index + position);
		}

		private void WriteLine(string text, int row)
		{
			if (legacy)
			{
				if (row == lastRow)
				{
					string text2 = new string(' ', Console.WindowWidth - 1);
					Console.Write("\r" + text2);
					Console.Write("\r" + text);
				}
				else
				{
					Console.WriteLine();
					Console.Write(text);
				}
				lastRow = row;
			}
			else
			{
				string value = new string(' ', Console.WindowWidth);
				Console.SetCursorPosition(0, row);
				Console.WriteLine(value);
				Console.SetCursorPosition(0, row);
				Console.WriteLine(text);
			}
		}
	}
}
