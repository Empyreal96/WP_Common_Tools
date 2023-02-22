using System;
using System.Text;
using Microsoft.Diagnostics.Telemetry;
using System.Diagnostics.Tracing;

namespace Microsoft.WindowsPhone.Imaging
{
	internal sealed class ImagingTelemetryLogger
	{
		private static ImagingTelemetryLogger _instance;

		private const string _generalEventName = "Imaging";

		private TelemetryEventSource _logger;

		private readonly EventSourceOptions _telemetryOptionMeasure = TelemetryEventSource.MeasuresOptions();

		public static ImagingTelemetryLogger Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = new ImagingTelemetryLogger();
				}
				return _instance;
			}
		}

		private ImagingTelemetryLogger()
		{
			_logger = new TelemetryEventSource("Microsoft-Windows-Deployment-Imaging");
		}

		public void LogString(string _eventName, Guid sessionId, params string[] values)
		{
			switch (values.Length)
			{
			case 0:
				_logger.Write("Imaging", _telemetryOptionMeasure, new
				{
					EventName = _eventName,
					Value1 = sessionId
				});
				return;
			case 1:
				_logger.Write("Imaging", _telemetryOptionMeasure, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0]
				});
				return;
			case 2:
				_logger.Write("Imaging", _telemetryOptionMeasure, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0],
					Value3 = values[1]
				});
				return;
			case 3:
				_logger.Write("Imaging", _telemetryOptionMeasure, new
				{
					EventName = _eventName,
					Value1 = sessionId,
					Value2 = values[0],
					Value3 = values[1],
					Value4 = values[2]
				});
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string value in values)
			{
				stringBuilder.Append(value);
				stringBuilder.Append(";");
			}
			_logger.Write("Imaging", _telemetryOptionMeasure, new
			{
				EventName = _eventName,
				Value1 = sessionId,
				Value2 = "Values count exceeded supported count.",
				Value3 = stringBuilder.ToString()
			});
		}
	}
}
