namespace Microsoft.MobileDevices.TestInfrastructure.UtilityLibrary.CommandLine
{
	public interface IArgumentHolder
	{
		string GetUsageString();

		void ValidateArguments();
	}
}
