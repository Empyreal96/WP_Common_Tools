using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.Windows.ImageTools
{
	internal class EtwSession : IDisposable
	{
		private string sessionName;

		private ulong traceHandle;

		public string EtlPath { get; private set; }

		public EtwSession()
			: this(true)
		{
		}

		public EtwSession(bool log)
		{
			if (log)
			{
				sessionName = Process.GetCurrentProcess().ProcessName + Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
				EtlPath = Path.Combine(Path.GetTempPath(), sessionName + ".etl");
				traceHandle = ulong.MaxValue;
				StartTracing();
			}
			else
			{
				sessionName = string.Empty;
				EtlPath = string.Empty;
				traceHandle = ulong.MaxValue;
			}
		}

		public void Dispose()
		{
			if (traceHandle != ulong.MaxValue)
			{
				EventTraceProperties eventTraceProperties;
				NativeMethods.StopTrace(traceHandle, sessionName, out eventTraceProperties);
				traceHandle = ulong.MaxValue;
			}
		}

		private void StartTracing()
		{
			EventTraceProperties eventTraceProperties = EventTraceProperties.CreateProperties(sessionName, EtlPath, LoggingModeConstant.PrivateLoggerMode | LoggingModeConstant.PrivateInProc);
			int num = NativeMethods.StartTrace(out traceHandle, sessionName, ref eventTraceProperties);
			if (num != 0)
			{
				throw new Win32Exception(num);
			}
			Guid guid = new Guid("3bbd891e-180f-4386-94b5-d71ba7ac25a9");
			Guid guid2 = new Guid("fb961307-bc64-4de4-8828-81d583524da0");
			Guid[] array = new Guid[2] { guid, guid2 };
			foreach (Guid providerId in array)
			{
				num = NativeMethods.EnableTraceEx2(traceHandle, providerId, 1u, TraceLevel.Verbose, ulong.MaxValue, 0uL);
				if (num != 0)
				{
					throw new Win32Exception(num);
				}
			}
		}
	}
}
