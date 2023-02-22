using System;
using System.Runtime.Serialization;

namespace Microsoft.CompPlat.PkgBldr.Tools
{
	[Serializable]
	public class RequiredParameterAfterOptionalParameterException : ParseException
	{
		public RequiredParameterAfterOptionalParameterException()
			: base("An optional parameter can't be followed by a required one")
		{
		}

		public RequiredParameterAfterOptionalParameterException(string message)
			: base("Program error:" + message)
		{
		}

		public RequiredParameterAfterOptionalParameterException(string message, Exception except)
			: base("Program error:" + message, except)
		{
		}

		protected RequiredParameterAfterOptionalParameterException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
