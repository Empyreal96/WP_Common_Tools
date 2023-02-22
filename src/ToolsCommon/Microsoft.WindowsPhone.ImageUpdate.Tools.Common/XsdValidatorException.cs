using System;
using System.Runtime.Serialization;

namespace Microsoft.WindowsPhone.ImageUpdate.Tools.Common
{
	[Serializable]
	public class XsdValidatorException : Exception
	{
		public XsdValidatorException()
		{
		}

		public XsdValidatorException(string message)
			: base(message)
		{
		}

		public XsdValidatorException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected XsdValidatorException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public override string ToString()
		{
			string text = Message;
			if (base.InnerException != null)
			{
				text += base.InnerException.ToString();
			}
			return text;
		}
	}
}
