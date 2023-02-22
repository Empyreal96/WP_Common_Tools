using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	public static class LogUtil
	{
		private enum MessageLevel
		{
			Error,
			Warning,
			Message,
			Debug
		}

		public delegate void InteropLogString([MarshalAs(UnmanagedType.LPWStr)] string outputStr);

		private static bool _verbose;

		private static Dictionary<MessageLevel, string> _msgPrefix;

		private static Dictionary<MessageLevel, ConsoleColor> _msgColor;

		private static InteropLogString _iuErrorLogger;

		private static InteropLogString _iuWarningLogger;

		private static InteropLogString _iuMsgLogger;

		private static InteropLogString _iuDebugLogger;

		[DllImport("UpdateDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void IU_LogTo([MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString ErrorMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString WarningMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString InfoMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString DebugMsgHandler);

		[DllImport("ConvertDSMDLL.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto, SetLastError = true)]
		private static extern void ConvertDSM_LogTo([MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString ErrorMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString WarningMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString InfoMsgHandler, [MarshalAs(UnmanagedType.FunctionPtr)] InteropLogString DebugMsgHandler);

		static LogUtil()
		{
			_verbose = false;
			_msgPrefix = new Dictionary<MessageLevel, string>();
			_msgColor = new Dictionary<MessageLevel, ConsoleColor>();
			_iuErrorLogger = null;
			_iuWarningLogger = null;
			_iuMsgLogger = null;
			_iuDebugLogger = null;
			_msgPrefix.Add(MessageLevel.Debug, "diagnostic ");
			_msgPrefix.Add(MessageLevel.Message, "info");
			_msgPrefix.Add(MessageLevel.Warning, "warning ");
			_msgPrefix.Add(MessageLevel.Error, "fatal error PKG");
			_msgColor.Add(MessageLevel.Debug, ConsoleColor.DarkGray);
			_msgColor.Add(MessageLevel.Message, ConsoleColor.Gray);
			_msgColor.Add(MessageLevel.Warning, ConsoleColor.Yellow);
			_msgColor.Add(MessageLevel.Error, ConsoleColor.Red);
			IULogTo(Error, Warning, Message, Diagnostic);
		}

		private static void LogMessage(MessageLevel level, string message)
		{
			if (level != MessageLevel.Debug || _verbose)
			{
				string[] array = message.Split(new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
				foreach (string arg in array)
				{
					ConsoleColor foregroundColor = Console.ForegroundColor;
					Console.ForegroundColor = _msgColor[level];
					Console.WriteLine("{0}: {1}", _msgPrefix[level], arg);
					Console.ForegroundColor = foregroundColor;
				}
			}
		}

		private static void LogMessage(MessageLevel level, string format, params object[] args)
		{
			LogMessage(level, string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public static void SetVerbosity(bool enabled)
		{
			_verbose = enabled;
		}

		public static void Error(string message)
		{
			LogMessage(MessageLevel.Error, message);
		}

		public static void Error(string format, params object[] args)
		{
			LogMessage(MessageLevel.Error, format, args);
		}

		public static void Warning(string message)
		{
			message = "PKG-W: " + message;
			LogMessage(MessageLevel.Warning, message);
		}

		public static void Warning(string format, params object[] args)
		{
			format = "PKG-W: " + format;
			LogMessage(MessageLevel.Warning, format, args);
		}

		public static void Message(string msg)
		{
			LogMessage(MessageLevel.Message, msg);
		}

		public static void Message(string format, params object[] args)
		{
			LogMessage(MessageLevel.Message, format, args);
		}

		public static void Diagnostic(string message)
		{
			LogMessage(MessageLevel.Debug, message);
		}

		public static void Diagnostic(string format, params object[] args)
		{
			LogMessage(MessageLevel.Debug, format, args);
		}

		public static void LogCopyright()
		{
			Console.WriteLine(CommonUtils.GetCopyrightString());
		}

		public static void IULogTo(InteropLogString errorLogger, InteropLogString warningLogger, InteropLogString msgLogger, InteropLogString debugLogger)
		{
			_iuErrorLogger = errorLogger;
			_iuWarningLogger = warningLogger;
			_iuMsgLogger = msgLogger;
			_iuDebugLogger = debugLogger;
			IU_LogTo(errorLogger, warningLogger, msgLogger, debugLogger);
			ConvertDSM_LogTo(errorLogger, warningLogger, msgLogger, debugLogger);
		}
	}
}
