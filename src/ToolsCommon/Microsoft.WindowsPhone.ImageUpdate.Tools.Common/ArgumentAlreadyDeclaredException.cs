using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[Serializable]
	public class ArgumentAlreadyDeclaredException : ParseException
	{
		public ArgumentAlreadyDeclaredException(string id)
			: base(string.Format(CultureInfo.InvariantCulture, "Argument '{0}' was already defined", new object[1] { id }))
		{
		}

		public ArgumentAlreadyDeclaredException()
		{
		}

		public ArgumentAlreadyDeclaredException(string message, Exception except)
			: base("Program error:" + message, except)
		{
		}

		protected ArgumentAlreadyDeclaredException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
