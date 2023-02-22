using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	[Serializable]
	public class NoSuchArgumentException : ParseException
	{
		public NoSuchArgumentException(string type, string id)
			: base(string.Format(CultureInfo.InvariantCulture, "The {0} '{1}' was not defined", new object[2] { type, id }))
		{
		}

		public NoSuchArgumentException(string id)
			: base(string.Format(CultureInfo.InvariantCulture, "The '{0}' was not defined", new object[1] { id }))
		{
		}

		public NoSuchArgumentException()
		{
		}

		public NoSuchArgumentException(string message, Exception except)
			: base("Program error:" + message, except)
		{
		}

		protected NoSuchArgumentException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
