using System;
using System.Diagnostics.Tracing;

namespace Microsoft.Diagnostics.Telemetry
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
	internal sealed class EventDescriptionAttribute : Attribute
	{
		public string Description { get; set; }

		public EventKeywords Keywords { get; set; }

		public EventLevel Level { get; set; }

		public EventOpcode Opcode { get; set; }

		public EventTags Tags { get; set; }

		public EventActivityOptions ActivityOptions { get; set; }
	}
}
