using System;
using System.Globalization;
using System.Threading;
using System.Diagnostics.Tracing;

namespace Microsoft.Diagnostics.Telemetry
{
	internal class TelemetryEventSource : EventSource
	{
		private class EventDescriptionInfo<T>
		{
			private static EventDescriptionInfo<T> instance;

			public EventSourceOptions Options;

			private EventDescriptionInfo(Type type)
			{
				Type typeFromHandle = typeof(T);
				EventDescriptionAttribute eventDescriptionAttribute = null;
				object[] customAttributes = typeFromHandle.GetCustomAttributes(typeof(EventDescriptionAttribute), false);
				int num = 0;
				if (num < customAttributes.Length)
				{
					object obj = customAttributes[num];
					eventDescriptionAttribute = obj as EventDescriptionAttribute;
				}
				if (eventDescriptionAttribute == null)
				{
					throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, "WriteTelemetry requires the data type {0} to have an {1} attribute", new object[2]
					{
						typeof(T).Name,
						typeof(EventDescriptionAttribute).Name
					}));
				}
				Options = new EventSourceOptions
				{
					Keywords = eventDescriptionAttribute.Keywords,
					Level = eventDescriptionAttribute.Level,
					Opcode = eventDescriptionAttribute.Opcode,
					Tags = eventDescriptionAttribute.Tags,
					ActivityOptions = eventDescriptionAttribute.ActivityOptions
				};
			}

			public static EventDescriptionInfo<T> GetInstance()
			{
				if (instance == null)
				{
					EventDescriptionInfo<T> value = new EventDescriptionInfo<T>(typeof(T));
					Interlocked.CompareExchange(ref instance, value, null);
				}
				return instance;
			}
		}

		public const EventKeywords Reserved44Keyword = (EventKeywords)17592186044416L;

		public const EventKeywords TelemetryKeyword = (EventKeywords)35184372088832L;

		public const EventKeywords MeasuresKeyword = (EventKeywords)70368744177664L;

		public const EventKeywords CriticalDataKeyword = (EventKeywords)140737488355328L;

		public const EventTags CostDeferredLatency = (EventTags)262144;

		public const EventTags CoreData = (EventTags)524288;

		public const EventTags InjectXToken = (EventTags)1048576;

		public const EventTags RealtimeLatency = (EventTags)2097152;

		public const EventTags NormalLatency = (EventTags)4194304;

		public const EventTags CriticalPersistence = (EventTags)8388608;

		public const EventTags NormalPersistence = (EventTags)16777216;

		public const EventTags DropPii = (EventTags)33554432;

		public const EventTags HashPii = (EventTags)67108864;

		public const EventTags MarkPii = (EventTags)134217728;

		public const EventFieldTags DropPiiField = (EventFieldTags)67108864;

		public const EventFieldTags HashPiiField = (EventFieldTags)134217728;

		private static readonly string[] telemetryTraits = new string[2] { "ETW_GROUP", "{4f50731a-89cf-4782-b3e0-dce8c90476ba}" };

		public TelemetryEventSource(string eventSourceName)
			: base(eventSourceName, EventSourceSettings.EtwSelfDescribingEventFormat, telemetryTraits)
		{
		}

		protected TelemetryEventSource()
			: base(EventSourceSettings.EtwSelfDescribingEventFormat, telemetryTraits)
		{
		}

		public static EventSourceOptions TelemetryOptions()
		{
			EventSourceOptions result = default(EventSourceOptions);
			result.Keywords = (EventKeywords)35184372088832L;
			return result;
		}

		public static EventSourceOptions MeasuresOptions()
		{
			EventSourceOptions result = default(EventSourceOptions);
			result.Keywords = (EventKeywords)70368744177664L;
			return result;
		}

		public static EventSourceOptions CriticalDataOptions()
		{
			EventSourceOptions result = default(EventSourceOptions);
			result.Keywords = (EventKeywords)140737488355328L;
			return result;
		}

		[NonEvent]
		public void WriteTelemetry<T>(T data)
		{
			if (IsEnabled())
			{
				Write(null, ref EventDescriptionInfo<T>.GetInstance().Options, ref data);
			}
		}

		[NonEvent]
		public void WriteTelemetry<T>(ref Guid activityId, ref Guid relatedActivityId, ref T data)
		{
			if (IsEnabled())
			{
				Write(null, ref EventDescriptionInfo<T>.GetInstance().Options, ref activityId, ref relatedActivityId, ref data);
			}
		}

		public static EventSourceOptions GetEventSourceOptionsForType<T>()
		{
			return EventDescriptionInfo<T>.GetInstance().Options;
		}
	}
}
