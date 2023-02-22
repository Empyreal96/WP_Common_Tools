using System;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	public class ConfigXmlException : Exception
	{
		public ConfigXmlException()
		{
		}

		public ConfigXmlException(string message)
			: base(message)
		{
		}

		public ConfigXmlException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
