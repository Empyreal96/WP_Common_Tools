using System;

namespace Microsoft.WindowsPhone.ImageUpdate.Customization
{
	public class CustomizationException : Exception
	{
		public CustomizationException(string message)
			: base(message)
		{
		}

		public CustomizationException(string message, params object[] args)
			: this(string.Format(message, args))
		{
		}

		public CustomizationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
