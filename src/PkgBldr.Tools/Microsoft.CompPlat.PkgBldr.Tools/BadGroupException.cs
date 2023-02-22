using System;
using System.Runtime.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	[Serializable]
	public class BadGroupException : ParseException
	{
		public BadGroupException(string message)
			: base(message)
		{
		}

		public BadGroupException()
		{
		}

		public BadGroupException(string message, Exception except)
			: base("Program error:" + message, except)
		{
		}

		protected BadGroupException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
