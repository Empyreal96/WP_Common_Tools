using System;

namespace SecureWim
{
	internal class UsagePrinter : IToolCommand
	{
		public delegate void UsageDelegate();

		private UsageDelegate printUsage;

		private string failureMessage;

		public UsagePrinter(UsageDelegate usageToPrint)
		{
			printUsage = usageToPrint;
		}

		public UsagePrinter(string failureMessage, UsageDelegate usageToPrint)
		{
			printUsage = usageToPrint;
			this.failureMessage = failureMessage;
		}

		public int Run()
		{
			if (failureMessage != null)
			{
				Console.WriteLine(failureMessage);
			}
			printUsage();
			if (failureMessage != null)
			{
				return -1;
			}
			return 0;
		}
	}
}
