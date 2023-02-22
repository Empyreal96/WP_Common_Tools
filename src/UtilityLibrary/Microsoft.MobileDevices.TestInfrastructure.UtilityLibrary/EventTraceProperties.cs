using System;
using System.Runtime.InteropServices;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct EventTraceProperties
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct WNodeHeader
		{
			public uint BufferSize;

			public uint ProviderId;

			public ulong HistoricalContext;

			public long TimeStamp;

			public Guid Guid;

			public uint ClientContext;

			public uint Flags;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		internal struct EventTracePropertiesCore
		{
			public WNodeHeader Wnode;

			public uint BufferSize;

			public uint MinimumBuffers;

			public uint MaximumBuffers;

			public uint MaximumFileSize;

			public LoggingModeConstant LogFileMode;

			public uint FlushTimer;

			public uint EnableFlags;

			public int AgeLimit;

			public uint NumberOfBuffers;

			public uint FreeBuffers;

			public uint EventsLost;

			public uint BuffersWritten;

			public uint LogBuffersLost;

			public uint RealTimeBuffersLost;

			public IntPtr LoggerThreadId;

			public uint LogFileNameOffset;

			public uint LoggerNameOffset;
		}

		public const int MaxLoggerNameLength = 1024;

		public EventTracePropertiesCore CoreProperties;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		private string loggerName;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
		private string logFileName;

		internal static EventTraceProperties CreateProperties(string sessionName, string logFilePath, LoggingModeConstant logMode)
		{
			uint bufferSize = (uint)Marshal.SizeOf(typeof(EventTraceProperties));
			EventTraceProperties result = default(EventTraceProperties);
			result.CoreProperties.Wnode = default(WNodeHeader);
			result.CoreProperties.Wnode.BufferSize = bufferSize;
			result.CoreProperties.Wnode.Flags = 131072u;
			result.CoreProperties.Wnode.Guid = Guid.NewGuid();
			result.CoreProperties.BufferSize = 64u;
			result.CoreProperties.MinimumBuffers = 5u;
			result.CoreProperties.MaximumBuffers = 200u;
			result.CoreProperties.FlushTimer = 0u;
			result.CoreProperties.LogFileMode = logMode;
			if (logFilePath != null && logFilePath.Length < 1024)
			{
				result.logFileName = logFilePath;
			}
			result.CoreProperties.LogFileNameOffset = (uint)(int)Marshal.OffsetOf(typeof(EventTraceProperties), "logFileName");
			if (sessionName != null && sessionName.Length < 1024)
			{
				result.loggerName = sessionName;
			}
			result.CoreProperties.LoggerNameOffset = (uint)(int)Marshal.OffsetOf(typeof(EventTraceProperties), "loggerName");
			return result;
		}

		internal static EventTraceProperties CreateProperties()
		{
			return CreateProperties(null, null, (LoggingModeConstant)0u);
		}
	}
}
