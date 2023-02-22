using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[Serializable]
	public class ParseException : IUException
	{
		public ParseException(string message)
			: base("Program error:" + message)
		{
		}

		public ParseException()
		{
		}

		public ParseException(string message, Exception except)
			: base(except, "Program error:" + message)
		{
		}

		protected ParseException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
