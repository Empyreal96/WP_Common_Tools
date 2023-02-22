using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Diagnostics.Telemetry;
using System.Diagnostics.Tracing;

namespace FFUComponents
{
	public sealed class FlashingTelemetryLogger
	{
		private static FlashingTelemetryLogger instance;

		public const string ErrorFlashingTimeTooShort = "Flashing took less than 1 second, which is impossible.";

		private const string generalEventName = "Flashing";

		private TelemetryEventSource logger;

		private readonly EventSourceOptions telemetryOptionMeasures = new EventSourceOptions
		{
			Keywords = (EventKeywords)140737488355328L
		};

		private const string fileLocationEventName = "FFUFileLocation";

		private const string fileLocationNotReliableWarning = "The location was inferred based on best guess and can be inaccurate.";

		private const int ffuReadSpeedTestLenghMB = 50;

		public static FlashingTelemetryLogger Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new FlashingTelemetryLogger();
				}
				return instance;
			}
		}

		private FlashingTelemetryLogger()
		{
			logger = new TelemetryEventSource("Microsoft-Windows-Deployment-Flashing");
		}

		public void LogFlashingInitialized(Guid sessionId, IFFUDevice device, bool optimizeHint, string ffuFile)
		{
			try
			{
				LogString("FlashingInitialized", sessionId, optimizeHint.ToString());
				LogString("DeviceInfo", sessionId, device.GetType().Name, device.DeviceFriendlyName);
				string location = GetType().Assembly.Location;
				FileInfo fileInfo = new FileInfo(location);
				LogString("DllInfo", sessionId, location, FileVersionInfo.GetVersionInfo(location).ProductVersion, fileInfo.CreationTime.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture), fileInfo.LastWriteTime.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture));
				string location2 = Assembly.GetEntryAssembly().Location;
				FileInfo fileInfo2 = new FileInfo(location2);
				string fileName = Path.GetFileName(location2);
				LogString("ExeInfo", sessionId, fileName, location2, FileVersionInfo.GetVersionInfo(location2).ProductVersion, fileInfo2.CreationTime.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture), fileInfo2.LastWriteTime.ToString("yyMMdd-HHmm", CultureInfo.InvariantCulture));
				LogFFUReadSpeed(sessionId, ffuFile);
			}
			catch
			{
			}
		}

		public void LogFlashingStarted(Guid sessionId)
		{
			LogString("FlashingStarted", sessionId);
		}

		public void LogFlashingEnded(Guid sessionId, Stopwatch flashingStopwatch, string ffuFilePath, IFFUDevice device)
		{
			if ((float)flashingStopwatch.ElapsedMilliseconds == 0f)
			{
				LogFlashingFailed(sessionId, "Flashing took less than 1 second, which is impossible.");
				return;
			}
			float num = (float)new FileInfo(ffuFilePath).Length / 1024f / 1024f;
			float flashingSpeed = num / ((float)flashingStopwatch.ElapsedMilliseconds / 1000f);
			LogFlashingCompleted(sessionId, flashingStopwatch.Elapsed.TotalSeconds, flashingSpeed, device);
			LogString("FFUInfo", sessionId, ffuFilePath, num.ToString());
			LogFFULocation(sessionId, ffuFilePath);
		}

		public void LogFlashingException(Guid sessionId, Exception e)
		{
			LogFlashingFailed(sessionId, e.Message);
			if (e.InnerException != null)
			{
				LogString("FlashingException", sessionId, e.InnerException.ToString());
			}
		}

		public void LogThorDeviceUSBConnectionType(Guid sessionId, ConnectionType connectionType)
		{
			LogString("ThorDeviceUSBConnectionType", sessionId, connectionType.ToString());
		}

		private void LogString(string _eventName, Guid sessionId, params string[] values)
		{
			switch (values.Length)
			{
			case 0:
				logger.Write("Flashing", telemetryOptionMeasures, new
				{
					EventName = _eventName,
					Value1 = sessionId
				});
				return;
			case 1:
				logger.Write("Flashing", telemetryOptionMeasures, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0]
				});
				return;
			case 2:
				logger.Write("Flashing", telemetryOptionMeasures, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0],
					Value3 = values[1]
				});
				return;
			case 3:
				logger.Write("Flashing", telemetryOptionMeasures, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0],
					Value3 = values[1],
					Value4 = values[2]
				});
				return;
			case 4:
				logger.Write("Flashing", telemetryOptionMeasures, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0],
					Value3 = values[1],
					Value4 = values[2],
					Value5 = values[3]
				});
				return;
			case 5:
				logger.Write("Flashing", telemetryOptionMeasures, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0],
					Value3 = values[1],
					Value4 = values[2],
					Value5 = values[3],
					Value6 = values[4]
				});
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string value in values)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(";");
			}
			logger.Write("Flashing", telemetryOptionMeasures, new
			{
				EventName = _eventName,
				Value1 = sessionId,
				Value2 = "Values count exceeded supported count.",
				Value3 = stringBuilder.ToString()
			});
		}

		private void LogFFULocation(Guid sessionId, string ffuFilePath)
		{
			try
			{
				Uri uri = new Uri(ffuFilePath);
				if (uri.IsUnc)
				{
					if (uri.IsLoopback)
					{
						DriveType localPathDriveType = GetLocalPathDriveType(uri.LocalPath);
						if (localPathDriveType == DriveType.Unknown)
						{
							LogFileLocation(sessionId, ffuFilePath, DriveType.Network, "The location was inferred based on best guess and can be inaccurate.");
						}
						else
						{
							LogFileLocation(sessionId, ffuFilePath, localPathDriveType, null);
						}
					}
					else
					{
						LogFileLocation(sessionId, ffuFilePath, DriveType.Network, "The location was inferred based on best guess and can be inaccurate.");
					}
				}
				else
				{
					LogFileLocation(sessionId, ffuFilePath, GetLocalPathDriveType(ffuFilePath), null);
				}
			}
			catch (Exception ex)
			{
				LogString("GetFFUFileLocationFailed", sessionId, ffuFilePath, ex.Message);
			}
		}

		private void LogFlashingCompleted(Guid sessionId, double elapsedTimeSeconds, float flashingSpeed, IFFUDevice device)
		{
			LogString("FlashingCompleted", sessionId, elapsedTimeSeconds.ToString(), flashingSpeed.ToString(), device.GetType().Name);
		}

		private void LogFlashingFailed(Guid sessionId, string message)
		{
			LogString("FlashingFailed", sessionId, message);
		}

		private void LogFileLocation(Guid sessionId, string ffuFilePath, DriveType ffuDriveType, string warningMessage)
		{
			LogString("FFUFileLocation", sessionId, ffuFilePath, ffuDriveType.ToString(), warningMessage);
		}

		private DriveType GetLocalPathDriveType(string filePath)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			foreach (DriveInfo driveInfo in drives)
			{
				if (string.Equals(Path.GetPathRoot(filePath), driveInfo.Name, StringComparison.OrdinalIgnoreCase))
				{
					return driveInfo.DriveType;
				}
			}
			return DriveType.Unknown;
		}

		private void LogFFUReadSpeed(Guid sessionId, string ffuFilePath)
		{
			try
			{
				using (BinaryReader binaryReader = new BinaryReader(new FileStream(ffuFilePath, FileMode.Open, FileAccess.Read)))
				{
					int num = 52428800;
					byte[] buffer = new byte[num];
					Stopwatch stopwatch = new Stopwatch();
					stopwatch.Start();
					binaryReader.Read(buffer, 0, num);
					stopwatch.Stop();
					float num2 = 50000f / (float)stopwatch.ElapsedMilliseconds;
					LogString("FFUReadSpeed", sessionId, num2.ToString(), stopwatch.ElapsedMilliseconds.ToString(), Stopwatch.IsHighResolution.ToString());
				}
			}
			catch (Exception ex)
			{
				LogString("GetFFUReadSpeedFailed", sessionId, ffuFilePath, ex.Message);
			}
		}
	}
}
