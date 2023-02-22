using System;

namespace Microsoft.Diagnostics.Telemetry
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
	internal sealed class EventProviderAttribute : Attribute
	{
		public const string TelemetryGroupId = "{4f50731a-89cf-4782-b3e0-dce8c90476ba}";

		public string Provider { get; private set; }

		public EventProviderAttribute(string providerName)
		{
			Provider = providerName;
		}
	}
}
