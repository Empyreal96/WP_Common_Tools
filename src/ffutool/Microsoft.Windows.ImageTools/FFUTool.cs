using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FFUComponents;

namespace Microsoft.Windows.ImageTools
{
	public static class FFUTool
	{
		private static Regex flashParam = new Regex("[-/]flash$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex uefiFlashParam = new Regex("[-/]uefiflash$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex fastFlashParam = new Regex("[-/]fastflash$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex skipParam = new Regex("[-/]skip$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex listParam = new Regex("[-/]list$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex forceParam = new Regex("[-/]force$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex massParam = new Regex("[-/]massStorage$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex clearIdParam = new Regex("[-/]clearId$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex serParam = new Regex("[-/]serial$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex wimParam = new Regex("[-/]wim$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex setBootModeParam = new Regex("[-/]setBootMode$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex servicingLogsParam = new Regex("[-/]getServicingLogs", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex flashLogsParam = new Regex("[-/]getFlashingLogs", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		private static Regex noLogParam = new Regex("[-/]noLog", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static void Main(string[] args)
		{
			if (args.Length > 2 && forceParam.IsMatch(args[2]))
			{
				Console.WriteLine(Resources.FORCE_OPTION_DEPRECATED);
			}
			bool flag = false;
			string text = null;
			string wimPath = null;
			uint bootMode = 0u;
			string profileName = null;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			if (args.Length < 1 || (!FFUTool.flashParam.IsMatch(args[0]) && !uefiFlashParam.IsMatch(args[0]) && !fastFlashParam.IsMatch(args[0]) && !wimParam.IsMatch(args[0]) && !skipParam.IsMatch(args[0]) && !listParam.IsMatch(args[0]) && !massParam.IsMatch(args[0]) && !serParam.IsMatch(args[0]) && !clearIdParam.IsMatch(args[0]) && !setBootModeParam.IsMatch(args[0]) && !servicingLogsParam.IsMatch(args[0]) && !flashLogsParam.IsMatch(args[0])))
			{
				flag = true;
			}
			if (!flag && (FFUTool.flashParam.IsMatch(args[0]) || uefiFlashParam.IsMatch(args[0]) || fastFlashParam.IsMatch(args[0]) || wimParam.IsMatch(args[0])))
			{
				if (args.Length <= 1)
				{
					flag = true;
				}
				else
				{
					text = args[1];
					if (!File.Exists(text))
					{
						Console.WriteLine(Resources.ERROR_FILE_NOT_FOUND, text);
						Environment.ExitCode = -1;
						return;
					}
					if (FFUTool.flashParam.IsMatch(args[0]) && args.Length >= 3)
					{
						wimPath = args[2];
					}
				}
			}
			if (!flag && servicingLogsParam.IsMatch(args[0]))
			{
				if (args.Length <= 1)
				{
					flag = true;
				}
				else
				{
					text = args[1];
				}
			}
			if (!flag && flashLogsParam.IsMatch(args[0]))
			{
				if (args.Length <= 1)
				{
					flag = true;
				}
				else
				{
					text = args[1];
				}
			}
			if (!flag && setBootModeParam.IsMatch(args[0]))
			{
				if (args.Length <= 1)
				{
					flag = true;
				}
				else
				{
					try
					{
						bootMode = Convert.ToUInt32(args[1], CultureInfo.InvariantCulture);
						profileName = ((args.Length < 3) ? "" : args[2]);
					}
					catch (Exception)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				Console.WriteLine(Resources.USAGE);
				Environment.ExitCode = -1;
				return;
			}
			if (args.Any((string s) => noLogParam.IsMatch(s)))
			{
				flag2 = true;
				Environment.SetEnvironmentVariable("FFUComponents_NoLog", "Yes");
			}
			try
			{
				FFUManager.Start();
				ICollection<IFFUDevice> devices = new List<IFFUDevice>();
				FFUManager.GetFlashableDevices(ref devices);
				if (devices.Count == 0)
				{
					if (!flag2)
					{
						Console.WriteLine(Resources.LOGGING_UFP_TO_LOG, FFUManager.GetUFPLogPath());
					}
					Console.WriteLine(Resources.NO_CONNECTED_DEVICES);
					Environment.ExitCode = 0;
					return;
				}
				if (listParam.IsMatch(args[0]))
				{
					Console.WriteLine(Resources.DEVICES_FOUND, devices.Count);
					int num = 0;
					foreach (IFFUDevice item in devices)
					{
						Console.WriteLine(Resources.DEVICE_NO, num);
						Console.WriteLine(Resources.NAME, item.DeviceFriendlyName);
						Console.WriteLine(Resources.ID, item.DeviceUniqueID);
						Console.WriteLine(Resources.DEVICE_TYPE, item.DeviceType);
						Console.WriteLine();
						num++;
					}
					Environment.ExitCode = 0;
					return;
				}
				FlashParam[] array = new FlashParam[devices.Count];
				EtwSession session = new EtwSession(!flag2);
				try
				{
					Console.CancelKeyPress += delegate
					{
						session.Dispose();
					};
					if (!flag2)
					{
						foreach (IFFUDevice item2 in devices)
						{
							if (string.Compare(item2.DeviceType, "UFPDevice") == 0)
							{
								flag4 = true;
							}
							else if (string.Compare(item2.DeviceType, "SimpleIODevice") == 0)
							{
								flag3 = true;
							}
						}
						if (flag3)
						{
							Console.WriteLine(Resources.LOGGING_SIMPLEIO_TO_ETL, session.EtlPath);
						}
						if (flag4)
						{
							Console.WriteLine(Resources.LOGGING_UFP_TO_LOG, FFUManager.GetUFPLogPath());
						}
					}
					Console.WriteLine();
					ConsoleEx.Instance.Initialize(devices);
					int num2 = 0;
					try
					{
						foreach (IFFUDevice item3 in devices)
						{
							AutoResetEvent waitHandle = new AutoResetEvent(false);
							if (uefiFlashParam.IsMatch(args[0]))
							{
								int num3 = num2;
								FlashParam obj = new FlashParam
								{
									Device = item3,
									FfuFilePath = text,
									WaitHandle = waitHandle,
									FastFlash = false
								};
								FlashParam flashParam = obj;
								array[num3] = obj;
								FlashParam param11 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoFlash(param11);
								});
							}
							else if (fastFlashParam.IsMatch(args[0]))
							{
								int num4 = num2;
								FlashParam obj2 = new FlashParam
								{
									Device = item3,
									FfuFilePath = text,
									WaitHandle = waitHandle,
									FastFlash = true
								};
								FlashParam flashParam = obj2;
								array[num4] = obj2;
								FlashParam param10 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoFlash(param10);
								});
							}
							else if (FFUTool.flashParam.IsMatch(args[0]))
							{
								int num5 = num2;
								FlashParam obj3 = new FlashParam
								{
									Device = item3,
									FfuFilePath = text,
									WimPath = wimPath,
									WaitHandle = waitHandle,
									FastFlash = true
								};
								FlashParam flashParam = obj3;
								array[num5] = obj3;
								FlashParam param9 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoWimFlash(param9);
								});
							}
							else if (skipParam.IsMatch(args[0]))
							{
								int num6 = num2;
								FlashParam obj4 = new FlashParam
								{
									Device = item3,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj4;
								array[num6] = obj4;
								FlashParam param8 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoSkip(param8);
								});
							}
							else if (massParam.IsMatch(args[0]))
							{
								int num7 = num2;
								FlashParam obj5 = new FlashParam
								{
									Device = item3,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj5;
								array[num7] = obj5;
								FlashParam param7 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoMassStorage(param7);
								});
							}
							else if (clearIdParam.IsMatch(args[0]))
							{
								int num8 = num2;
								FlashParam obj6 = new FlashParam
								{
									Device = item3,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj6;
								array[num8] = obj6;
								FlashParam param6 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoClearId(param6);
								});
							}
							else if (serParam.IsMatch(args[0]))
							{
								int num9 = num2;
								FlashParam obj7 = new FlashParam
								{
									Device = item3,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj7;
								array[num9] = obj7;
								FlashParam param5 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoSerialNumber(param5);
								});
							}
							else if (wimParam.IsMatch(args[0]))
							{
								int num10 = num2;
								FlashParam obj8 = new FlashParam
								{
									Device = item3,
									FfuFilePath = text,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj8;
								array[num10] = obj8;
								FlashParam param4 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoWim(param4);
								});
							}
							else if (setBootModeParam.IsMatch(args[0]))
							{
								int num11 = num2;
								SetBootModeParam obj9 = new SetBootModeParam
								{
									Device = item3,
									BootMode = bootMode,
									ProfileName = profileName,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj9;
								array[num11] = obj9;
								FlashParam param3 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoSetBootMode(param3 as SetBootModeParam);
								});
							}
							else if (servicingLogsParam.IsMatch(args[0]))
							{
								int num12 = num2;
								FlashParam obj10 = new FlashParam
								{
									Device = item3,
									LogFolderPath = text,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj10;
								array[num12] = obj10;
								FlashParam param2 = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoServicingLogs(param2);
								});
							}
							else if (flashLogsParam.IsMatch(args[0]))
							{
								int num13 = num2;
								FlashParam obj11 = new FlashParam
								{
									Device = item3,
									LogFolderPath = text,
									WaitHandle = waitHandle
								};
								FlashParam flashParam = obj11;
								array[num13] = obj11;
								FlashParam param = flashParam;
								ThreadPool.QueueUserWorkItem(delegate
								{
									DoFlashLogs(param);
								});
							}
							num2++;
						}
						WaitHandle.WaitAll(((IEnumerable<FlashParam>)array).Select((Func<FlashParam, WaitHandle>)((FlashParam p) => p.WaitHandle)).ToArray());
						if (array.Any((FlashParam p) => p.Result == -1))
						{
							Console.WriteLine(Resources.ERROR_AT_LEAST_ONE_DEVICE_FAILED);
							Environment.ExitCode = -1;
						}
					}
					finally
					{
						Console.CancelKeyPress -= delegate
						{
							session.Dispose();
						};
					}
				}
				finally
				{
					if (session != null)
					{
						((IDisposable)session).Dispose();
					}
				}
			}
			catch (FFUException ex2)
			{
				Console.WriteLine();
				Console.WriteLine(Resources.ERROR_FFU + ex2.Message);
				Environment.ExitCode = -1;
			}
			catch (TimeoutException ex3)
			{
				Console.WriteLine();
				Console.WriteLine(Resources.ERROR_TIMED_OUT + ex3.Message);
				Environment.ExitCode = -1;
			}
			finally
			{
				FFUManager.Stop();
			}
		}

		private static void DoSerialNumber(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				byte[] array = param.Device.SerialNumber.ToByteArray();
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, string.Format(CultureInfo.InvariantCulture, Resources.SERIAL_NO_FORMAT, new object[2]
				{
					Resources.SERIAL_NO,
					BitConverter.ToString(array).Replace("-", string.Empty)
				}));
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoSkip(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, Resources.STATUS_SKIPPING);
				if (param.Device.SkipTransfer())
				{
					ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, Resources.STATUS_SKIPPED);
					return;
				}
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.ERROR, Resources.ERROR_SKIP_TRANSFER);
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoMassStorage(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				if (param.Device.EnterMassStorage())
				{
					ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, Resources.RESET_MASS_STORAGE_MODE);
					return;
				}
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.ERROR, Resources.ERROR_RESET_MASS_STORAGE_MODE);
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoClearId(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				Console.WriteLine(Resources.DEVICE_ID, param.Device.DeviceFriendlyName);
				if (param.Device.ClearIdOverride())
				{
					param.Result = 0;
					ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, string.Format(CultureInfo.CurrentUICulture, Resources.REMOVE_PLATFORM_ID, new object[1] { param.Device.DeviceFriendlyName }));
				}
				else
				{
					param.Result = -1;
					ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.ERROR, Resources.ERROR_NO_PLATFORM_ID);
				}
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoWim(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, string.Format(CultureInfo.CurrentUICulture, Resources.BOOTING_WIM, new object[1] { Path.GetFileName(param.FfuFilePath) }));
				Stopwatch stopwatch = Stopwatch.StartNew();
				param.Device.EndTransfer();
				bool num = param.Device.WriteWim(param.FfuFilePath);
				stopwatch.Stop();
				if (num)
				{
					ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, string.Format(CultureInfo.CurrentUICulture, Resources.WIM_TRANSFER_RATE, new object[1] { stopwatch.Elapsed.TotalSeconds }));
				}
				else
				{
					ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, Resources.ERROR_BOOT_WIM);
				}
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoServicingLogs(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, Resources.STATUS_LOGS);
				string servicingLogs = param.Device.GetServicingLogs(param.LogFolderPath);
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, string.Format(CultureInfo.CurrentUICulture, Resources.LOGS_PATH, new object[1] { servicingLogs }));
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoFlashLogs(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, Resources.STATUS_LOGS);
				string flashingLogs = param.Device.GetFlashingLogs(param.LogFolderPath);
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, string.Format(CultureInfo.CurrentUICulture, Resources.LOGS_PATH, new object[1] { flashingLogs }));
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void PrepareFlash(IFFUDevice device)
		{
			device.EndTransfer();
		}

		private static void TransferWimIfPresent(ref IFFUDevice device, string ffuFilePath, string wimFilePath)
		{
			IFFUDevice wimDevice = null;
			Guid id = device.DeviceUniqueID;
			ManualResetEvent deviceConnected = new ManualResetEvent(false);
			EventHandler<ConnectEventArgs> value = delegate(object sender, ConnectEventArgs e)
			{
				if (e.Device.DeviceUniqueID == id)
				{
					wimDevice = e.Device;
					deviceConnected.Set();
				}
			};
			if (string.IsNullOrEmpty(wimFilePath))
			{
				wimFilePath = Path.Combine(Path.GetDirectoryName(ffuFilePath), "flashwim.wim");
			}
			if (!File.Exists(wimFilePath))
			{
				return;
			}
			FFUManager.DeviceConnectEvent += value;
			ConsoleEx.Instance.UpdateStatus(device, DeviceStatus.TRANSFER_WIM, wimFilePath);
			bool flag = false;
			try
			{
				flag = device.WriteWim(wimFilePath);
			}
			catch (FFUException)
			{
			}
			if (flag)
			{
				ConsoleEx.Instance.UpdateStatus(device, DeviceStatus.BOOTING_WIM, wimFilePath);
				bool num = deviceConnected.WaitOne(TimeSpan.FromSeconds(30.0));
				FFUManager.DeviceConnectEvent -= value;
				if (!num)
				{
					throw new FFUException(device.DeviceFriendlyName, device.DeviceUniqueID, Resources.ERROR_WIM_BOOT);
				}
				device = wimDevice;
			}
		}

		private static void FlashFile(IFFUDevice device, string ffuFilePath, bool optimize)
		{
			ConsoleEx.Instance.UpdateStatus(device, DeviceStatus.FLASHING, null);
			device.ProgressEvent += Device_ProgressEvent;
			device.EndTransfer();
			device.FlashFFUFile(ffuFilePath, optimize);
		}

		private static void DoWimFlash(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				PrepareFlash(param.Device);
				TransferWimIfPresent(ref param.Device, param.FfuFilePath, param.WimPath);
				FlashFile(param.Device, param.FfuFilePath, param.FastFlash);
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.DONE, null);
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoFlash(FlashParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				PrepareFlash(param.Device);
				FlashFile(param.Device, param.FfuFilePath, param.FastFlash);
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.DONE, null);
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void DoSetBootMode(SetBootModeParam param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param");
			}
			try
			{
				uint num = param.Device.SetBootMode(param.BootMode, param.ProfileName);
				if (num == 0)
				{
					ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.MESSAGE, Resources.RESET_BOOT_MODE);
					return;
				}
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.ERROR, string.Format(CultureInfo.CurrentUICulture, Resources.ERROR_RESET_BOOT_MODE, new object[1] { num }));
			}
			catch (Exception data)
			{
				param.Result = -1;
				ConsoleEx.Instance.UpdateStatus(param.Device, DeviceStatus.EXCEPTION, data);
			}
			finally
			{
				param.WaitHandle.Set();
			}
		}

		private static void Device_ProgressEvent(object sender, ProgressEventArgs e)
		{
			ConsoleEx.Instance.UpdateProgress(e);
		}
	}
}
