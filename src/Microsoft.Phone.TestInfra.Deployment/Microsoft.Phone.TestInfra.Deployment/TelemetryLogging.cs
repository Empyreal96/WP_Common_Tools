using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Diagnostics.Telemetry;
using System.Diagnostics.Tracing;

namespace Microsoft.Phone.TestInfra.Deployment
{
	public static class TelemetryLogging
	{
		[Description("Telemetry Logging type")]
		public enum TelemetryLogType
		{
			Unknown,
			LogEvent,
			LogError
		}

		[Description("The spkg deployment events")]
		public enum SpkgDeployTelemetryEventType
		{
			Unknown,
			SpkgDeployStart,
			SpkgDeployFinished,
			SpkgErrorOccurred
		}

		public static readonly EventSourceOptions MeasuresOption = new EventSourceOptions
		{
			Keywords = (EventKeywords)70368744177664L
		};

		public static readonly EventSourceOptions TelemetryInfoOption = new EventSourceOptions
		{
			Keywords = (EventKeywords)35184372088832L,
			Level = EventLevel.Informational,
			Opcode = EventOpcode.Info
		};

		public static readonly EventSourceOptions TelemetryErrorOption = new EventSourceOptions
		{
			Keywords = (EventKeywords)35184372088832L,
			Level = EventLevel.Error,
			Opcode = EventOpcode.Info
		};

		public static readonly EventSourceOptions TelemetryStartOption = new EventSourceOptions
		{
			Keywords = (EventKeywords)35184372088832L,
			Opcode = EventOpcode.Start
		};

		public static readonly EventSourceOptions TelemetryStopOption = new EventSourceOptions
		{
			Keywords = (EventKeywords)35184372088832L,
			Opcode = EventOpcode.Stop
		};

		private static EventSource log;

		private static string telemetryProvider = "Microsoft.OSG.QBI.SpkgDeploy";

		public static EventSource Log
		{
			get
			{
				if (log == null)
				{
					Instance();
				}
				return log;
			}
		}

		public static EventSource Instance(string providersName = null)
		{
			if (providersName != null)
			{
				telemetryProvider = providersName;
			}
			if (log == null)
			{
				log = new TelemetryEventSource(telemetryProvider);
			}
			return log;
		}

		public static string EnumToString(this Enum eff)
		{
			return Enum.GetName(eff.GetType(), eff);
		}

		public static void LogEvent(SpkgDeployTelemetryEventType eventType, IEnumerable<string> packages, IEnumerable<string> rootPath, IEnumerable<string> altRootPath, string message, DateTime time)
		{
			Log.Write(eventType.EnumToString(), TelemetryInfoOption, new
			{
				MachineName = Environment.MachineName,
				EventTime = time.ToString(),
				Message = (message ?? string.Empty),
				Packages = ((packages != null && packages.Any()) ? string.Join(";", packages) : string.Empty),
				RootPath = ((rootPath != null && rootPath.Any()) ? string.Join(";", rootPath) : string.Empty),
				AltRootPath = ((altRootPath != null && altRootPath.Any()) ? string.Join(";", altRootPath) : string.Empty)
			});
		}

		public static void LogEvent(SpkgDeployTelemetryEventType eventType, string message)
		{
			log.Write(eventType.EnumToString(), TelemetryInfoOption, new
			{
				MachineName = Environment.MachineName,
				Message = message
			});
		}

		public static void LogError(SpkgDeployTelemetryEventType eventType, string errorMessage)
		{
			log.Write(eventType.EnumToString(), TelemetryErrorOption, new
			{
				MachineName = Environment.MachineName,
				ExceptionMessage = errorMessage
			});
		}
	}
}
