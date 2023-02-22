using System;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.CommandLine
{
	[AttributeUsage(AttributeTargets.Field)]
	public class DefaultArgumentAttribute : ArgumentAttribute
	{
		public DefaultArgumentAttribute(ArgumentType type)
			: base(type)
		{
		}
	}
}
