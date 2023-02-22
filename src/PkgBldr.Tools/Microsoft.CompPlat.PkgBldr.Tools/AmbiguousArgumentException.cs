using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	[Serializable]
	public class AmbiguousArgumentException : ParseException
	{
		public AmbiguousArgumentException(string id1, string id2)
			: base(string.Format(CultureInfo.InvariantCulture, "Defined arguments '{0}' and '{1}' are ambiguous", new object[2] { id1, id2 }))
		{
		}

		public AmbiguousArgumentException(string id1)
			: base(string.Format(CultureInfo.InvariantCulture, "Defined argument '{0}' is ambiguous", new object[1] { id1 }))
		{
		}

		public AmbiguousArgumentException()
		{
		}

		public AmbiguousArgumentException(string message, Exception except)
			: base("Program error:" + message, except)
		{
		}

		protected AmbiguousArgumentException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
