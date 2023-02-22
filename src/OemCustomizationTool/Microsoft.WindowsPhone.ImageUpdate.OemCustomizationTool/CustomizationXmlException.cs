using System;

namespace Microsoft.WindowsPhone.ImageUpdate.OemCustomizationTool
{
	public class CustomizationXmlException : Exception
	{
		public CustomizationXmlException()
		{
		}

		public CustomizationXmlException(string message)
			: base(message)
		{
		}

		public CustomizationXmlException(string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}
