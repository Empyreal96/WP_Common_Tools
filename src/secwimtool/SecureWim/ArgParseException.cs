using System;

namespace SecureWim
{
	internal class ArgParseException : Exception
	{
		public UsagePrinter.UsageDelegate PrintUsage { get; private set; }

		public ArgParseException(string message, UsagePrinter.UsageDelegate printUsage)
			: base(message)
		{
			PrintUsage = printUsage;
		}
	}
}
