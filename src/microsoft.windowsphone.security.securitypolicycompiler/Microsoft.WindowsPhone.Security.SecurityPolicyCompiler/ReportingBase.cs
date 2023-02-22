namespace Microsoft.WindowsPhone.Security.SecurityPolicyCompiler
{
	public abstract class ReportingBase
	{
		private static bool useInternalLogger = true;

		private static bool enableDebugMessage = true;

		private static volatile ReportingBase instance;

		private static object syncRoot = new object();

		public static bool UseInternalLogger
		{
			get
			{
				return useInternalLogger;
			}
			set
			{
				useInternalLogger = value;
			}
		}

		public static bool EnableDebugMessage
		{
			get
			{
				return enableDebugMessage;
			}
			set
			{
				enableDebugMessage = value;
			}
		}

		public abstract void ErrorLine(string errorMsg);

		public abstract void Debug(string debugMsg);

		public abstract void DebugLine(string debugMsg);

		public abstract void XmlElementLine(string indentation, string element);

		public abstract void XmlAttributeLine(string indentation, string element, string value);

		public static ReportingBase GetInstance()
		{
			if (instance == null)
			{
				lock (syncRoot)
				{
					if (instance == null)
					{
						if (UseInternalLogger)
						{
							instance = new ConsoleReport();
						}
						else
						{
							instance = new LogUtilityReport();
						}
					}
				}
			}
			return instance;
		}
	}
}
