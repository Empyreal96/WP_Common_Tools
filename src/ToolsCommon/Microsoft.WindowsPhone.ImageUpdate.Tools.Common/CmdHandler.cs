namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public abstract class CmdHandler
	{
		protected CommandLineParser _cmdLineParser = new CommandLineParser();

		public abstract string Command { get; }

		public abstract string Description { get; }

		protected abstract int DoExecution();

		public int Execute(string cmdParams, string applicationName)
		{
			if (!_cmdLineParser.ParseString("appName " + cmdParams, true))
			{
				string appName = applicationName + " " + Command;
				LogUtil.Message(_cmdLineParser.UsageString(appName));
				return -1;
			}
			return DoExecution();
		}
	}
}
