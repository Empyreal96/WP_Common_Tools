#define TRACE
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class Logger
	{
		private static readonly object SyncRoot = new object();

		private static StreamWriter fileWriter;

		private static TraceLevel consoleLevel = TraceLevel.Off;

		private static TraceLevel fileLevel = TraceLevel.Off;

		public static TextWriter Writer => fileWriter;

		public static event EventHandler<LogEventArgs> OnLogMessage;

		public static void Configure(TraceLevel consoleLogLevel, TraceLevel fileLogLevel, string logFile, bool append)
		{
			lock (SyncRoot)
			{
				Close();
				consoleLevel = consoleLogLevel;
				fileLevel = fileLogLevel;
				if (!string.IsNullOrEmpty(logFile))
				{
					string directoryName = Path.GetDirectoryName(logFile);
					if (!string.IsNullOrEmpty(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					StreamWriter streamWriter = new StreamWriter(logFile, append);
					streamWriter.AutoFlush = true;
					fileWriter = streamWriter;
				}
			}
		}

		public static void Close()
		{
			lock (SyncRoot)
			{
				consoleLevel = TraceLevel.Off;
				fileLevel = TraceLevel.Off;
				if (fileWriter != null)
				{
					fileWriter.Dispose();
					fileWriter = null;
				}
			}
		}

		public static void Debug(string message, params object[] args)
		{
			LogMessage(TraceLevel.Verbose, message, args);
		}

		public static void Info(string message, params object[] args)
		{
			LogMessage(TraceLevel.Info, message, args);
		}

		public static void Warning(string message, params object[] args)
		{
			LogMessage(TraceLevel.Warning, message, args);
		}

		public static void Error(string message, params object[] args)
		{
			LogMessage(TraceLevel.Error, message, args);
		}

		private static void LogMessage(TraceLevel logLevel, string message, params object[] args)
		{
			string message2 = string.Format(CultureInfo.InvariantCulture, message, args);
			string message3 = FormatMessage(logLevel, message2);
			LogToConsole(logLevel, message2);
			LogToTrace(logLevel, message3);
			LogToFile(logLevel, message3);
			if (Logger.OnLogMessage != null)
			{
				Logger.OnLogMessage(null, new LogEventArgs(message3, logLevel));
			}
		}

		private static void LogToConsole(TraceLevel logLevel, string message)
		{
			if (logLevel > consoleLevel)
			{
				return;
			}
			ConsoleColor foregroundColor = Console.ForegroundColor;
			try
			{
				switch (logLevel)
				{
				case TraceLevel.Off:
					break;
				case TraceLevel.Error:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine(message);
					break;
				case TraceLevel.Warning:
					Console.ForegroundColor = ConsoleColor.Yellow;
					goto case TraceLevel.Info;
				case TraceLevel.Info:
				case TraceLevel.Verbose:
					Console.WriteLine(message);
					break;
				default:
					throw new ArgumentOutOfRangeException("logLevel", logLevel, null);
				}
			}
			finally
			{
				Console.ForegroundColor = foregroundColor;
			}
		}

		private static void LogToTrace(TraceLevel logLevel, string message)
		{
			switch (logLevel)
			{
			case TraceLevel.Info:
				Trace.TraceInformation(message);
				break;
			case TraceLevel.Warning:
				Trace.TraceWarning(message);
				break;
			case TraceLevel.Error:
				Trace.TraceError(message);
				break;
			default:
				Trace.WriteLine(message);
				break;
			}
		}

		private static void LogToFile(TraceLevel logLevel, string message)
		{
			if (fileWriter != null && fileLevel > TraceLevel.Off && logLevel <= fileLevel)
			{
				fileWriter.WriteLine(message);
			}
		}

		private static string FormatMessage(TraceLevel logLevel, string message, params object[] args)
		{
			string text = string.Format(CultureInfo.InvariantCulture, "{0:MMM dd HH:mm:ss.ffff} {1}: ", new object[2]
			{
				DateTime.Now,
				logLevel
			});
			if (args != null && args.Any())
			{
				try
				{
					message = string.Format(CultureInfo.InvariantCulture, message, args);
				}
				catch
				{
				}
			}
			return text + message;
		}
	}
}
