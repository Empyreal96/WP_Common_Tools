using Microsoft.Phone.Test.TestMetadata.CommandLine;

namespace Microsoft.Phone.Test.TestMetadata.Commands
{
	[Command("help", BriefDescription = "Show help for commands.", BriefUsage = "help <command name>", AllowNoNameOptions = true)]
	[CommandAlias("/?")]
	[CommandAlias("-?")]
	internal class HelpCommand : HelpCommandBase
	{
	}
}
