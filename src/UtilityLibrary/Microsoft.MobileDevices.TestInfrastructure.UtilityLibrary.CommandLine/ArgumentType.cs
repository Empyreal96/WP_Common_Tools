using System;

namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.CommandLine
{
	[Flags]
	public enum ArgumentType
	{
		Required = 1,
		Unique = 2,
		Multiple = 4,
		Hidden = 8,
		AtMostOnce = 0,
		LastOccurenceWins = 4,
		MultipleUnique = 6
	}
}
