using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[Serializable]
	public class EmptyArgumentDeclaredException : ParseException
	{
		public EmptyArgumentDeclaredException()
			: base("You cannot define an argument with ID: \"\"")
		{
		}

		public EmptyArgumentDeclaredException(string message)
			: base("You cannot define an argument with ID: " + message)
		{
		}

		public EmptyArgumentDeclaredException(string message, Exception except)
			: base("Program error:" + message, except)
		{
		}

		protected EmptyArgumentDeclaredException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
