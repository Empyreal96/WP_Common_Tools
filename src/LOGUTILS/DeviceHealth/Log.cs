using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace DeviceHealth
{
	public class Log
	{
		public enum Level : ushort
		{
			NONE = 255,
			FATAL = 128,
			ERROR = 64,
			WARNING = 32,
			INFO = 16,
			TRACE = 8,
			VERBOSE = 4
		}

		private static Level s_level = Level.INFO;

		private static string s_LogSource = "LOGUTILS";

		private static string s_LogName = "Test Log";

		private static EventLog s_eventLog = RefreshEventLogSettings();

		private static bool s_fEventLog = false;

		private static bool s_fPrefix = false;

		public static Level Verbosity
		{
			get
			{
				return s_level;
			}
			set
			{
				s_level = value;
			}
		}

		public static string Source
		{
			get
			{
				return s_LogSource;
			}
			set
			{
				s_LogSource = value;
				RefreshEventLogSettings();
			}
		}

		public static string Name
		{
			get
			{
				return s_LogName;
			}
			set
			{
				s_LogName = value;
				RefreshEventLogSettings();
			}
		}

		public static bool EventLog
		{
			get
			{
				return s_fEventLog;
			}
			set
			{
				s_fEventLog = value;
			}
		}

		public static bool Prefix
		{
			get
			{
				return s_fPrefix;
			}
			set
			{
				s_fPrefix = value;
			}
		}

		private static EventLog RefreshEventLogSettings()
		{
			if (s_eventLog != null)
			{
				s_eventLog.Close();
			}
			s_eventLog = new EventLog(s_LogName, ".", s_LogSource);
			return s_eventLog;
		}

		public static bool SetVerbosityLevel(string strVerbosity)
		{
			bool flag = false;
			Regex regex = new Regex("^n(one)?$", RegexOptions.IgnoreCase);
			Regex regex2 = new Regex("^f(atal)?$", RegexOptions.IgnoreCase);
			Regex regex3 = new Regex("^e(rr(or)?)?$", RegexOptions.IgnoreCase);
			Regex regex4 = new Regex("^w(arn(ing)?)?$", RegexOptions.IgnoreCase);
			Regex regex5 = new Regex("^info(rmation)?$", RegexOptions.IgnoreCase);
			Regex regex6 = new Regex("^t(race)?$", RegexOptions.IgnoreCase);
			Regex regex7 = new Regex("^v(erbose)?$", RegexOptions.IgnoreCase);
			if (regex.IsMatch(strVerbosity.Trim()))
			{
				s_level = Level.NONE;
				return true;
			}
			if (regex2.IsMatch(strVerbosity.Trim()))
			{
				s_level = Level.FATAL;
				return true;
			}
			if (regex3.IsMatch(strVerbosity.Trim()))
			{
				s_level = Level.ERROR;
				return true;
			}
			if (regex4.IsMatch(strVerbosity.Trim()))
			{
				s_level = Level.WARNING;
				return true;
			}
			if (regex5.IsMatch(strVerbosity.Trim()))
			{
				s_level = Level.INFO;
				return true;
			}
			if (regex6.IsMatch(strVerbosity.Trim()))
			{
				s_level = Level.TRACE;
				return true;
			}
			if (regex7.IsMatch(strVerbosity.Trim()))
			{
				s_level = Level.VERBOSE;
				return true;
			}
			s_level = Level.INFO;
			return false;
		}

		private static void logfn(EventLogEntryType type, string strPrefix, string strMsg)
		{
			if (s_fEventLog)
			{
				try
				{
					s_eventLog.WriteEntry(strMsg, type);
				}
				catch (Exception arg)
				{
					Console.WriteLine("Log.logfn: {0}", arg);
				}
			}
			strMsg = ((!s_fPrefix) ? $"{strMsg}" : $"{s_LogSource} - {strPrefix}: {strMsg}");
			Console.WriteLine(strMsg);
		}

		private static void LogInternal(Level level, EventLogEntryType type, string strPrefix, params object[] args)
		{
			if ((int)level >= (int)s_level && args.Length != 0)
			{
				if (1 == args.Length)
				{
					logfn(type, strPrefix, string.Format(args[0].ToString()));
					return;
				}
				object[] array = new object[args.Length - 1];
				Array.Copy(args, 1, array, 0, args.Length - 1);
				logfn(type, strPrefix, string.Format(args[0].ToString(), array));
			}
		}

		public static void Fatal(params object[] args)
		{
			LogInternal(Level.FATAL, EventLogEntryType.Error, "FATAL ERROR", args);
		}

		public static void Error(params object[] args)
		{
			LogInternal(Level.ERROR, EventLogEntryType.Error, "ERROR", args);
		}

		public static void Warning(params object[] args)
		{
			LogInternal(Level.WARNING, EventLogEntryType.Warning, "WARNING", args);
		}

		public static void Info(params object[] args)
		{
			LogInternal(Level.INFO, EventLogEntryType.Information, "INFO", args);
		}

		public static void Trace(params object[] args)
		{
			LogInternal(Level.TRACE, EventLogEntryType.Information, "TRACE", args);
		}

		public static void Verbose(params object[] args)
		{
			LogInternal(Level.VERBOSE, EventLogEntryType.Information, "VERBOSE", args);
		}
	}
}
