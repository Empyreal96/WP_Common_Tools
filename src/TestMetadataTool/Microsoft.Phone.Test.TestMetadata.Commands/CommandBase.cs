using Microsoft.Phone.Test.TestMetadata.CommandLine;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	internal abstract class CommandBase : Command
	{
		protected void PrintError(string message)
		{
			Log.Error(message);
			throw new UsageException(message)
			{
				Command = this
			};
		}
	}
}
