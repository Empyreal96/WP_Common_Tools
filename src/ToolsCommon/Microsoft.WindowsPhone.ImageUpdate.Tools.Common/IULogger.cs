using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public class IULogger : IDeploymentLogger
	{
		private LoggingLevel MinLogLevel;

		private Dictionary<LoggingLevel, string> LoggingMessage = new Dictionary<LoggingLevel, string>();

		private Dictionary<LoggingLevel, LogString> LoggingFunctions = new Dictionary<LoggingLevel, LogString>();

		private Dictionary<LoggingLevel, ConsoleColor> LoggingColors = new Dictionary<LoggingLevel, ConsoleColor>();

		private ConsoleColor _overrideColor;

		public ConsoleColor OverrideColor
		{
			get
			{
				return _overrideColor;
			}
			set
			{
				_overrideColor = value;
			}
		}

		public bool UseOverrideColor => _overrideColor != ConsoleColor.Black;

		public LogString ErrorLogger
		{
			get
			{
				return LoggingFunctions[LoggingLevel.Error];
			}
			set
			{
				SetLogFunction(LoggingLevel.Error, value);
			}
		}

		public LogString WarningLogger
		{
			get
			{
				return LoggingFunctions[LoggingLevel.Warning];
			}
			set
			{
				SetLogFunction(LoggingLevel.Warning, value);
			}
		}

		public LogString InformationLogger
		{
			get
			{
				return LoggingFunctions[LoggingLevel.Info];
			}
			set
			{
				SetLogFunction(LoggingLevel.Info, value);
			}
		}

		public LogString DebugLogger
		{
			get
			{
				return LoggingFunctions[LoggingLevel.Debug];
			}
			set
			{
				SetLogFunction(LoggingLevel.Debug, value);
			}
		}

		public IULogger()
		{
			MinLogLevel = LoggingLevel.Debug;
			LoggingMessage.Add(LoggingLevel.Debug, "DEBUG");
			LoggingMessage.Add(LoggingLevel.Info, "INFO");
			LoggingMessage.Add(LoggingLevel.Warning, "WARNING");
			LoggingMessage.Add(LoggingLevel.Error, "ERROR");
			LoggingFunctions.Add(LoggingLevel.Debug, LogToConsole);
			LoggingFunctions.Add(LoggingLevel.Info, LogToConsole);
			LoggingFunctions.Add(LoggingLevel.Warning, LogToError);
			LoggingFunctions.Add(LoggingLevel.Error, LogToError);
			LoggingColors.Add(LoggingLevel.Debug, ConsoleColor.DarkGray);
			LoggingColors.Add(LoggingLevel.Info, ConsoleColor.Gray);
			LoggingColors.Add(LoggingLevel.Warning, ConsoleColor.Yellow);
			LoggingColors.Add(LoggingLevel.Error, ConsoleColor.Red);
		}

		public static void LogToConsole(string format, params object[] list)
		{
			if (list.Length != 0)
			{
				Console.WriteLine(string.Format(CultureInfo.CurrentCulture, format, list));
			}
			else
			{
				Console.WriteLine(format);
			}
		}

		public static void LogToError(string format, params object[] list)
		{
			if (list.Length != 0)
			{
				Console.Error.WriteLine(string.Format(CultureInfo.CurrentCulture, format, list));
			}
			else
			{
				Console.Error.WriteLine(format);
			}
		}

		public static void LogToNull(string format, params object[] list)
		{
		}

		public void SetLoggingLevel(LoggingLevel level)
		{
			MinLogLevel = level;
		}

		public void SetLogFunction(LoggingLevel level, LogString logFunc)
		{
			if (logFunc == null)
			{
				LoggingFunctions[level] = LogToNull;
			}
			else
			{
				LoggingFunctions[level] = logFunc;
			}
		}

		public void ResetOverrideColor()
		{
			_overrideColor = ConsoleColor.Black;
		}

		public void Log(LoggingLevel level, string format, params object[] list)
		{
			if (level >= MinLogLevel)
			{
				ConsoleColor foregroundColor = Console.ForegroundColor;
				Console.ForegroundColor = (UseOverrideColor ? _overrideColor : LoggingColors[level]);
				LoggingFunctions[level](format, list);
				Console.ForegroundColor = foregroundColor;
			}
		}

		public void LogException(Exception exp)
		{
			LogException(exp, LoggingLevel.Error);
		}

		public void LogException(Exception exp, LoggingLevel level)
		{
			StringBuilder stringBuilder = new StringBuilder();
			StackTrace stackTrace = new StackTrace(exp, true);
			if (stackTrace.FrameCount > 0)
			{
				StackFrame stackFrame = null;
				stackFrame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
				if (stackFrame != null)
				{
					string arg = $"{stackFrame.GetFileName()}({stackFrame.GetFileLineNumber()},{stackFrame.GetFileColumnNumber()}):";
					stringBuilder.Append($"{arg}{Environment.NewLine}");
				}
			}
			stringBuilder.Append(string.Format("{0}: {1}{2}", LoggingMessage[level], "0x" + Marshal.GetHRForException(exp).ToString("X"), Environment.NewLine));
			stringBuilder.Append($"{Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().ProcessName)}:{Environment.NewLine}");
			stringBuilder.Append($"EXCEPTION: {exp.Message}{Environment.NewLine}");
			stringBuilder.Append(string.Format("STACKTRACE:{1}{0}{1}", exp.StackTrace, Environment.NewLine));
			Log(level, stringBuilder.ToString());
		}

		public void LogError(string format, params object[] list)
		{
			Log(LoggingLevel.Error, format, list);
		}

		public void LogWarning(string format, params object[] list)
		{
			Log(LoggingLevel.Warning, format, list);
		}

		public void LogInfo(string format, params object[] list)
		{
			
			Log(LoggingLevel.Info, format, list);
		}

		public void LogDebug(string format, params object[] list)
		{
			Log(LoggingLevel.Debug, format, list);
		}
	}
}
