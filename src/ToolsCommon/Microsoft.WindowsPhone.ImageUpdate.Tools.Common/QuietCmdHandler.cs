namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	public abstract class QuietCmdHandler : CmdHandler
	{
		private const string QUIET_SWITCH_NAME = "quiet";

		protected void SetLoggingVerbosity(IULogger logger)
		{
			if (_cmdLineParser.GetSwitchAsBoolean("quiet"))
			{
				logger.SetLoggingLevel(LoggingLevel.Warning);
			}
		}

		protected void SetQuietCommand()
		{
			_cmdLineParser.SetOptionalSwitchBoolean("quiet", "When set only errors and warnings will be logged.", false);
		}
	}
}
