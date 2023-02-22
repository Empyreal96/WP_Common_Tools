using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[Serializable]
	public class ParseFailedException : ParseException
	{
		public ParseFailedException(string message)
			: base(message)
		{
		}

		public ParseFailedException()
		{
		}

		public ParseFailedException(string message, Exception except)
			: base("Program error:" + message, except)
		{
		}

		protected ParseFailedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
