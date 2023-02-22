using System;
using System.ComponentModel;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary
{
	public class EtwSession : IDisposable
	{
		public const int MaxPrivateLoggingSession = 3;

		private string sessionName;

		private ulong traceHandle;

		public string EtlPath { get; private set; }

		public EtwSession(string sessionName, string path)
		{
			traceHandle = ulong.MaxValue;
			this.sessionName = sessionName;
			EtlPath = path;
		}

		public void Dispose()
		{
			StopTracing();
		}

		public void StartTracing()
		{
			EventTraceProperties eventTraceProperties = EventTraceProperties.CreateProperties(sessionName, EtlPath, LoggingModeConstant.PrivateLoggerMode | LoggingModeConstant.PrivateInProc);
			int num = NativeMethods.StartTrace(out traceHandle, sessionName, ref eventTraceProperties);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
		}

		public void AddProvider(Guid providerGuid)
		{
			if (traceHandle != ulong.MaxValue)
			{
				int num = NativeMethods.EnableTraceEx2(traceHandle, providerGuid, 1u, TraceLevel.Verbose, ulong.MaxValue, 0uL);
				if (num != 0)
				{
					throw new Win32Exception(num);
				}
				return;
			}
			throw new InvalidOperationException("AddProvider requires the etw session to be started");
		}

		public void StopTracing()
		{
			if (traceHandle != ulong.MaxValue)
			{
				EventTraceProperties Properties = EventTraceProperties.CreateProperties(sessionName, null, (LoggingModeConstant)0u);
				int num = NativeMethods.ControlTrace(traceHandle, null, ref Properties, 1u);
				traceHandle = ulong.MaxValue;
				if (num != 0)
				{
					throw new Win32Exception(num);
				}
			}
		}
	}
}
